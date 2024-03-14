using System;
using System.Collections.Generic;
using System.Linq;

namespace com.brg.Common.Utils
{
    public static partial class Extensions
    {
        public static T[] CloneShiftRight<T>(this T[] array, int count)
        {
            count %= array.Length;

            var result = new T[array.Length];

            for (int i = 0; i < array.Length; ++i)
            {
                var tI = (i + count) % array.Length;
                result[tI] = array[i];
            }

            return result;
        }

        public static IEnumerable<T> CloneShiftRight<T>(this IEnumerable<T> enumerable, int count)
        {
            var elements = enumerable as T[] ?? enumerable.ToArray();
            var enumerableCount = elements.Length;
            count %= enumerableCount;

            var i = 0;
            foreach (var element in elements)
            {
                var tI = (i + count) % enumerableCount;
                yield return elements[tI];
                ++i;
            }
        }

        public static T[] CloneShiftLeft<T>(this T[] array, int count)
        {
            count %= array.Length;

            var result = new T[array.Length];

            for (int i = 0; i < array.Length; ++i)
            {
                var tI = (i - count + array.Length) % array.Length;
                result[tI] = array[i];
            }

            return result;
        }

        public static IEnumerable<T> CloneShiftLeft<T>(this IEnumerable<T> enumerable, int count)
        {
            var elements = enumerable as T[] ?? enumerable.ToArray();
            var enumerableCount = elements.Length;
            count %= enumerableCount;

            var i = 0;
            foreach (var element in elements)
            {
                var tI = (i - count + elements.Length) % enumerableCount;
                yield return elements[tI];
                ++i;
            }
        }

        public static T ShiftRemove<T>(this T[] array, int index)
        {
            if (index >= array.Length) throw new IndexOutOfRangeException();
        
            var result = array[index];
        
            for (int i = index + 1; i < array.Length; ++i)
            {
                array[i - 1] = array[i];
            }

            return result;
        }

        public static T[] ShiftRemoveRange<T>(this T[] array, IEnumerable<int> indexes)
        {
            var sortedIndexes = indexes.Where(x => x < array.Length && x >= 0).OrderBy(x => x).ToArray();
            var results = new T[sortedIndexes.Length];

            var j = 0;
            var shiftAmount = 0;
            var i = 0;
            while (i < array.Length)
            {
                if (j < sortedIndexes.Length && i == sortedIndexes[j])
                {
                    results[j] = array[sortedIndexes[j]];
                    ++shiftAmount;
                    ++j;
                }
                else if (shiftAmount > 0)
                {
                    array[i - shiftAmount] = array[i];
                }
            
                ++i;
            }

            return results;
        }

        public static IEnumerable<int> FindAllIndexes<T>(this T[] array, Predicate<T> predicate)
        {
            var result = new List<int>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (predicate(array[i]))
                {
                    result.Add(i);
                }
            }

            return result;
        }
    
        public static IEnumerable<int> FindAllIndexes<T>(this List<T> list, Predicate<T> predicate)
        {
            var result = new List<int>();
            for (int i = 0; i < list.Count; ++i)
            {
                if (predicate(list[i]))
                {
                    result.Add(i);
                }
            }

            return result;
        }
    }
}