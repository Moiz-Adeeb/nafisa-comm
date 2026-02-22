namespace Common.Util
{
    public class StringGenerator
    {
        const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string RandomString(int length)
        {
            var random = new Random((int)DateTime.UtcNow.Ticks);
            return new string(
                Enumerable.Repeat(Chars, length).Select(s => s[random.Next(s.Length)]).ToArray()
            );
        }

        public static string Password()
        {
            return "Tam@12345678";
        }

        public static string GuidString()
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            return guid;
        }
    }
}
