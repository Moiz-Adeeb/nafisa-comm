namespace Common.Extensions
{
    public static class StreamExtensions
    {
        public static string StreamToBase64(this Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            // Reset position if possible
            if (inputStream.CanSeek)
                inputStream.Position = 0;

            using (var memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }
    }

    public static class ClassExtensions
    {
        public static void SetProperty<T>(this T parent)
            where T : class, new() { }

        public static T CreateCopy<T>(this object parent)
            where T : class, new()
        {
            T target = new T();
            var parentProperties = parent.GetType().GetProperties();
            var targetProperties = target.GetType().GetProperties();
            foreach (var parentProperty in parentProperties)
            {
                foreach (var targetProperty in targetProperties)
                {
                    if (
                        parentProperty.Name == targetProperty.Name
                        && parentProperty.PropertyType == targetProperty.PropertyType
                    )
                    {
                        targetProperty.SetValue(target, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// This functions copies all matching fields into the parameter child
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public static void Copy(this object parent, object child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (
                        parentProperty.Name == childProperty.Name
                        && parentProperty.PropertyType == childProperty.PropertyType
                    )
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }
    }
}
