using System.IO;
using System.Text;

namespace SilksongCoop.Net;

/// <summary>Reads what <see cref="PacketWriter"/> wrote; a truncated packet throws.</summary>
internal sealed class PacketReader
{
    private readonly BinaryReader _reader;

    public PacketReader(byte[] data, int offset = 0)
    {
        MemoryStream body = new(data, offset, data.Length - offset);
        _reader = new BinaryReader(body, Encoding.UTF8);
    }

    public bool AtEnd => _reader.BaseStream.Position >= _reader.BaseStream.Length;

    public byte Byte() => _reader.ReadByte();

    public bool Bool() => _reader.ReadBoolean();

    public int Int() => _reader.ReadInt32();

    public uint UInt() => _reader.ReadUInt32();

    public float Float() => _reader.ReadSingle();

    public string String()
    {
        ushort length = _reader.ReadUInt16();
        return Encoding.UTF8.GetString(Exact(length));
    }

    public byte[] Bytes() => Exact(_reader.ReadInt32());

    /// <summary>Everything not read yet.</summary>
    public byte[] Rest() => Exact((int)(_reader.BaseStream.Length - _reader.BaseStream.Position));

    private byte[] Exact(int count)
    {
        byte[] bytes = _reader.ReadBytes(count);
        if (bytes.Length != count)
        {
            throw new EndOfStreamException($"wanted {count} bytes, packet had {bytes.Length}");
        }
        return bytes;
    }
}
