namespace Common.Util
{
    public class DateConverter
    {
        public string ConvertToShortDate(DateTime dateTime)
        {
            return dateTime.ToShortDateString();
        }

        public string ConvertToDateTime(DateTime dateTime)
        {
            return dateTime.ToString("g");
        }
    }
}
