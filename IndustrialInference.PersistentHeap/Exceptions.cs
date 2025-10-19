namespace IndustrialInference.BPlusTree;

using System;
using System.Runtime.CompilerServices;

[Serializable]
public class BPlusTreeException : Exception
{
    public BPlusTreeException()
    { }

    public BPlusTreeException(string message) : base(message)
    {
    }

    public BPlusTreeException(string message, Exception inner) : base(message, inner)
    {
    }

    protected BPlusTreeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    public static void Throw(string message)
    {
        throw new BPlusTreeException(message);
    }

    public static void ThrowIf(bool argument, string? message = null, [CallerArgumentExpression(nameof(argument))] string argString = "")
    {
        if (argument)
        {
            Throw($"Assertion failure: {argString}");
        }
    }
}

[Serializable]
public class OverfullNodeException : BPlusTreeException
{
    public object? SourceNode { get; private set; }

    public OverfullNodeException()
    {
        SourceNode = default;
    }

    public OverfullNodeException(string message, object? sourceNode) : base(message)
    {
        SourceNode = sourceNode;
    }
    public OverfullNodeException(object? sourceNode) : this(string.Empty, sourceNode)
    {
    }

    // static helper function to throw a new instance of the exception
    public static void Throw(string message, object? sourceNode)
    {
        throw new OverfullNodeException(message, sourceNode);
    }

    public static void ThrowIf(bool argument, string? message = null, object? sourceNode = null, [CallerArgumentExpression(nameof(argument))] string argString = "")
    {
        if (argument)
        {
            Throw($"Overfull node: {argString}", sourceNode);
        }
    }
}
