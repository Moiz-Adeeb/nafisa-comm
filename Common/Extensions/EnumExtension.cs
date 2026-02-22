namespace Common.Extensions
{
    public static class EnumExtension
    {
        public static int ToInt<T>(this T source)
            where T : IConvertible //enum
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            return (int)(IConvertible)source;
        }

        public static List<T> GetAllEnumValues<T>()
            where T : Enum
        {
            return [.. (T[])Enum.GetValues(typeof(T))];
        }
    }
}
