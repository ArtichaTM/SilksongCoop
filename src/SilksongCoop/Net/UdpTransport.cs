using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SilksongCoop.Net;

/// <summary>
/// One UDP socket. A background thread receives into <see cref="Inbox"/>; everything else runs on the
/// main thread. Disposing closes the socket and joins the thread (C2).
/// </summary>
internal sealed class UdpTransport : IDisposable
{
    // Windows reports an ICMP "port unreachable" as a reset on the next receive unless this is off.
    private const int SioUdpConnReset = -1744830452;

    private readonly UdpClient _client;
    private readonly Thread _thread;
    private volatile bool _running = true;

    public UdpTransport(int port)
    {
        _client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
        try
        {
            _client.Client.IOControl(SioUdpConnReset, [0, 0, 0, 0], null);
        }
        catch (NotSupportedException)
        {
            // Not Windows: nothing to switch off.
        }
        catch (SocketException)
        {
            // Not Windows: nothing to switch off.
        }
        _thread = new Thread(ReceiveLoop) { IsBackground = true, Name = "SilksongCoop UDP" };
        _thread.Start();
    }

    public ConcurrentQueue<(IPEndPoint From, byte[] Data, double At)> Inbox { get; } = new();

    public void Send(IPEndPoint to, byte[] data)
    {
        try
        {
            _client.Send(data, data.Length, to);
        }
        catch (SocketException)
        {
            // A dropped datagram is the normal UDP failure; heartbeats notice a dead peer.
        }
        catch (ObjectDisposedException)
        {
            // Shutting down.
        }
    }

    private void ReceiveLoop()
    {
        while (_running)
        {
            try
            {
                IPEndPoint from = new(IPAddress.Any, 0);
                byte[] data = _client.Receive(ref from);
                Inbox.Enqueue((from, data, Clock.Now));
            }
            catch (SocketException)
            {
                // Connection resets and timeouts: keep listening.
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }
    }

    public void Dispose()
    {
        _running = false;
        _client.Close();
        _thread.Join(1000);
    }
}
