// ReSharper disable All
namespace com.brg.Common.Utils
{
    public static partial class Extensions
    {
        public static bool IsSet(this int mask, int pos)
        {
            var res = (mask & (1 << pos)) != 0;
            return res;
        }

        public static int Set(this int mask, int pos)
        {
            return mask | (1 << pos);
        }

        public static int UnSet(this int mask, int pos)
        {
            return mask & ~(1 << pos);
        }

        public static int Toggle(this int mask, int pos)
        {
            return mask ^ (1 << pos);
        }

        public static bool HasOnlyOneFlag(this int mask)
        {
            return !mask.IsEmpty() && (mask & (mask - 1)) == 0;
        }

        public static bool IsEmpty(this int mask)
        {
            return mask == 0;
        }
    }
}