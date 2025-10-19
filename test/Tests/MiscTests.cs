namespace PersistentHeap.Tests;


using System;

public class MiscTests
{
    [Fact]
    void TestInsertIntoSortedArray()
    {
        // Arrange
        int[] sortedArray = { 1, 3, 5, 7, 9 };
        int valueToInsert = 4;
        int[] expectedArray = { 1, 3, 4, 5, 7, 9 };
        var workingArray = new int[sortedArray.Length + 1];
        Array.Copy(sortedArray, workingArray, sortedArray.Length);

        // Act
        int insertionIndex = FindInsertionPointByBinarysSearch(workingArray, valueToInsert);
        Array.Copy(workingArray, insertionIndex, workingArray, insertionIndex + 1, workingArray.Length - (insertionIndex + 1));
        workingArray[insertionIndex] = valueToInsert;
        // test working array is still ordered
        for (int i = 0; i < workingArray.Length - 1; i++)
        {
            if (workingArray[i] > workingArray[i + 1])
            {
                Assert.Fail();
            }
        }

    }

    int FindInsertionPointByBinarysSearch(int[] array, int value)
    {
        int low = 0;
        int high = array.Length - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (array[mid] == value)
            {
                return mid;
            }
            if (array[mid] < value)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        return low;
    }
}
