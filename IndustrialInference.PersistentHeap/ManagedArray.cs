namespace IndustrialInference.BPlusTree;

using System;

public class ManagedArray<T>
{
    public T[] Arr { get; }
    public int Count { get; set; }
    public int Length => Arr.Length;
    public bool IsFull => Count == Arr.Length;
    public bool IsEmpty => Count == 0;

    public ManagedArray(T[] arr, int count, int capacity) : this(capacity)
    {
        ArgumentNullException.ThrowIfNull(arr);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, arr.Length);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, count);

        OverwriteWith(arr, count);
        Count = count;
    }

    public ManagedArray(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0);
        Arr = new T[capacity];
        Count = 0;
    }

    public ManagedArray(T[] array, int capacity) : this(array, array.Length, capacity)
    {
    }

    public void OverwriteWith(T[] a, int l)
    {
        BPlusTreeException.ThrowIf(Arr.Length < a.Length, "no space to copy in source array");
        BPlusTreeException.ThrowIf(a.Length < l, "Count of source array is too large");
        Array.Copy(a, Arr, l);
        Count = l;
    }
    public T this[int index]
    {
        get => Arr[index];
        set => Arr[index] = value;
    }

    public int FindInsertionPoint(T value)
    {
        var idx = Array.BinarySearch(Arr, 0, Count, value);
        return idx >= 0 ? idx : ~idx;
    }

    public int IndexOf(T value)
    {
        for (int i = 0; i < Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(Arr[i], value))
            {
                return i;
            }
        }
        return -1;
    }

    public void Append(T t)
    {
        BPlusTreeException.ThrowIf(IsFull, "Array is full");
        Arr[Count] = t;
        Count += 1;
    }
    public void InsertAt(T t, int insertionIndex)
    {
        BPlusTreeException.ThrowIf(insertionIndex < 0 || insertionIndex > Count, "Insertion index out of bounds");
        if (IsFull)
        {
            throw new ApplicationException("Array is full");
        }
        if (insertionIndex < Count)
        {
            Array.Copy(Arr, insertionIndex, Arr, insertionIndex + 1, Count - (insertionIndex));
        }
        Arr[insertionIndex] = t;
        Count += 1;
    }

    public void DeleteAt(int deletionPoint)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(deletionPoint, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(deletionPoint, Count);
        BPlusTreeException.ThrowIf(Count == 0, "Cannot delete from empty array");

        Array.Copy(Arr, deletionPoint + 1, Arr, deletionPoint, Arr.Length - (deletionPoint + 1));
        Arr[Count - 1] = default(T);
        Count -= 1;
    }

    public void ReplaceValue(T oldValue, T newValue)
    {
        var idx = IndexOf(oldValue);
        if (idx >= 0)
        {
            Arr[idx] = newValue;
        }
    }
}
