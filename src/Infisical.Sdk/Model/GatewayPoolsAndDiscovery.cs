using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Gateway pools
// ---------------------------------------------------------------------------------------------

public class GatewayPool : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("memberCount")]
  public int? MemberCount { get; set; }

  [JsonPropertyName("healthyMemberCount")]
  public int? HealthyMemberCount { get; set; }
}

// ---------------------------------------------------------------------------------------------
// PKI discovery
// ---------------------------------------------------------------------------------------------

public static class PkiDiscoveryTypes
{
  public const string Network = "network";
  public const string LinuxServer = "linux-server";
  public const string WindowsServer = "windows-server";
}

public class PkiDiscoveryJob : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("discoveryType")]
  public string? DiscoveryType { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("isAutoScanEnabled")]
  public bool? IsAutoScanEnabled { get; set; }

  [JsonPropertyName("scanIntervalDays")]
  public int? ScanIntervalDays { get; set; }

  [JsonPropertyName("isActive")]
  public bool? IsActive { get; set; }

  [JsonPropertyName("gatewayId")]
  public string? GatewayId { get; set; }

  [JsonPropertyName("gatewayPoolId")]
  public string? GatewayPoolId { get; set; }

  [JsonPropertyName("targetConfig")]
  public JsonElement? TargetConfig { get; set; }

  [JsonPropertyName("certificatesFound")]
  public int? CertificatesFound { get; set; }
}

/// <summary>
/// What a discovery job scans. Network jobs use <see cref="IpRanges"/>, <see cref="Domains"/> and <see cref="Ports"/>;
/// server jobs use <see cref="ConnectionIds"/> (SSH / WinRM app connections) plus type specific settings in
/// <see cref="Extra"/> (e.g. searchFolderPaths, skipFolderPaths, maxFolderDepth).
/// </summary>
public class PkiDiscoveryTargetConfig
{
  /// <summary>IP addresses or CIDR ranges (network).</summary>
  [JsonPropertyName("ipRanges")]
  public string[]? IpRanges { get; init; }

  [JsonPropertyName("domains")]
  public string[]? Domains { get; init; }

  /// <summary>Ports and ranges, e.g. "443,8443,9000-9010" (network).</summary>
  [JsonPropertyName("ports")]
  public string? Ports { get; init; }

  /// <summary>App connections used to reach the servers (linux-server / windows-server).</summary>
  [JsonPropertyName("connectionIds")]
  public string[]? ConnectionIds { get; init; }

  [JsonPropertyName("searchFolderPaths")]
  public string[]? SearchFolderPaths { get; init; }

  [JsonPropertyName("skipFolderPaths")]
  public string[]? SkipFolderPaths { get; init; }

  [JsonPropertyName("maxFolderDepth")]
  public int? MaxFolderDepth { get; init; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; init; }
}

public class PkiDiscoveryJobOptions
{
  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? DiscoveryId { get; init; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>See <see cref="PkiDiscoveryTypes"/> (default "network"). Only used when creating.</summary>
  [JsonPropertyName("discoveryType")]
  public string? DiscoveryType { get; init; }

  [JsonPropertyName("targetConfig")]
  public PkiDiscoveryTargetConfig? TargetConfig { get; init; }

  [JsonPropertyName("isAutoScanEnabled")]
  public bool? IsAutoScanEnabled { get; init; }

  [JsonPropertyName("scanIntervalDays")]
  public int? ScanIntervalDays { get; init; }

  /// <summary>Gateway that performs the scan. Mutually exclusive with <see cref="GatewayPoolId"/>.</summary>
  [JsonPropertyName("gatewayId")]
  public string? GatewayId { get; init; }

  [JsonPropertyName("gatewayPoolId")]
  public string? GatewayPoolId { get; init; }

  /// <summary>Only used when updating.</summary>
  [JsonPropertyName("isActive")]
  public bool? IsActive { get; init; }
}

public class PkiInstallation : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("discoveryName")]
  public string? DiscoveryName { get; set; }

  [JsonPropertyName("certificatesCount")]
  public int? CertificatesCount { get; set; }
}

public class ListPkiInstallationsOptions
{
  public string? ProjectId { get; init; }
  public string? DiscoveryId { get; init; }
  public string? CertificateId { get; init; }
  public string? Search { get; init; }
  public int? Offset { get; init; }
  public int? Limit { get; init; }
}
