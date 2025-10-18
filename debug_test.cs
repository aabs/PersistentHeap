using IndustrialInference.BPlusTree;

var data = new int[] { 5, -3, 1, 2, -4, 3, 4, 0, -2, -1 };

var tree = new BPlusTree<long, long>();

Console.WriteLine("=== Inserting data ===");
foreach (var x in data)
{
    Console.WriteLine($"Inserting {x}, count before: {tree.Count()}");
    tree.Insert(x, x);
    Console.WriteLine($"Count after: {tree.Count()}");
}

Console.WriteLine($"\n=== Final tree state ===");
Console.WriteLine($"Total count: {tree.Count()}");
Console.WriteLine($"Unique values in original array: {data.Distinct().Count()}");

Console.WriteLine($"\n=== Attempting to delete first value ({data[0]}) ===");
var countBefore = tree.Count();
Console.WriteLine($"Count before delete: {countBefore}");

try
{
    var result = tree.Delete(data[0]);
    Console.WriteLine($"Delete result: {result}");
    Console.WriteLine($"Count after delete: {tree.Count()}");
    Console.WriteLine($"Expected count: {countBefore - 1}");
}
catch (Exception ex)
{
    Console.WriteLine($"Delete failed: {ex.Message}");
}