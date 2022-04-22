﻿using System.Reflection;

namespace S7.Net.Types;

/// <summary>
/// Contains the methods to convert a C# class to S7 data types
/// </summary>
public static class Class
{
    private static IEnumerable<PropertyInfo> GetAccessableProperties(Type classType)
    {
        return classType
            .GetProperties(
                BindingFlags.SetProperty |
                BindingFlags.Public |
                BindingFlags.Instance)
            .Where(p => p.GetSetMethod() != null);
    }

    private static double GetIncreasedNumberOfBytes(double numBytes, Type type)
    {
        switch (type.Name)
        {
            case "Boolean":
                numBytes += 0.125;
                break;
            case "Byte":
                numBytes = Math.Ceiling(numBytes);
                numBytes++;
                break;
            case "Int16":
            case "UInt16":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                numBytes += 2;
                break;
            case "Int32":
            case "UInt32":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                numBytes += 4;
                break;
            case "Single":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                numBytes += 4;
                break;
            case "Double":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                numBytes += 8;
                break;
            default:
                object? propertyClass = Activator.CreateInstance(type);
                numBytes = GetClassSize(propertyClass, numBytes, true);
                break;
        }

        return numBytes;
    }

    /// <summary>
    /// Gets the size of the class in bytes.
    /// </summary>
    /// <param name="instance">An instance of the class</param>
    /// <returns>the number of bytes</returns>
    public static double GetClassSize(object instance, double numBytes = 0.0, bool isInnerProperty = false)
    {
        IEnumerable<PropertyInfo>? properties = GetAccessableProperties(instance.GetType());
        foreach (PropertyInfo? property in properties)
        {
            if (property.PropertyType.IsArray)
            {
                Type elementType = property.PropertyType.GetElementType();
                Array array = (Array)property.GetValue(instance, null);
                if (array.Length <= 0)
                {
                    throw new Exception("Cannot determine size of class, because an array is defined which has no fixed size greater than zero.");
                }

                IncrementToEven(ref numBytes);
                for (int i = 0; i < array.Length; i++)
                {
                    numBytes = GetIncreasedNumberOfBytes(numBytes, elementType);
                }
            }
            else
            {
                numBytes = GetIncreasedNumberOfBytes(numBytes, property.PropertyType);
            }
        }
        if (false == isInnerProperty)
        {
            // enlarge numBytes to next even number because S7-Structs in a DB always will be resized to an even byte count
            numBytes = Math.Ceiling(numBytes);
            if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                numBytes++;
        }
        return numBytes;
    }

    private static object? GetPropertyValue(Type propertyType, byte[] bytes, ref double numBytes)
    {
        object? value = null;

        switch (propertyType.Name)
        {
            case "Boolean":
                // get the value
                int bytePos = (int)Math.Floor(numBytes);
                int bitPos = (int)((numBytes - (double)bytePos) / 0.125);
                if ((bytes[bytePos] & (int)Math.Pow(2, bitPos)) != 0)
                    value = true;
                else
                    value = false;
                numBytes += 0.125;
                break;
            case "Byte":
                numBytes = Math.Ceiling(numBytes);
                value = (byte)(bytes[(int)numBytes]);
                numBytes++;
                break;
            case "Int16":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                // hier auswerten
                ushort source = Word.FromBytes(bytes[(int)numBytes + 1], bytes[(int)numBytes]);
                value = source.ConvertToShort();
                numBytes += 2;
                break;
            case "UInt16":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                // hier auswerten
                value = Word.FromBytes(bytes[(int)numBytes + 1], bytes[(int)numBytes]);
                numBytes += 2;
                break;
            case "Int32":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                // hier auswerten
                uint sourceUInt = DWord.FromBytes(bytes[(int)numBytes + 3],
                                                                   bytes[(int)numBytes + 2],
                                                                   bytes[(int)numBytes + 1],
                                                                   bytes[(int)numBytes + 0]);
                value = sourceUInt.ConvertToInt();
                numBytes += 4;
                break;
            case "UInt32":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                // hier auswerten
                value = DWord.FromBytes(
                    bytes[(int)numBytes],
                    bytes[(int)numBytes + 1],
                    bytes[(int)numBytes + 2],
                    bytes[(int)numBytes + 3]);
                numBytes += 4;
                break;
            case "Single":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                // hier auswerten
                value = Real.FromByteArray(
                    new byte[] {
                        bytes[(int)numBytes],
                        bytes[(int)numBytes + 1],
                        bytes[(int)numBytes + 2],
                        bytes[(int)numBytes + 3] });
                numBytes += 4;
                break;
            case "Double":
                numBytes = Math.Ceiling(numBytes);
                if ((numBytes / 2 - Math.Floor(numBytes / 2.0)) > 0)
                    numBytes++;
                byte[]? buffer = new byte[8];
                Array.Copy(bytes, (int)numBytes, buffer, 0, 8);
                // hier auswerten
                value = LReal.FromByteArray(buffer);
                numBytes += 8;
                break;
            default:
                object? propClass = Activator.CreateInstance(propertyType);
                numBytes = FromBytes(propClass, bytes, numBytes);
                value = propClass;
                break;
        }

