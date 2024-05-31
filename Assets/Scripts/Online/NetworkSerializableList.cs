using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;


public struct NetworkSerializableList<TElement> : INetworkSerializable, IList<TElement> where TElement : unmanaged, IComparable, IConvertible, IComparable<TElement>, IEquatable<TElement>
{
    public NetworkSerializableList(TElement[] array)
    {
        this.array = array ?? Array.Empty<TElement>();
    }

    private TElement[] array;
    private TElement[] SafeArray => array ?? Array.Empty<TElement>();
    public int Count => SafeArray.Length;
    public bool IsReadOnly => false;
    public TElement this[int index]
    {
        get => SafeArray[index];
        set => SafeArray[index] = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref array);
    }

    public IEnumerator<TElement> GetEnumerator() => ((IEnumerable<TElement>)SafeArray).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => SafeArray.GetEnumerator();

    public void Add(TElement item)
    {
        array = Resize(SafeArray, Count + 1);
        array[^1] = item;
    }

    public bool Remove(TElement item)
    {
        if (!Contains(item)) return false;
        array = RemoveAt(SafeArray, IndexOf(item));
        return true;
    }

    public void Clear() => array = Array.Empty<TElement>();
    public bool Contains(TElement item) => SafeArray.Contains(item);
    public void CopyTo(TElement[] array, int arrayIndex) => SafeArray.CopyTo(array, arrayIndex);
    
    public int IndexOf(TElement item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (array[i].Equals(item))
            {
                return i;
            }
        }

        return -1;
    }

    public void Insert(int index, TElement item)
    {
        array = Insert(SafeArray, index, item);
    }

    public void RemoveAt(int index)
    {
        array = RemoveAt(SafeArray, index);
    }
    private static TElement[] Resize(TElement[] array, int newSize)
    {
        if (array == null)
        {
            return new TElement[newSize];
        }

        if (newSize == array?.Length) return array;
        if (newSize == 0)
        {
            return Array.Empty<TElement>();
        }

        TElement[] newArray = new TElement[newSize];
        Array.Copy(array, newArray, Math.Min(array.Length, newSize));
        return newArray;
    }
    private static TElement[] Insert(TElement[] array, int index, TElement item)
    {
        if (array == null || array.Length == 0)
        {
            return new[] { item };
        }

        TElement[] newArray = new TElement[array.Length + 1];
        for (int i = 0; i < newArray.Length; i++)
        {
            if (i < index)
            {
                newArray[i] = array[i];
            }
            else if (i == index)
            {
                newArray[i] = item;
            }
            else
            {
                newArray[i] = array[i - 1];
            }
        }

        return newArray;
    }

    private static TElement[] RemoveAt(TElement[] array, int removedElement)
    {
        if (array == null || array.Length == 0)
        {
            return Array.Empty<TElement>();
        }

        TElement[] newArray = new TElement[array.Length - 1];
        int index = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (i == removedElement) continue;
            newArray[index] = array[i];
            index++;
        }

        return newArray;
    }
}