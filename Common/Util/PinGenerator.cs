namespace Common.Util
{
    public class PinGenerator
    {
        public static string CreatePin()
        {
            return new Random(DateTime.UtcNow.Millisecond).Next(1000, 9999) + "";
        }
    }
}