        return value;
    }

    /// <summary>
    /// Sets the object's values with the given array of bytes
    /// </summary>
    /// <param name="sourceClass">The object to fill in the given array of bytes</param>
    /// <param name="bytes">The array of bytes</param>
    public static double FromBytes(object sourceClass, byte[] bytes, double numBytes = 0, bool isInnerClass = false)
    {
        if (bytes is null)
            return numBytes;

        IEnumerable<PropertyInfo>? properties = GetAccessableProperties(sourceClass.GetType());
        foreach (PropertyInfo? property in properties)
        {
            if (property.PropertyType.IsArray)
            {
                Array array = (Array)property.GetValue(sourceClass, null);
                IncrementToEven(ref numBytes);
                Type elementType = property.PropertyType.GetElementType();
                for (int i = 0; i < array.Length && numBytes < bytes.Length; i++)
                {
                    array.SetValue(
                        GetPropertyValue(elementType, bytes, ref numBytes),
                        i);
                }
            }
            else
            {
                property.SetValue(
                    sourceClass,
                    GetPropertyValue(property.PropertyType, bytes, ref numBytes),
                    null);
            }
        }

        return numBytes;
    }

    private static double SetBytesFromProperty(object propertyValue, byte[] bytes, double numBytes)
    {
        int bytePos = 0;
        int bitPos = 0;
        byte[]? bytes2 = null;

        switch (propertyValue.GetType().Name)
        {
            case "Boolean":
                // get the value
                bytePos = (int)Math.Floor(numBytes);
                bitPos = (int)((numBytes - (double)bytePos) / 0.125);
                if ((bool)propertyValue)
                    bytes[bytePos] |= (byte)Math.Pow(2, bitPos);            // is true
                else
                    bytes[bytePos] &= (byte)(~(byte)Math.Pow(2, bitPos));   // is false
                numBytes += 0.125;
                break;
            case "Byte":
                numBytes = (int)Math.Ceiling(numBytes);
                bytePos = (int)numBytes;
                bytes[bytePos] = (byte)propertyValue;
                numBytes++;
                break;
            case "Int16":
                bytes2 = Int.ToByteArray((short)propertyValue);
                break;
            case "UInt16":
                bytes2 = Word.ToByteArray((ushort)propertyValue);
                break;
            case "Int32":
                bytes2 = DInt.ToByteArray((int)propertyValue);
                break;
            case "UInt32":
                bytes2 = DWord.ToByteArray((uint)propertyValue);
                break;
            case "Single":
                bytes2 = Real.ToByteArray((float)propertyValue);
                break;
            case "Double":
                bytes2 = LReal.ToByteArray((double)propertyValue);
                break;
            default:
                numBytes = ToBytes(propertyValue, bytes, numBytes);
                break;
        }

        if (bytes2 != null)
        {
            IncrementToEven(ref numBytes);

            bytePos = (int)numBytes;
            for (int bCnt = 0; bCnt < bytes2.Length; bCnt++)
                bytes[bytePos + bCnt] = bytes2[bCnt];
            numBytes += bytes2.Length;
        }

        return numBytes;
    }

    /// <summary>
    /// Creates a byte array depending on the struct type.
    /// </summary>
    /// <param name="sourceClass">The struct object</param>
    /// <returns>A byte array or null if fails.</returns>
    public static double ToBytes(object sourceClass, byte[] bytes, double numBytes = 0.0)
    {
        IEnumerable<PropertyInfo>? properties = GetAccessableProperties(sourceClass.GetType());
        foreach (PropertyInfo? property in properties)
        {
            if (property.PropertyType.IsArray)
            {
                Array array = (Array)property.GetValue(sourceClass, null);
                IncrementToEven(ref numBytes);
                Type elementType = property.PropertyType.GetElementType();
                for (int i = 0; i < array.Length && numBytes < bytes.Length; i++)
                {
                    numBytes = SetBytesFromProperty(array.GetValue(i), bytes, numBytes);
                }
            }
            else
            {
                numBytes = SetBytesFromProperty(property.GetValue(sourceClass, null), bytes, numBytes);
            }
        }
        return numBytes;
    }

    private static void IncrementToEven(ref double numBytes)
    {
        numBytes = Math.Ceiling(numBytes);
        if (numBytes % 2 > 0) numBytes++;
    }
}