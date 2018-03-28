namespace Inforigami.Regalo.Core
{
    public static class EntityVersion
    {
        public const int New = -1;
        public const int Latest = -2;

        public static string GetName(int value)
        {
            switch (value)
            {
                case New:
                    return "No-stream";
                case Latest:
                    return "Max";
                default:
                    return value.ToString();
            }
        }
    }
}