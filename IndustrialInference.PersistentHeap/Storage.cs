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

}

public class ByteStorage : IStorage
{
    private readonly byte[] _data;
    public ByteStorage(int size)
    {
        if (0 < size && size <= MaxBufferSize)
        {
            _data = new byte[size];
        }
    }

    private const int MaxBufferSize = 1 << 16;

    public byte[] Buf => _data;
}