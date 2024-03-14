using System.Numerics;

namespace com.brg.Common.Utils
{
    public static partial class Extensions
    {
        public static int CountDigits(this int n)
        {
            if (n < 100000)
            {
                if (n < 100)
                {
                    if (n < 10)
                        return 1;
                    return 2;
                }

                // 3 or 4 or 5
                if (n < 1000)
                    return 3;
            
                // 4 or 5
                if (n < 10000)
                    return 4;
                return 5;
            }

            // 6 or more
            if (n < 10000000)
            {
                // 6 or 7
                if (n < 1000000)
                    return 6;
                return 7;
            }

            // 8 to 10
            if (n < 100000000)
                return 8;
            // 9 or 10
            if (n < 1000000000)
                return 9;
            return 10;
        }
    
        public static bool OfSameDigits(int n)
        {
            int length = n.CountDigits();
            int m = (Utils.Utilities.PowerOfTen(length) - 1) / 9;
            m *= n % 10;

            return m == n;
        }

        public static int DigitAt(this int n, int pos)
        {
            var res = n / Utils.Utilities.PowerOfTen(pos) % 10;
            return res;
        }

        public static int TopDigit(this int n)
        {
            return n == 0 ? 0 : n.DigitAt(CountDigits(n) - 1);
        }

        public static bool IsOdd(this int n)
        {
            return n % 2 == 1;
        }

        public static bool IsEven(this int n)
        {
            return n % 2 == 0;
        }

        public static int NormalizeModulo(this int n, int modulo)
        {
            return (n % modulo + modulo) % 4;
        }

        public static float GetDecimal(this float number)
        {
            var whole = (float)(int)number;
            return number - whole;
        }
    }
}