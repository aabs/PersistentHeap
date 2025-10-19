using IndustrialInference.BPlusTree;

var data = new int[] { 5, -3, 1, 2, -4, 3, 4, 0, -2, -1 };

var tree = new BPlusTree<long, long>();

Console.WriteLine("=== Inserting all data ===");
foreach (var x in data)
{
    tree.Insert(x, x);
}

Console.WriteLine($"Final count: {tree.Count()}");
Console.WriteLine($"Expected: {data.Distinct().Count()}");

Console.WriteLine($"\n=== Testing delete ===");
var countBefore = tree.Count();
var valToRemove = data[0]; // 5
Console.WriteLine($"Removing {valToRemove}:");
Console.WriteLine($"  Count before: {countBefore}");
Console.WriteLine($"  Contains {valToRemove} before: {tree.ContainsKey(valToRemove)}");

var result = tree.Delete(valToRemove);
Console.WriteLine($"  Delete result: {result}");
Console.WriteLine($"  Count after: {tree.Count()}");
Console.WriteLine($"  Contains {valToRemove} after: {tree.ContainsKey(valToRemove)}");
Console.WriteLine($"  Expected count: {countBefore - 1}");
Console.WriteLine($"  SUCCESS: {tree.Count() == countBefore - 1}");
