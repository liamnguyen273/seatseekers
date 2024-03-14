using System;
using System.Collections.Generic;
using System.Linq;

namespace com.brg.Common.Utils
{
    public static partial class Utilities
    {
        public static IEnumerable<int> Permutate(int start, int count, Func<int> integerGetter)
        {
            return Enumerable.Range(start, count).OrderBy(_ => integerGetter());
        }

        public static IEnumerable<T> Iterate<T>(params IEnumerable<T>[] iterables)
        {
            return iterables.SelectMany(iterable => iterable);
        }
    }
}