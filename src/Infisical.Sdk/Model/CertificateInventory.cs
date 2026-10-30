using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Columns that can be shown in a certificate inventory view.</summary>
public static class CertificateInventoryColumns
{
  public const string SanCn = "sanCn";
  public const string SerialNumber = "serialNumber";
  public const string EnrollmentMethod = "enrollmentMethod";
  public const string Status = "status";
  public const string Health = "health";
  public const string IssuedAt = "issuedAt";
  public const string ExpiresAt = "expiresAt";
  public const string Ca = "ca";
  public const string Profile = "profile";
  public const string Application = "application";
  public const string Algorithm = "algorithm";
  public const string Source = "source";
}

public class CertificateInventoryFilters
{
  [JsonPropertyName("status")]
  public string? Status { get; init; }

  /// <summary>ISO-8601 timestamps.</summary>
  [JsonPropertyName("notAfterFrom")]
  public string? NotAfterFrom { get; init; }

  [JsonPropertyName("notAfterTo")]
  public string? NotAfterTo { get; init; }

  [JsonPropertyName("notBeforeFrom")]
  public string? NotBeforeFrom { get; init; }

  [JsonPropertyName("notBeforeTo")]
  public string? NotBeforeTo { get; init; }

  [JsonPropertyName("enrollmentTypes")]
  public string[]? EnrollmentTypes { get; init; }

  /// <summary>A key algorithm or an array of them (pass a string or string[]).</summary>
  [JsonPropertyName("keyAlgorithm")]
  public object? KeyAlgorithm { get; init; }

  [JsonPropertyName("keySizes")]
  public int[]? KeySizes { get; init; }

  [JsonPropertyName("caIds")]
  public string[]? CaIds { get; init; }

  [JsonPropertyName("profileIds")]
  public string[]? ProfileIds { get; init; }

  [JsonPropertyName("applicationIds")]
  public string[]? ApplicationIds { get; init; }

  /// <summary>A source or an array of them (pass a string or string[]).</summary>
  [JsonPropertyName("source")]
  public object? Source { get; init; }

  [JsonPropertyName("metadata")]
  public IdentityMetadataEntry[]? Metadata { get; init; }
}

public class CertificateInventoryView : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("isShared")]
  public bool? IsShared { get; set; }

  [JsonPropertyName("isSystem")]
  public bool? IsSystem { get; set; }

  [JsonPropertyName("filters")]
  public JsonElement? Filters { get; set; }

  [JsonPropertyName("columns")]
  public JsonElement? Columns { get; set; }

  [JsonPropertyName("createdByUserId")]
  public string? CreatedByUserId { get; set; }
}

public class CertificateInventoryViews
{
  public CertificateInventoryViews(CertificateInventoryView[] systemViews, CertificateInventoryView[] sharedViews, CertificateInventoryView[] customViews)
  {
    SystemViews = systemViews;
    SharedViews = sharedViews;
    CustomViews = customViews;
  }

  /// <summary>Built-in views (e.g. expiring soon).</summary>
  public CertificateInventoryView[] SystemViews { get; }

  /// <summary>Views shared with the project.</summary>
  public CertificateInventoryView[] SharedViews { get; }

  /// <summary>Views of the current user.</summary>
  public CertificateInventoryView[] CustomViews { get; }
}

public class CertificateInventoryViewOptions
{
  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? ViewId { get; init; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("filters")]
  public CertificateInventoryFilters? Filters { get; init; }

  /// <summary>See <see cref="CertificateInventoryColumns"/>.</summary>
  [JsonPropertyName("columns")]
  public string[]? Columns { get; init; }

  [JsonPropertyName("isShared")]
  public bool? IsShared { get; init; }

  /// <summary>Only used when creating.</summary>
  [JsonPropertyName("applicationId")]
  public string? ApplicationId { get; init; }
}
