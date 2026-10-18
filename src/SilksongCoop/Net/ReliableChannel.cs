using System.Collections.Generic;

namespace SilksongCoop.Net;

/// <summary>
/// Ordered, acknowledged delivery over plain datagrams: resends until acked, delivers in sequence order.
/// Time is passed in so the logic stays testable.
/// </summary>
internal sealed class ReliableChannel
{
    public const int Window = 128;

    private readonly double _resendAfter;
    private readonly SortedDictionary<uint, Pending> _inFlight = [];
    private readonly Queue<byte[]> _waiting = new();
    private readonly Dictionary<uint, byte[]> _early = [];
    private uint _nextSequence;
    private uint _nextExpected;

    public ReliableChannel(double resendAfter = 0.1) => _resendAfter = resendAfter;

    public int Unacked => _inFlight.Count + _waiting.Count;

    public void Send(byte[] message) => _waiting.Enqueue(message);

    /// <summary>Frames to put on the wire now: new ones that fit the window and overdue resends.</summary>
    public List<(uint Sequence, byte[] Message)> Due(double now)
    {
        while (_waiting.Count > 0 && _inFlight.Count < Window)
        {
            _inFlight[_nextSequence++] = new Pending(_waiting.Dequeue());
        }
        List<(uint, byte[])> due = [];
        foreach (KeyValuePair<uint, Pending> entry in _inFlight)
        {
            Pending pending = entry.Value;
            if (pending.LastSent < 0 || now - pending.LastSent >= _resendAfter)
            {
                pending.LastSent = now;
                due.Add((entry.Key, pending.Message));
            }
        }
        return due;
    }

    public void Ack(uint sequence) => _inFlight.Remove(sequence);

    /// <summary>Messages now deliverable in order; none for duplicates and gaps. Always ack.</summary>
    public List<byte[]> Receive(uint sequence, byte[] message)
    {
        List<byte[]> ready = [];
        if (sequence < _nextExpected)
        {
            return ready;
        }
        _early[sequence] = message;
        while (_early.TryGetValue(_nextExpected, out byte[]? next))
        {
            _early.Remove(_nextExpected);
            ready.Add(next);
            _nextExpected++;
        }
        return ready;
    }

    private sealed class Pending(byte[] message)
    {
        public byte[] Message { get; } = message;

        public double LastSent { get; set; } = -1;
    }
}
