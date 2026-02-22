namespace Common.Extensions
{
    public static class StringExtension
    {
        public static bool IsNotNullOrWhiteSpace(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string ToStringSnippet(this string stringContent, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(stringContent) || stringContent.Length <= maxLength)
            {
                return stringContent;
            }
            
            return stringContent.Substring(0, maxLength) + "...";
        }
    }

    public static class StreamExtension
    {
        public static async Task<string> ReadString(this Stream stream)
        {
            var data = "";
            try
            {
                var reader = new StreamReader(stream);
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                data = await reader.ReadToEndAsync();
                reader.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return data;
        }
    }
}
