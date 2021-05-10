using System;
using System.Collections.ObjectModel;

namespace Multitool.Sorting
{
    public class ObservableCollectionQuickSort : QuickSort
    {
        public static T[] Sort<T>(ObservableCollection<T> array) where T : IComparable<T>
        {
            T[] items = new T[array.Count];
            array.CopyTo(items, 0);
            Sort(items, 0, items.Length - 1);

            return items;
        }
    }
}
