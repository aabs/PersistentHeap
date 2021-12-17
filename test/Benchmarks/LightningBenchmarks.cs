using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using LightningDB;

namespace Benchmarks;

[SimpleJob(RuntimeMoniker.Net60, baseline: true)]
[RPlotExporter]
public class LightningBenchmark
{
    private LightningEnvironment env;
    private Random r;
    
    [Params(1, 10, 100)]
    public int N;

    private LightningTransaction tx;
    private LightningDatabase db;

    [GlobalSetup]
    public void Setup()
    {
        env = new LightningEnvironment("pathtofolder");
        env.MaxDatabases = 2;
        env.Open();
        r = new Random(29);
        tx = env.BeginTransaction();
        db = tx.OpenDatabase(null, new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
    }

    [GlobalCleanup]
    public void Teardown()
    {
        tx.TruncateDatabase(db);
        tx.Commit();
        db.Dispose();
        tx.Dispose();
        env.Dispose();
    }

    [Benchmark]
    public void GetAndSetPageViaSpans()
    {
        var contents1 = new ContentsBlock(r.Next(), r.Next());
        // write
        {
            for (int i = 0; i < N; i++)
            {
                var arr = new ReadOnlySpan<ContentsBlock>(new[] { contents1 });
                var arrSpan = MemoryMarshal.Cast<ContentsBlock, byte>(arr);
                var keySpan = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"Contents {i}"));
                tx.Put(db, keySpan, arrSpan);
            }
        }
        // read back
        {
            for (int i = 0; i < N; i++)
            {
                var keySpan = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes($"Contents {i}"));
                var (resultCode, key, value) = tx.Get(db, keySpan);
                
                var contentSpan = MemoryMarshal.Cast<byte, ContentsBlock>(value.AsSpan());
                var contents2 = contentSpan.ToArray()[0];
            }
        }
    }


    [Benchmark]
    public void GetAndSetPageViaArrays()
    {
        var contents1 = new ContentsBlock(r.Next(), r.Next());
        // write
        {
            for (int i = 0; i < N; i++)
            {
                var keySpan = Encoding.UTF8.GetBytes($"Contents {i}");
                tx.Put(db, keySpan, ObjectToByteArray(contents1));
            }
        }
        // read back
        {
            for (int i = 0; i < N; i++)
            {
                var keySpan = Encoding.UTF8.GetBytes($"Contents {i}");
                var (resultCode, key, value) = tx.Get(db, keySpan);
                var contents2 = ByteArrayToObject(value.CopyToNewArray());
            }
        }
    }


    // Convert an object to a byte array
    private byte[] ObjectToByteArray(Object obj)
    {
        if (obj == null)
            return null;

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, obj);

        return ms.ToArray();
    }

    // Convert a byte array to an Object
    private Object ByteArrayToObject(byte[] arrBytes)
    {
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binForm = new BinaryFormatter();
        memStream.Write(arrBytes, 0, arrBytes.Length);
        memStream.Seek(0, SeekOrigin.Begin);
        Object obj = (Object)binForm.Deserialize(memStream);

        return obj;
    }
    [Serializable]
    public struct ContentsBlock
    {
        public ContentsBlock(int version, int soIndexBlockName)
        {
            Version = version;
            SoIndexBlockName = soIndexBlockName;
        }

        public int Version { get; set; }
        public int SoIndexBlockName { get; set; }
    }

}