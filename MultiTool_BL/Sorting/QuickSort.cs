using System;

namespace Multitool.Sorting
{
    public class QuickSort
    {
        static void Swap<T>(T[] array, int i, int j)
        {
            T item = array[i];
            array[i] = array[j];
            array[j] = item;
        }

        public static int Partition<T>(T[] array, int low, int high) where T : IComparable<T>
        {
            T pivot = array[high];
            int i = low - 1;
            for (int j = low; j <= high - 1; j++)
            {
                if (array[j].CompareTo(pivot) < 0)
                {
                    i++;
                    Swap(array, i, j);
                }
            }
            Swap(array, i + 1, high);
            return i + 1;
        }

        public static void Sort<T>(T[] array, int low, int high) where T : IComparable<T>
        {
            #region bounds checks && null array
            if (high >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(high));
            }
            if (low < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(low));
            }
            if (array == null)
            {
                throw new ArgumentException("Array was null", nameof(array));
            }
            #endregion

            if (low < high)
            {
                int pIndex = Partition(array, low, high);

                Sort(array, low, pIndex - 1);
                Sort(array, pIndex + 1, high);
            }
        }
    }
}
