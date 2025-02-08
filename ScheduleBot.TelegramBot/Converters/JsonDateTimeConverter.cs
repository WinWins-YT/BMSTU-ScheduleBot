using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScheduleBot.TelegramBot.Converters;

public class JsonDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString()!, "dd.MM.yyyy", new CultureInfo("ru-ru"));
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("dd.MM.yyyy"));
    }
}