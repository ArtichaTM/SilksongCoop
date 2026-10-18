using System;
using System.IO;
using System.Text;

namespace SilksongCoop.Net;

/// <summary>Little-endian binary writer for one datagram or message body.</summary>
internal sealed class PacketWriter
{
    private readonly MemoryStream _stream = new();
    private readonly BinaryWriter _writer;

    public PacketWriter() => _writer = new BinaryWriter(_stream, Encoding.UTF8);

    public int Length => (int)_stream.Length;

    public PacketWriter Byte(byte value)
    {
        _writer.Write(value);
        return this;
    }

    public PacketWriter Bool(bool value)
    {
        _writer.Write(value);
        return this;
    }

    public PacketWriter Int(int value)
    {
        _writer.Write(value);
        return this;
    }

    public PacketWriter UInt(uint value)
    {
        _writer.Write(value);
        return this;
    }

    public PacketWriter Float(float value)
    {
        _writer.Write(value);
        return this;
    }

    /// <summary>Null is written as an empty string.</summary>
    public PacketWriter String(string? value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value ?? "");
        if (bytes.Length > ushort.MaxValue)
        {
            throw new ArgumentException($"string of {bytes.Length} bytes does not fit a packet");
        }
        _writer.Write((ushort)bytes.Length);
        _writer.Write(bytes);
        return this;
    }

    public PacketWriter Bytes(byte[] value)
    {
        _writer.Write(value.Length);
        _writer.Write(value);
        return this;
    }

    public byte[] ToArray()
    {
        _writer.Flush();
        return _stream.ToArray();
    }
}
