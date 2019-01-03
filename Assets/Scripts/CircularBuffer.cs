using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularBuffer<T> : IEnumerable<T>
{
    private readonly T[] buffer;
    private int startIndex = 0;

    public CircularBuffer(int size)
    {
        buffer = new T[size];
    }

    public T GetFirstElement()
    {
        return buffer[GetInternalIndex(0)];
    }

    public T GetLastElement()
    {
        return buffer[GetInternalIndex(buffer.Length - 1)];
    }

    public T this[int index]
    {
        get
        {
            return buffer[GetInternalIndex(index)];
        }
        set
        {
            buffer[GetInternalIndex(index)] = value;
        }
    }

    public void Rotate(int shift)
    {
        startIndex = (startIndex + shift) % buffer.Length;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < buffer.Length; ++i)
        {
            yield return buffer[GetInternalIndex(i)];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }

    private int GetInternalIndex(int index)
    {
        return Mod(startIndex + index, buffer.Length);
    }

    int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
