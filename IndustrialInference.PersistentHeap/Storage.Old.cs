using System.Buffers;
using System.Runtime.InteropServices;

namespace IndustrialInference.PersistentHeap.Old;

public interface IStorage
{
    byte[] Buf { get; }
    byte this[int i] { get; set; }
    byte this[Index i] { get; set; }
    byte[] this[Range i] { get; set; }
}

public class ArrayOf<T>
{
    private readonly ByteStorage _buf;

    public ArrayOf()
    {
        int bufSize = 1;
        _buf = new ByteStorage(bufSize);
    }
}

public class ByteStorage : IStorage
{
    private const int MaxBufferSize = 1 << 16;

    public ByteStorage(int size)
    {
        if (0 < size && size <= MaxBufferSize) Buf = new byte[size];
    }

    public byte[] Buf { get; }

    public byte[] this[Range i]
    {
        get => Buf[i];
        set
        {
            if (i.Start.Value + value.Length > Buf.Length) throw new ArgumentOutOfRangeException(nameof(value));
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

/// <summary>
///     A class providing an abstraction over some storage.
/// </summary>
/// <remarks>
///     The purpose of this class is to encapsulate storage so that the user doesn't know whether they are writing to disk
///     or writing to an in-memory byte array (or both)
/// </remarks>
public class StorageProvider
{
}

/// <summary>
/// A MemoryManager over a raw pointer
/// </summary>
/// <remarks><para>The pointer is assumed to be fully unmanaged, or externally pinned - no attempt will be made to pin this data</para>
/// <para>https://github.com/mgravell/Pipelines.Sockets.Unofficial/blob/master/src/Pipelines.Sockets.Unofficial/UnsafeMemory.cs</para>
/// </remarks>
public sealed unsafe class UnmanagedMemoryManager<T> : MemoryManager<T>
    where T : unmanaged
{
    private readonly int _length;
    private readonly T* _pointer;

    /// <summary>
    /// Create a new UnmanagedMemoryManager instance at the given pointer and size
    /// </summary>
    /// <remarks>It is assumed that the span provided is already unmanaged or externally pinned</remarks>
    public UnmanagedMemoryManager(Span<T> span)
    {
        fixed (T* ptr = &MemoryMarshal.GetReference(span))
        {
            _pointer = ptr;
            _length = span.Length;
        }
    }

    /// <summary>
    /// Create a new UnmanagedMemoryManager instance at the given pointer and size
    /// </summary>
    [CLSCompliant(false)]
    public UnmanagedMemoryManager(T* pointer, int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        _pointer = pointer;
        _length = length;
    }

    /// <summary>
    /// Create a new UnmanagedMemoryManager instance at the given pointer and size
    /// </summary>
    public UnmanagedMemoryManager(IntPtr pointer, int length) : this((T*)pointer.ToPointer(), length) { }

    /// <summary>
    /// Obtains a span that represents the region
    /// </summary>
    public override Span<T> GetSpan() => new Span<T>(_pointer, _length);

    /// <summary>
    /// Provides access to a pointer that represents the data (note: no actual pin occurs)
    /// </summary>
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if (elementIndex < 0 || elementIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));
        return new MemoryHandle(_pointer + elementIndex);
    }

    /// <summary>
    /// Has no effect
    /// </summary>
    public override void Unpin()
    { }

    /// <summary>
    /// Releases all resources associated with this object
    /// </summary>
    protected override void Dispose(bool disposing)
    { }
}