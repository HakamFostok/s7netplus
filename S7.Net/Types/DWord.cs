﻿namespace S7.Net.Types;

/// <summary>
/// Contains the conversion methods to convert DWord from S7 plc to C#.
/// </summary>
public static class DWord
{
    /// <summary>
    /// Converts a S7 DWord (4 bytes) to uint (UInt32)
    /// </summary>
    public static uint FromByteArray(byte[] bytes)
    {
        return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
    }

    /// <summary>
    /// Converts 4 bytes to DWord (UInt32)
    /// </summary>
    public static uint FromBytes(byte b1, byte b2, byte b3, byte b4)
    {
        return (uint)((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
    }

    /// <summary>
    /// Converts a uint (UInt32) to S7 DWord (4 bytes)
    /// </summary>
    public static byte[] ToByteArray(uint value)
    {
        byte[] bytes = new byte[4];

        bytes[0] = (byte)((value >> 24) & 0xFF);
        bytes[1] = (byte)((value >> 16) & 0xFF);
        bytes[2] = (byte)((value >> 8) & 0xFF);
        bytes[3] = (byte)((value) & 0xFF);

        return bytes;
    }

    /// <summary>
    /// Converts an array of uint (UInt32) to an array of S7 DWord (4 bytes)
    /// </summary>
    public static byte[] ToByteArray(uint[] value)
    {
        ByteArray arr = new();
        foreach (uint val in value)
            arr.Add(ToByteArray(val));
        return arr.Array;
    }

    /// <summary>
    /// Converts an array of S7 DWord to an array of uint (UInt32)
    /// </summary>
    public static uint[] ToArray(byte[] bytes)
    {
        uint[] values = new uint[bytes.Length / 4];

        int counter = 0;
        for (int cnt = 0; cnt < bytes.Length / 4; cnt++)
            values[cnt] = FromByteArray(new byte[] { bytes[counter++], bytes[counter++], bytes[counter++], bytes[counter++] });

        return values;
    }
}