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
        
        public static bool IsBase64(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;

            // 1. Check for Data URI prefix from frontend
            if (str.Contains(",")) return true; 

            // 2. Technical check for raw Base64 string
            Span<byte> buffer = new Span<byte>(new byte[str.Length]);
            return Convert.TryFromBase64String(str, buffer, out _);
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
