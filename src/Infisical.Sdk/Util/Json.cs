using System.Text.Json;

namespace Infisical.Sdk.Util
{
  /// <summary>
  /// Helpers to read the (slightly inconsistent) response envelopes of the Infisical API.
  /// </summary>
  internal static class ResponseReader
  {
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    public static T? Deserialize<T>(JsonElement element)
    {
      return JsonSerializer.Deserialize<T>(element.GetRawText(), Options);
    }

    /// <summary>Returns the value of the first matching envelope key, or the root element itself.</summary>
    public static JsonElement Unwrap(JsonElement root, params string[] keys)
    {
      if (root.ValueKind == JsonValueKind.Object)
      {
        foreach (var key in keys)
        {
          foreach (var prop in root.EnumerateObject())
          {
            if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
            {
              return prop.Value;
            }
          }
        }
      }
      return root;
    }

    public static T Entity<T>(JsonElement root, params string[] keys) where T : new()
    {
      var element = Unwrap(root, keys);
      return Deserialize<T>(element) ?? throw new InfisicalException("Failed to deserialize response content");
    }

    public static T[] List<T>(JsonElement root, params string[] keys)
    {
      var element = Unwrap(root, keys);
      if (element.ValueKind != JsonValueKind.Array)
      {
        return Array.Empty<T>();
      }
      return Deserialize<T[]>(element) ?? Array.Empty<T>();
    }
  }

  internal static class HttpMethods
  {
    public static readonly HttpMethod Patch = new HttpMethod("PATCH");
  }
}
