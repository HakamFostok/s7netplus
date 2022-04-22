using System.Globalization;

namespace S7.Net;

/// <summary>
/// Conversion methods to convert from Siemens numeric format to C# and back
/// </summary>
public static class Conversion
{
    /// <summary>
    /// Converts a binary string to Int32 value
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    public static int BinStringToInt32(this string txt)
    {
        int ret = 0;

        for (int i = 0; i < txt.Length; i++)
        {
            ret = (ret << 1) | ((txt[i] == '1') ? 1 : 0);
        }
        return ret;
    }

    /// <summary>
    /// Converts a binary string to a byte. Can return null.
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    public static byte? BinStringToByte(this string txt)
    {
        if (txt.Length == 8) return (byte)BinStringToInt32(txt);
        return null;
    }

    /// <summary>
    /// Converts the value to a binary string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ValToBinString(this object value)
    {
        int cnt = 0;
        int cnt2 = 0;
        int x = 0;
        string txt = "";
        long longValue = 0;

        try
        {
            if (value.GetType().Name.IndexOf("[]") < 0)
            {
                // ist nur ein Wert
                switch (value.GetType().Name)
                {
                    case "Byte":
                        x = 7;
                        longValue = (byte)value;
                        break;
                    case "Int16":
                        x = 15;
                        longValue = (short)value;
                        break;
                    case "Int32":
                        x = 31;
                        longValue = (int)value;
                        break;
                    case "Int64":
                        x = 63;
                        longValue = (long)value;
                        break;
                    default:
                        throw new Exception();
                }

                for (cnt = x; cnt >= 0; cnt += -1)
                {
                    if ((longValue & (long)Math.Pow(2, cnt)) > 0)
                        txt += "1";
                    else
                        txt += "0";
                }
            }
            else
            {
                // ist ein Array
                switch (value.GetType().Name)
                {
                    case "Byte[]":
                        x = 7;
                        byte[] ByteArr = (byte[])value;
                        for (cnt2 = 0; cnt2 <= ByteArr.Length - 1; cnt2++)
                        {
                            for (cnt = x; cnt >= 0; cnt += -1)
                                if ((ByteArr[cnt2] & (byte)Math.Pow(2, cnt)) > 0) txt += "1"; else txt += "0";
                        }
                        break;
                    case "Int16[]":
                        x = 15;
                        short[] Int16Arr = (short[])value;
                        for (cnt2 = 0; cnt2 <= Int16Arr.Length - 1; cnt2++)
                        {
                            for (cnt = x; cnt >= 0; cnt += -1)
                                if ((Int16Arr[cnt2] & (byte)Math.Pow(2, cnt)) > 0) txt += "1"; else txt += "0";
                        }
                        break;
                    case "Int32[]":
                        x = 31;
                        int[] Int32Arr = (int[])value;
                        for (cnt2 = 0; cnt2 <= Int32Arr.Length - 1; cnt2++)
                        {
                            for (cnt = x; cnt >= 0; cnt += -1)
                                if ((Int32Arr[cnt2] & (byte)Math.Pow(2, cnt)) > 0) txt += "1"; else txt += "0";
                        }
                        break;
                    case "Int64[]":
                        x = 63;
                        byte[] Int64Arr = (byte[])value;
                        for (cnt2 = 0; cnt2 <= Int64Arr.Length - 1; cnt2++)
                        {
                            for (cnt = x; cnt >= 0; cnt += -1)
                                if ((Int64Arr[cnt2] & (byte)Math.Pow(2, cnt)) > 0) txt += "1"; else txt += "0";
                        }
                        break;
                    default:
                        throw new Exception();
                }
            }
            return txt;
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Helper to get a bit value given a byte and the bit index.
    /// Example: DB1.DBX0.5 -> var bytes = ReadBytes(DB1.DBW0); bool bit = bytes[0].SelectBit(5);
    /// </summary>
    /// <param name="data"></param>
    /// <param name="bitPosition"></param>
    /// <returns></returns>
    public static bool SelectBit(this byte data, int bitPosition)
    {
        int mask = 1 << bitPosition;
        int result = data & mask;

        return result != 0;
    }

    /// <summary>
    /// Converts from ushort value to short value; it's used to retrieve negative values from words
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static short ConvertToShort(this ushort input)
    {
        short output = short.Parse(input.ToString("X"), NumberStyles.HexNumber);
        return output;
    }

    /// <summary>
    /// Converts from short value to ushort value; it's used to pass negative values to DWs
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static ushort ConvertToUshort(this short input)
    {
        ushort output = ushort.Parse(input.ToString("X"), NumberStyles.HexNumber);
        return output;
    }

    /// <summary>
    /// Converts from UInt32 value to Int32 value; it's used to retrieve negative values from DBDs
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int ConvertToInt(this uint input)
    {
        int output = int.Parse(input.ToString("X"), NumberStyles.HexNumber);
        return output;
    }

    /// <summary>
    /// Converts from Int32 value to UInt32 value; it's used to pass negative values to DBDs
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static uint ConvertToUInt(this int input)
    {
        uint output = uint.Parse(input.ToString("X"), NumberStyles.HexNumber);
        return output;
    }

    /// <summary>
    /// Converts from float to DWord (DBD)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static uint ConvertToUInt(this float input)
    {
        uint output = S7.Net.Types.DWord.FromByteArray(S7.Net.Types.Real.ToByteArray(input));
        return output;
    }

    /// <summary>
    /// Converts from DWord (DBD) to float
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static float ConvertToFloat(this uint input)
    {
        float output = S7.Net.Types.Real.FromByteArray(S7.Net.Types.DWord.ToByteArray(input));
        return output;
    }
}