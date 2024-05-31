using System;
using System.Collections.Generic;
using Unity.Netcode;

public static class NetworkSerializableExtension
{
    public static void SerializeString<T>(this BufferSerializer<T> serializer, ref string value) where T : IReaderWriter
    {
        if (serializer.IsWriter)
        {
            int count = value.Length;
            serializer.SerializeValue(ref count);
            serializer.SerializeValue(ref value);
        }
        else
        {
            int count = 0;
            serializer.SerializeValue(ref count);
            serializer.SerializeValue(ref value);
        }
    }

    public static void SerializeList_NetworkSerializable<T, U>(this BufferSerializer<T> serializer, ref List<U> serializableList) where T : IReaderWriter where U : INetworkSerializable, new()
    {
        U[] array;
        int count = 0;
        if (serializer.IsWriter)
        {
            array = serializableList.ToArray();
            count = array.Length;
        }
        else
        {
            array = new U[count];
        }

        serializer.SerializeValue(ref count);
        serializer.SerializeValue(ref array);
        if (serializer.IsReader)
        {
            serializableList = new List<U>(array);
        }
    }

    public static void SerializeList_Primitives<T, U>(this BufferSerializer<T> serializer, ref List<U> serializableList) where T : IReaderWriter where U : unmanaged, IComparable, IConvertible, IComparable<U>, IEquatable<U>
    {
        U[] array;
        int count = 0;
        if (serializer.IsWriter)
        {
            array = serializableList.ToArray();
            count = array.Length;
        }
        else
        {
            array = new U[count];
        }

        serializer.SerializeValue(ref count);
        serializer.SerializeValue(ref array);
        if (serializer.IsReader)
        {
            serializableList = new List<U>(array);
        }
    }
}