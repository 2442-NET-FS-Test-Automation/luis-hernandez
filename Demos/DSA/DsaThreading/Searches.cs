namespace DsaThreading;

public static class Searches
{
    public static int LinearSearch(int[] arr, int target)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == target)
            {
                return i;
            }
        }
        return -1; // Target not found
    }

    //Binary search - half the search space each step
    // O(log n) - but we sorted before we start
    public static int BinarySearch(int[] sorted, int target)
    {
        int mid = sorted.Length / 2;
        int left = 0;
        int right = sorted.Length - 1;

        while (left <= right)
        {
            if (sorted[mid] == target)
            {
                return mid;
            }
            else if (sorted[mid] < target)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return -1; //Target not found
    }
}