using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Extensions;

namespace WebApi.Formatters
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var data = reader.GetString();
            return data == null ? TimeSpan.MinValue : TimeSpan.Parse(data);
        }

        public override void Write(
            Utf8JsonWriter writer,
            TimeSpan value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var data = reader.GetString();
            return data == null ? DateTime.MinValue : DateTime.Parse(data);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options
        )
        {
            var s = value.ToGeneralDateTime();
            writer.WriteStringValue(s);
        }
    }

    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var data = reader.GetString();
            return data == null ? DateOnly.MinValue : DateOnly.Parse(data);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateOnly value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStringValue(value.ToGeneralDate());
        }
    }
}
