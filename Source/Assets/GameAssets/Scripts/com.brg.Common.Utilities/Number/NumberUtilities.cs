using System;
using System.Collections.Generic;

namespace com.brg.Common.Utils
{
    public static partial class Utilities
    {   
        private static Dictionary<int, int>? _powersOfTen;
    
        public static int PowerOfTen(int n)
        {
            _powersOfTen ??= new Dictionary<int, int>
            {
                { 0, 1 },
                { 1, 10 },
                { 2, 100 },
                { 3, 1000 },
                { 4, 10000 },
                { 5, 100000 },
                { 6, 1000000 },
                { 7, 10000000 },
                { 8, 100000000 },
                { 9, 1000000000 }
            };

            return _powersOfTen[n];
        }
    }
}