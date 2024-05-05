namespace IndustrialInference.BPlusTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Serializable]
public class BPlusTreeException : Exception
{
    public BPlusTreeException() { }
    public BPlusTreeException(string message) : base(message) { }
    public BPlusTreeException(string message, Exception inner) : base(message, inner) { }
    protected BPlusTreeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}


[Serializable]
public class OverfullNodeException : BPlusTreeException
{
    public OverfullNodeException() { }
    public OverfullNodeException(string message) : base(message) { }
    public OverfullNodeException(string message, Exception inner) : base(message, inner) { }
    protected OverfullNodeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    // static helper function to throw a new instance of the exception
    public static void Throw(string message)
    {
        throw new OverfullNodeException(message);
    }
}
