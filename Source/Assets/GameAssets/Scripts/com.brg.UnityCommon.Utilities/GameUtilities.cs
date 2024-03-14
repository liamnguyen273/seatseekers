using System;
using System.Collections.Generic;
using System.Linq;

namespace com.brg.UnityCommon
{
    public static partial class Utilities
    {
        public static void SplitResources(string resource, int count, out string[] itemArray, out int[] countArray)
        {
            var leftCount = count;

            var items = new List<string>();
            var counts = new List<int>();
                        
            if  (leftCount < 10)
            {
                items.AddRange(Enumerable.Repeat(resource, leftCount));
                counts.AddRange(Enumerable.Repeat(1, leftCount));
            }
            else
            {
                while (leftCount > 0)
                {
                    var amount = Math.Min(10, leftCount);
                    items.Add(resource);
                    counts.Add(amount);
                    leftCount -= amount;
                }
            }

            itemArray = items.ToArray();
            countArray = counts.ToArray();
        }
    }
}