using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SilksongCoop.Net;

/// <summary>
/// The connection to the one partner (D10, D26): handshake, heartbeats, timeouts, reliable and fast
/// messages. Datagrams are processed on the main thread in <see cref="Tick"/>; heartbeats go out from a
/// timer so a long scene load on one side doesn't look like a disconnect to the other.
/// </summary>
internal sealed class Session : IDisposable
{
    public const double Timeout = 8;
    private const byte Magic0 = (byte)'S';
    private const byte Magic1 = (byte)'C';
    private const double RejectedLinger = 3;
    private const double AnswerWait = 10;
    private const int SafeDatagram = 1400;

    private readonly UdpTransport _transport;
    private readonly Timer _heartbeat;
    private readonly Credentials _credentials;
    private readonly string _name;
    private readonly Dictionary<MessageType, Action<PacketReader>> _handlers = [];
    private Peer? _peer;
    private double _lastFullReply = -1;
    private readonly double _startedAt = Clock.Now;

    private Session(bool isHost, int port, Credentials credentials, string name)
    {
        IsHost = isHost;
        _credentials = credentials;
        _name = name;
        _transport = new UdpTransport(port);
        _heartbeat = new Timer(_ => Beat(), null, 500, 500);
    }

    public bool IsHost { get; }

    public bool Connected => _peer is { Accepted: true, ClosingAt: null };

    public string PartnerName { get; private set; } = "";

    /// <summary>The partner passed the handshake.</summary>
    public event Action? Joined;

    /// <summary>The partner is gone; the argument says why.</summary>
    public event Action<string>? Left;

    /// <summary>A datagram over <see cref="SafeDatagram"/> bytes: routers may drop it whole.</summary>
    public event Action<MessageType, int>? Oversized;

    /// <summary>A message handler threw; the messages after it are still delivered.</summary>
    public event Action<MessageType, Exception>? HandlerFailed;

    public static Session Host(int port, Credentials credentials, string name) =>
        new(true, port, credentials, name);

    public static Session Join(string address, int port, Credentials credentials, string name)
    {
        IPAddress host = Resolve(address);
        Session session = new(false, 0, credentials, name);
        session._peer = new Peer(new IPEndPoint(host, port));
        session.Send(MessageType.Hello, writer => writer
            .String(credentials.Version)
            .String(credentials.Build)
            .String(credentials.Password)
            .String(name));
        return session;
    }

    public void On(MessageType type, Action<PacketReader> handler) => _handlers[type] = handler;

    /// <summary>Reliable and ordered.</summary>
    public void Send(MessageType type, Action<PacketWriter>? fill = null) =>
        _peer?.Channel.Send(Payload(type, fill));

    /// <summary>Unreliable; only for state the next message replaces.</summary>
    public void SendFast(MessageType type, Action<PacketWriter> body)
    {
        if (Connected)
        {
            SendFrame(_peer!.Endpoint, Framing.Unreliable, Payload(type, body));
        }
    }

    public void Tick()
    {
        double now = Clock.Now;
        while (_transport.Inbox.TryDequeue(out (IPEndPoint From, byte[] Data, double At) datagram))
        {
            Receive(datagram.From, datagram.Data, datagram.At);
        }
        if (_peer is null)
        {
            return;
        }
        foreach ((uint sequence, byte[] message) in _peer.Channel.Due(now))
        {
            SendFrame(_peer.Endpoint, Framing.Reliable, new PacketWriter().UInt(sequence), message);
        }
        if (_peer.ClosingAt is { } closing && now >= closing)
        {
            _peer = null;
        }
        else if (_peer.Accepted && now - _peer.LastHeard > Timeout)
        {
            Drop("connection timed out");
        }
        else if (!IsHost && !_peer.Accepted && now - _startedAt > AnswerWait)
        {
            Drop("the host did not answer");
        }
    }

    /// <summary>Tells the partner we are leaving, best effort, and forgets them.</summary>
    public void Close(string reason)
    {
        if (_peer is { } peer)
        {
            SendFrame(peer.Endpoint, Framing.Unreliable, Payload(MessageType.Goodbye, w => w.String(reason)));
        }
        _peer = null;
    }

    public void Dispose()
    {
        Close("partner quit");
        _heartbeat.Dispose();
        _transport.Dispose();
    }

    private static IPAddress Resolve(string address)
    {
        if (IPAddress.TryParse(address, out IPAddress? literal))
        {
            return literal;
        }
        IPAddress[] found = Dns.GetHostAddresses(address);
        IPAddress? v4 = found.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        return v4 ?? throw new IOException($"{address} has no IPv4 address");
    }

    private static byte[] Payload(MessageType type, Action<PacketWriter>? body)
    {
        PacketWriter writer = new PacketWriter().Byte((byte)type);
        body?.Invoke(writer);
        return writer.ToArray();
    }

