using TimeZoneConverter;

namespace Common.Extensions
{
    public static class DateExtension
    {
        public static DateOnly ToDateOnly(this DateTime dateTime)
        {
            return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        public static TimeOnly ToTimeOnly(this TimeSpan span)
        {
            return new TimeOnly(span.Hours, span.Minutes, span.Seconds);
        }

        public static DateTime ToDateTimeZoneUtc(this DateTimeOffset? dateTime, string timeZone)
        {
            if (timeZone.IsNullOrWhiteSpace() || dateTime == null)
            {
                if (dateTime.HasValue)
                {
                    return dateTime.GetValueOrDefault().UtcDateTime;
                }
                return DateTime.MinValue;
            }
            var tzi = TZConvert.GetTimeZoneInfo(timeZone);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime.Value.Date, tzi);
        }

        public static DateTime ToDateTimeZoneUtc(this DateTimeOffset dateTime, string timeZone)
        {
            if (timeZone.IsNullOrWhiteSpace())
            {
                return dateTime.UtcDateTime;
            }
            var tzi = TZConvert.GetTimeZoneInfo(timeZone);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime.Date, tzi);
        }

        public static DateTimeOffset ToDateTimeZoneUtc(this DateTime dateTime, string timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
            {
                // If no timezone specified, assume it's already UTC
                return new DateTimeOffset(dateTime, TimeSpan.Zero);
            }
            var tzi = TZConvert.GetTimeZoneInfo(timeZone);

            // Create DateTimeOffset from the local time in the specified timezone
            var localDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            var offset = tzi.GetUtcOffset(localDateTime);
            var dateTimeOffset = new DateTimeOffset(localDateTime, offset);

            // Convert to UTC
            return dateTimeOffset.ToUniversalTime();
        }

        public static DateTimeOffset FromUtcToTimeZone(
            this DateTimeOffset utcDateTimeOffset,
            string timeZone
        )
        {
            if (timeZone.IsNullOrWhiteSpace())
            {
                return utcDateTimeOffset.ToUniversalTime();
            }

            var tzi = TZConvert.GetTimeZoneInfo(timeZone);
            return TimeZoneInfo.ConvertTime(utcDateTimeOffset, tzi);
        }

        // Helper method to convert DateTime to DateTimeOffset with explicit timezone
        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime, string timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
            {
                // Assume UTC if no timezone provided
                return new DateTimeOffset(dateTime, TimeSpan.Zero);
            }

            var tzi = TZConvert.GetTimeZoneInfo(timeZone);
            var localDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            var offset = tzi.GetUtcOffset(localDateTime);

            return new DateTimeOffset(localDateTime, offset);
        }

        public static DateTime ToDateTimeZoneUtc(this DateTime? dateTime, string timeZone)
        {
            if (timeZone.IsNullOrWhiteSpace() || dateTime == null)
            {
                return dateTime ?? DateTime.MinValue;
            }
            var tzi = TZConvert.GetTimeZoneInfo(timeZone);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime.Value.Date, tzi);
        }

        public static string ToGeneralDateTime(this long ticks)
        {
            if (ticks == 0)
            {
                return "";
            }
            DateTime myDate = new DateTime(ticks);
            return myDate.ToGeneralDateTime();
        }

        public static string ToDayAndDate(this DateTime dateTime)
        {
            return dateTime.ToString("ddd dd-MMM");
        }

        public static string ToDayAndDate(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return dateTime.GetValueOrDefault().ToString("ddd dd-MMM");
            }
            return "";
        }

        public static string ToShortDate(this DateTime dateTime)
        {
            return dateTime.ToShortDateString();
        }

        public static string ToGeneralDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("u").Replace(" ", "T");
        }

        public static string ToGeneralDate(this DateOnly dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static string ToGeneralDate(this DateOnly? dateTime)
        {
            return dateTime?.ToString("yyyy-MM-dd");
        }

        public static string ToGeneralDateTime(this DateTime? dateTime)
        {
            return !dateTime.HasValue
                ? ""
                : dateTime.GetValueOrDefault().ToString("u").Replace(" ", "T");
        }

        public static string ToUtc(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToGeneralDateTime();
        }

        public static DateTime ToDateTimeUtc(this long val)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddSeconds(val);
        }

        public static DateTimeOffset FromUnixTimeSeconds(this long seconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        public static DateTimeOffset FromUnixTimeMilliseconds(this long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        }
    }
}
