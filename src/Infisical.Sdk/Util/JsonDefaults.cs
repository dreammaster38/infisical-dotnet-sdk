using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infisical.Sdk.Util
{
  /// <summary>
  /// Lenient reading options shared by all clients. A field the SDK models as text also accepts a number or boolean
  /// from the server, and a numeric field accepts a numeric string, so a small difference between the SDK's model and
  /// the server's actual response does not make the whole call fail.
  /// </summary>
  internal static class JsonDefaults
  {
    public static readonly JsonSerializerOptions Read = Create();

    private static JsonSerializerOptions Create()
    {
      var options = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
      };
      options.Converters.Add(new LenientStringConverter());
      return options;
    }

    private sealed class LenientStringConverter : JsonConverter<string>
    {
      public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        switch (reader.TokenType)
        {
          case JsonTokenType.String:
            return reader.GetString();
          case JsonTokenType.Number:
            using (var doc = JsonDocument.ParseValue(ref reader)) return doc.RootElement.GetRawText();
          case JsonTokenType.True:
            return "true";
          case JsonTokenType.False:
            return "false";
          case JsonTokenType.Null:
            return null;
          default:
            // objects / arrays: keep the JSON text instead of failing
            using (var doc = JsonDocument.ParseValue(ref reader)) return doc.RootElement.GetRawText();
        }
      }

      public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
      {
        writer.WriteStringValue(value);
      }
    }
  }
}
