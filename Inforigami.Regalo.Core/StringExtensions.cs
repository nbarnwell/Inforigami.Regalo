namespace Inforigami.Regalo.Core
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string input)
        {
            return input.Substring(0, 1).ToUpperInvariant() + input.Substring(1);
        }
    }
}