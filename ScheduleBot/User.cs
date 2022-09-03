using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScheduleBot
{
    internal record User
    {
        public long? Id { get; set; }
        public string Group { get; set; }
        public bool IsAlarmOn { get; set; }
        public TimeOnly AlarmTime { get; set; }
        public Location Location { get; set; }
    }

    internal enum Location
    {
        Menu, Settings, Alarm
    }

    class JsonTimeOnlyConverter : JsonConverter<TimeOnly>
    {
        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => TimeOnly.Parse(reader.GetString());

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
