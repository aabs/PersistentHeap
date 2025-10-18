namespace PersistentHeap.Tests;

using DotNext.IO.MemoryMappedFiles;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;


public class MemoryMappedFilesTests
{
    [Fact]
    public void CreateAndWriteToMMF()
    {
        var filename = Path.GetTempFileName();
        try
        {
            using var file = File.Create(filename, 2048, FileOptions.RandomAccess);
            using var mappedFile = MemoryMappedFile.CreateFromFile(file,
                                                                   null,
                                                                   1024,
                                                                   MemoryMappedFileAccess.ReadWrite,
                                                                   HandleInheritability.None,
                                                                   false);
            int[] array = new[] { 1, 2, 3, 4, 5 };
            using var accessor = mappedFile.CreateViewAccessor(0, sizeof(int) * array.Length);
            accessor.WriteArray(0, array, 0, 5);
            accessor.Flush();
            var newArray = new int[5];
            accessor.ReadArray<int>(0, newArray, 0, 5);
            foreach (var item in newArray)
            {
                Console.WriteLine(item);
            }
        }
        finally
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }

    [Fact]
    public void CreateAndWriteToMMFWithoutFlush()
    {
        var filename = Path.GetTempFileName();
        try
        {
            using var file = File.Create(filename, 2048, FileOptions.RandomAccess);
            using var mappedFile = MemoryMappedFile.CreateFromFile(file,
                                                                   null,
                                                                   1024,
                                                                   MemoryMappedFileAccess.ReadWrite,
                                                                   HandleInheritability.None,
                                                                   false);
            int[] array = new[] { 1, 2, 3, 4, 5 };
            using var accessor = mappedFile.CreateViewAccessor(0, sizeof(int) * array.Length);
            accessor.WriteArray(0, array, 0, 5);
            var newArray = new int[5];
            accessor.ReadArray<int>(0, newArray, 0, 5);
            foreach (var item in newArray)
            {
                Console.WriteLine(item);
            }
        }
        finally
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }

    [Fact]
    public void AccessMemoryMappedFileUsingMemoryOfT()
    {
        var filename = Path.GetTempFileName();
        using var file = File.Create(filename, 2048, FileOptions.RandomAccess);
        using var mappedFile = MemoryMappedFile.CreateFromFile(file,
                                                               null,
                                                               1024,
                                                               MemoryMappedFileAccess.ReadWrite,
                                                               HandleInheritability.None,
                                                               false);
        var accessor = mappedFile.CreateMemoryAccessor();
        var memInts = MemoryMarshal.Cast<byte, int>(accessor.Bytes);
        for (int i = 0; i < 5; i++)
        {
            memInts[i] = i;
        }
        accessor.Flush();

        using var accessor2 = mappedFile.CreateViewAccessor(0, sizeof(int) * 10);
        var newArray = new int[5];
        accessor2.ReadArray<int>(0, newArray, 0, 5);
        foreach (var item in newArray)
        {
            Console.WriteLine(item);
        }
    }

    [Fact(Skip = "interactive")]
    public void Test2()
    {
        // create a memory-mapped file of length 1000 bytes and give it a 'map name' of 'test'  
        var mmf = MemoryMappedFile.CreateNew("test", 1000);
        // write an integer value of 42 to this file at position 500  
        var accessor = mmf.CreateViewAccessor();
        accessor.Write(500, 42);
        Console.WriteLine("Memory-mapped file created!");
        Console.ReadLine(); // pause till enter key is pressed  
        // dispose of the memory-mapped file object and its accessor  
        accessor.Dispose();
        mmf.Dispose();
    }
}
