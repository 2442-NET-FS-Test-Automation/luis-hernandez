namespace DsaThreading;

public static class Sorts
{
    //Buble sort (O(n^2) - worst case) -  we just swap adjacent pairs until the the largest ones buble to the end
    public static int[] Bubble(int[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            for (int j = 0; j < input.Length; j++)
            {
                if (input[j] > input[i])
                {
                    int temp = input[i];
                    input[i] = input[j];
                    input[j] = temp;
                }
            }
        }

        return input;
    }

    // Insertion sort (O(n^2) - worst case): building the sorted array one element at a time
    // Start with a new empty array, and then as we insert compare, and continue
    public static int[] Insertion(int[] input)
    {
        int length = input.Length;

        for (int i = 0; i < length; i++)
        {
            int current = input[i];
            int j = i - 1;

            // Shift elements of input that are greater than the current one position ahead of where they are now
            while (j >= 0 && input[j] > current)
            {
                input[j + 1] = input[j];
                j--;
            }

            input[j + 1] = current;
        }

        return input;
    }

    public static int[] Selection(int[] input)
    {
        int length = input.Length;

        for (int i = 0; i < length - 1; i++)
        {
            int minIndex = i;

            for (int j = i + 1; j < length; j++)
            {
                if (input[j] < input[minIndex])
                {
                    minIndex = j;
                }
            }

            // Swap the found minimum element with the first element
            int temp = input[minIndex];
            input[minIndex] = input[i];
            input[i] = temp;
        }

        return input;
    }

    public static int[] Merge(int[] input)
    {
        // Base case, if its an array of 1
        if (input.Length <= 1)
        {
            return input;
        }

        int mid = input.Length / 2;

        //We split the array into two halves, and then we recursively call merge sort on each half
        int[] left = Merge(input[..mid]);
        int[] right = Merge(input[mid..]);

        return MergeTwo(left, right);
    }

    public static int[] MergeTwo(int[] left, int[] right)
    {
        //Empty array that is the total length of left + right
        int[] sorted = new int[left.Length + right.Length];

        int i = 0, j = 0, k = 0;

        //Tr


        return sorted;
    }
}