namespace com.brg.Common.Utils
{
    public static partial class Utilities 
    {
        public static int GetMask(int count)
        {
            return (1 << count) - 1;
        }

        public static int GetOnlyMask(int pos)
        {
            return 1 << pos;
        }
    }
}