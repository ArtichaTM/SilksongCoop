using SilksongCoop.Net;

namespace SilksongCoop.Tests;

public class ReliableChannelTests
{
    private static byte[] Message(byte value) => [value];

    [Fact]
    public void NewMessagesGoOutOnceUntilResendIsDue()
    {
        ReliableChannel channel = new(resendAfter: 0.1);
        channel.Send(Message(1));
        Assert.Single(channel.Due(0));
        Assert.Empty(channel.Due(0.05));
        Assert.Single(channel.Due(0.1));
    }

    [Fact]
    public void ResendWaitDoublesUpToTheCap()
    {
        // Binary fractions, so the waits add up exactly.
        ReliableChannel channel = new(resendAfter: 0.125);
        channel.Send(Message(1));
        Assert.Single(channel.Due(0));
        double[] resends = [0.125, 0.375, 0.875, 1.875, 2.875];
        foreach (double at in resends)
        {
            Assert.Empty(channel.Due(at - 0.01));
            Assert.Single(channel.Due(at));
        }
    }

    [Fact]
    public void AckedMessagesStopResending()
    {
        ReliableChannel channel = new(resendAfter: 0.1);
        channel.Send(Message(1));
        uint sequence = channel.Due(0)[0].Sequence;
        channel.Ack(sequence);
        Assert.Empty(channel.Due(1));
        Assert.Equal(0, channel.Unacked);
    }

    [Fact]
    public void ReceiverDeliversInOrderAndDropsDuplicates()
    {
        ReliableChannel receiver = new();
        Assert.Empty(receiver.Receive(1, Message(11)));
        List<byte[]> ready = receiver.Receive(0, Message(10));
        Assert.Equal([10, 11], ready.Select(m => m[0]));
        Assert.Empty(receiver.Receive(0, Message(10)));
        Assert.Empty(receiver.Receive(1, Message(11)));
    }

    [Fact]
    public void WindowHoldsBackTheRest()
    {
        ReliableChannel channel = new();
        for (int i = 0; i < ReliableChannel.Window + 5; i++)
        {
            channel.Send(Message((byte)i));
        }
        Assert.Equal(ReliableChannel.Window, channel.Due(0).Count);
        channel.Ack(0);
        Assert.Contains(channel.Due(0.01), frame => frame.Sequence == ReliableChannel.Window);
    }
}
