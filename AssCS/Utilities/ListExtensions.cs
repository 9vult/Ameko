// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Utilities;

public static class ListExtensions
{
    /// <param name="list">Sorted list</param>
    /// <typeparam name="T">Type of list</typeparam>
    extension<T>(IList<T> list)
    {
        /// <summary>
        /// Add an item to a sorted list
        /// </summary>
        /// <param name="value">Value to add to the list</param>
        public void AddSorted(T value)
        {
            var idx = list.BinarySearch(value);
            list.Insert(idx >= 0 ? idx : ~idx, value);
        }

        /// <summary>
        /// Binary search implementation for <see cref="IList{T}"/>
        /// </summary>
        /// <param name="value">Value to look for</param>
        /// <param name="comparer">Optional comparer</param>
        /// <returns>Index</returns>
        public int BinarySearch(T value, IComparer<T>? comparer = null)
        {
            ArgumentNullException.ThrowIfNull(list);

            comparer ??= Comparer<T>.Default;

            var lower = 0;
            var upper = list.Count - 1;

            while (lower <= upper)
            {
                var middle = lower + (upper - lower) / 2;
                var comparisonResult = comparer.Compare(value, list[middle]);
                switch (comparisonResult)
                {
                    case 0:
                        return middle;
                    case < 0:
                        upper = middle - 1;
                        break;
                    default:
                        lower = middle + 1;
                        break;
                }
            }

            return ~lower;
        }
    }
}
