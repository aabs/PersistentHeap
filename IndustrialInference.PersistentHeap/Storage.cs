namespace IndustrialInference.PersistentHeap;

/// <summary>
///     A class providing an abstraction over some storage.
/// </summary>
/// <remarks>
///     The purpose of this class is to encapsulate storage so that the user doesn't know whether they are writing to disk
///     or writing to an in-memory byte array (or both)
/// </remarks>
public class StorageProvider
{
    public StorageProvider()
    {
        
    }
}

public interface IStorage
{
    byte[] Buf { get; }
    byte this[int i] { get; set; }
    byte this[Index i] { get; set; }
    byte[] this[Range i] { get; set; }
}

public class ByteStorage : IStorage
{
    public ByteStorage(int size)
    {
        if (0 < size && size <= MaxBufferSize)
        {
            Buf = new byte[size];
        }
    }

    private const int MaxBufferSize = 1 << 16;

    public byte[] Buf { get; }

    public byte[] this[Range i]
    {
        get => Buf[i];
        set
        {
            if (i.Start.Value + value.Length > Buf.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            Array.Copy(value, 0, Buf, i.Start.Value, value.Length);
        }
    }

    public byte this[int i]
    {
        get => Buf[i];
        set => Buf[i] = value;
    }

    public byte this[Index i]
    {
        get => Buf[i];
        set => Buf[i] = value;
    }
}