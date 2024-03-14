using System.Text;

namespace com.brg.Common.Utils
{
    public static partial class Utilities
    {
        public static string Repeat(this string input, int count)
        {
            if (string.IsNullOrEmpty(input) || count <= 1)
                return input;

            var builder = new StringBuilder(input.Length * count);

            for (var i = 0; i < count; i++) builder.Append(input);

            return builder.ToString();
        }
    }
}