    private void Beat()
    {
        if (Connected && _peer is { } peer)
        {
            SendFrame(peer.Endpoint, Framing.Unreliable, Payload(MessageType.Heartbeat, null));
        }
    }

    private void SendFrame(IPEndPoint to, Framing kind, byte[] payload) => SendFrame(to, kind, null, payload);

    private void SendFrame(IPEndPoint to, Framing framing, PacketWriter? header, byte[] payload)
    {
        byte[] head = header?.ToArray() ?? [];
        byte[] frame = new byte[3 + head.Length + payload.Length];
        frame[0] = Magic0;
        frame[1] = Magic1;
        frame[2] = (byte)framing;
        head.CopyTo(frame, 3);
        payload.CopyTo(frame, 3 + head.Length);
        if (frame.Length > SafeDatagram && payload.Length > 0)
        {
            Oversized?.Invoke((MessageType)payload[0], frame.Length);
        }
        _transport.Send(to, frame);
    }

    private void Receive(IPEndPoint from, byte[] data, double at)
    {
        if (data.Length < 3 || data[0] != Magic0 || data[1] != Magic1)
        {
            return;
        }
        Peer? peer = PeerFor(from, at);
        if (peer is null)
        {
            return;
        }
        peer.LastHeard = Math.Max(peer.LastHeard, at);
        try
        {
            Unframe(peer, (Framing)data[2], new PacketReader(data, 3));
        }
        catch (EndOfStreamException)
        {
            // Truncated or foreign datagram: ignore it.
        }
    }

    private Peer? PeerFor(IPEndPoint from, double at)
    {
        if (_peer is { } peer && peer.Endpoint.Equals(from))
        {
            return peer;
        }
        if (!IsHost)
        {
            return null;
        }
        bool busy = _peer is { Accepted: true, ClosingAt: null } && at - _peer.LastHeard < Timeout;
        if (!busy)
        {
            _peer = new Peer(from) { LastHeard = at };
            return _peer;
        }
        if (at - _lastFullReply > 1)
        {
            _lastFullReply = at;
            byte[] full = Payload(MessageType.Reject, w => w.String("the host already has a partner"));
            SendFrame(from, Framing.Unreliable, full);
        }
        return null;
    }

    private void Unframe(Peer peer, Framing framing, PacketReader reader)
    {
        switch (framing)
        {
            case Framing.Ack:
                peer.Channel.Ack(reader.UInt());
                break;
            case Framing.Unreliable:
                Dispatch(peer, (MessageType)reader.Byte(), reader);
                break;
            case Framing.Reliable:
                uint sequence = reader.UInt();
                SendFrame(peer.Endpoint, Framing.Ack, new PacketWriter().UInt(sequence).ToArray());
                foreach (byte[] message in peer.Channel.Receive(sequence, reader.Rest()))
                {
                    PacketReader body = new(message);
                    Dispatch(peer, (MessageType)body.Byte(), body);
                }
                break;
        }
    }

    private void Dispatch(Peer peer, MessageType type, PacketReader body)
    {
        switch (type)
        {
            case MessageType.Hello when IsHost && !peer.Accepted:
                Admit(peer, body);
                return;
            case MessageType.Welcome when !IsHost:
                peer.Accepted = true;
                PartnerName = body.String();
                Joined?.Invoke();
                return;
            case MessageType.Reject when !IsHost:
                Drop(body.String());
                return;
            case MessageType.Goodbye:
                Drop(body.String());
                return;
            case MessageType.Heartbeat:
                return;
        }
        bool open = peer.Accepted && peer.ClosingAt is null;
        if (!open || !_handlers.TryGetValue(type, out Action<PacketReader>? handler))
        {
            return;
        }
        try
        {
            handler(body);
        }
        catch (Exception exception)
        {
            // A reliable delivery may carry several messages; one failing must not lose the rest.
            HandlerFailed?.Invoke(type, exception);
        }
    }

    private void Admit(Peer peer, PacketReader hello)
    {
        Credentials client = new(hello.String(), hello.String(), hello.String());
        string name = hello.String();
        string? reason = Handshake.Reject(_credentials, client);
        if (reason is not null)
        {
            peer.Channel.Send(Payload(MessageType.Reject, w => w.String(reason)));
            peer.ClosingAt = Clock.Now + RejectedLinger;
            return;
        }
        peer.Accepted = true;
        PartnerName = name;
        Send(MessageType.Welcome, w => w.String(_name));
        Joined?.Invoke();
    }

    private void Drop(string reason)
    {
        bool wasConnected = Connected || !IsHost;
        _peer = null;
        if (wasConnected)
        {
            Left?.Invoke(reason);
        }
    }

    private sealed class Peer(IPEndPoint endpoint)
    {
        public IPEndPoint Endpoint { get; } = endpoint;

        public ReliableChannel Channel { get; } = new();

        public double LastHeard { get; set; } = Clock.Now;

        public bool Accepted { get; set; }

        public double? ClosingAt { get; set; }
    }
}
