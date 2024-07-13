using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AURankedPlugin.Models
{
    public class Match
    {

        public int MatchID { get; set; }
        public string gameStarted { get; set; }
        public string players { get; set; }
        public string impostors { get; set; }
        public string eventsLogFile { get; set; }
        public string result { get; set; }
        public string reason { get; set; }

        public string MinifyJson(JsonElement jsonElement)
        {
            StringBuilder builder = new StringBuilder();
            WriteJsonElement(builder, jsonElement);
            return builder.ToString();
        }

        public void WriteJsonElement(StringBuilder builder, JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                    builder.Append('{');
                    bool first = true;
                    foreach (var property in jsonElement.EnumerateObject())
                    {
                        if (!first)
                            builder.Append(',');
                        builder.Append('"').Append(property.Name).Append('"').Append(':');
                        WriteJsonElement(builder, property.Value);
                        first = false;
                    }
                    builder.Append('}');
                    break;
                case JsonValueKind.Array:
                    builder.Append('[');
                    first = true;
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        if (!first)
                            builder.Append(',');
                        WriteJsonElement(builder, item);
                        first = false;
                    }
                    builder.Append(']');
                    break;
                case JsonValueKind.String:
                    builder.Append('"').Append(jsonElement.GetString()).Append('"');
                    break;
                case JsonValueKind.Number:
                    builder.Append(jsonElement.ToString());
                    break;
                case JsonValueKind.True:
                    builder.Append("true");
                    break;
                case JsonValueKind.False:
                    builder.Append("false");
                    break;
                case JsonValueKind.Null:
                    builder.Append("null");
                    break;
                default:
                    throw new InvalidOperationException("Invalid JSON value kind.");
            }
        }

    }
}
