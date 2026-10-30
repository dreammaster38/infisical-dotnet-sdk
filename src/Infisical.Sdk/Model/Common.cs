using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Base class of all response models. Properties the SDK does not model explicitly are kept in
/// <see cref="AdditionalData"/>, so no information of an API response is lost.
/// </summary>
public abstract class ApiModel
{
  [JsonExtensionData]
  public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class IpEntry
{
  public IpEntry() { }

  public IpEntry(string ipAddress)
  {
    IpAddress = ipAddress;
  }

  [JsonPropertyName("ipAddress")]
  public string IpAddress { get; set; } = string.Empty;
}

public class PagedResult<T>
{
  public PagedResult(T[] items, int? totalCount)
  {
    Items = items;
    TotalCount = totalCount;
  }

  public T[] Items { get; }

  public int? TotalCount { get; }
}
