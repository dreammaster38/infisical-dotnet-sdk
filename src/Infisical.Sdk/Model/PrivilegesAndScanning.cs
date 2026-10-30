using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Additional privileges of machine identities
// ---------------------------------------------------------------------------------------------

public class IdentityAdditionalPrivilege : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string Slug { get; set; } = string.Empty;

  [JsonPropertyName("projectMembershipId")]
  public string? ProjectMembershipId { get; set; }

  [JsonPropertyName("isTemporary")]
  public bool? IsTemporary { get; set; }

  [JsonPropertyName("temporaryMode")]
  public string? TemporaryMode { get; set; }

  [JsonPropertyName("temporaryRange")]
  public string? TemporaryRange { get; set; }

  [JsonPropertyName("temporaryAccessStartTime")]
  public string? TemporaryAccessStartTime { get; set; }

  [JsonPropertyName("temporaryAccessEndTime")]
  public string? TemporaryAccessEndTime { get; set; }

  [JsonPropertyName("permissions")]
  public JsonElement? Permissions { get; set; }
}

public class PrivilegeValidity
{
  [JsonPropertyName("isTemporary")]
  public bool IsTemporary { get; init; }

  /// <summary>"relative" - only for temporary privileges.</summary>
  [JsonPropertyName("temporaryMode")]
  public string? TemporaryMode { get; init; }

  /// <summary>Duration such as "1h" - only for temporary privileges.</summary>
  [JsonPropertyName("temporaryRange")]
  public string? TemporaryRange { get; init; }

  /// <summary>ISO-8601 timestamp - only for temporary privileges.</summary>
  [JsonPropertyName("temporaryAccessStartTime")]
  public string? TemporaryAccessStartTime { get; init; }

  public static PrivilegeValidity Permanent() => new PrivilegeValidity { IsTemporary = false };

  public static PrivilegeValidity Temporary(string range, DateTime startTimeUtc) => new PrivilegeValidity
  {
    IsTemporary = true,
    TemporaryMode = "relative",
    TemporaryRange = range,
    TemporaryAccessStartTime = startTimeUtc.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'")
  };
}

public class CreateIdentityPrivilegeOptions
{
  [JsonPropertyName("identityId")]
  public string IdentityId { get; init; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  /// <summary>Permission rules, e.g. <c>new { subject = "secrets", action = new[] { "read" } }</c>.</summary>
  [JsonPropertyName("permissions")]
  public object[] Permissions { get; init; } = Array.Empty<object>();

  [JsonPropertyName("type")]
  public PrivilegeValidity Type { get; init; } = PrivilegeValidity.Permanent();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(IdentityId)) throw new InfisicalException("IdentityId is required");
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
  }
}

public class UpdateIdentityPrivilegeOptions
{
  [JsonIgnore]
  public string PrivilegeId { get; init; } = string.Empty;

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("permissions")]
  public object[]? Permissions { get; init; }

  [JsonPropertyName("type")]
  public PrivilegeValidity? Type { get; init; }
}

// ---------------------------------------------------------------------------------------------
// Secret scanning
// ---------------------------------------------------------------------------------------------

/// <summary>Types of secret scanning data sources.</summary>
public static class SecretScanningDataSourceTypes
{
  public const string GitHub = "github";
  public const string Bitbucket = "bitbucket";
  public const string GitLab = "gitlab";
}

public static class SecretScanningFindingStatuses
{
  public const string Resolved = "resolved";
  public const string Unresolved = "unresolved";
  public const string FalsePositive = "false-positive";
  public const string Ignore = "ignore";
}

public class SecretScanningDataSource : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("type")]
  public string Type { get; set; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("isAutoScanEnabled")]
  public bool? IsAutoScanEnabled { get; set; }

  [JsonPropertyName("isHistoricalScanEnabled")]
  public bool? IsHistoricalScanEnabled { get; set; }

  [JsonPropertyName("isScanNewCommitsEnabled")]
  public bool? IsScanNewCommitsEnabled { get; set; }

  [JsonPropertyName("config")]
  public JsonElement? Config { get; set; }

  [JsonPropertyName("connection")]
  public JsonElement? Connection { get; set; }
}

public class CreateSecretScanningDataSourceOptions
{
  /// <summary>One of <see cref="SecretScanningDataSourceTypes"/>.</summary>
  [JsonIgnore]
  public string Type { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>App connection used to access the data source (a "github-radar" connection for GitHub).</summary>
  [JsonPropertyName("connectionId")]
  public string? ConnectionId { get; init; }

  [JsonPropertyName("isAutoScanEnabled")]
  public bool? IsAutoScanEnabled { get; init; }

  [JsonPropertyName("isHistoricalScanEnabled")]
  public bool? IsHistoricalScanEnabled { get; init; }

  [JsonPropertyName("isScanNewCommitsEnabled")]
  public bool? IsScanNewCommitsEnabled { get; init; }

  /// <summary>Type specific configuration (empty for GitHub).</summary>
  [JsonPropertyName("config")]
  public Dictionary<string, object?> Config { get; init; } = new Dictionary<string, object?>();
}

public class UpdateSecretScanningDataSourceOptions
{
  [JsonIgnore]
  public string Type { get; init; } = string.Empty;

  [JsonIgnore]
  public string DataSourceId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("isAutoScanEnabled")]
  public bool? IsAutoScanEnabled { get; init; }

  [JsonPropertyName("isHistoricalScanEnabled")]
  public bool? IsHistoricalScanEnabled { get; init; }

  [JsonPropertyName("isScanNewCommitsEnabled")]
  public bool? IsScanNewCommitsEnabled { get; init; }

  [JsonPropertyName("config")]
  public Dictionary<string, object?>? Config { get; init; }
}

public class SecretScanningFinding : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("ruleId")]
  public string? RuleId { get; set; }

  [JsonPropertyName("severity")]
  public string? Severity { get; set; }

  [JsonPropertyName("confidence")]
  public string? Confidence { get; set; }

  [JsonPropertyName("dataSourceId")]
  public string? DataSourceId { get; set; }

  [JsonPropertyName("resourceName")]
  public string? ResourceName { get; set; }

  [JsonPropertyName("triageComment")]
  public string? TriageComment { get; set; }

  [JsonPropertyName("details")]
  public JsonElement? Details { get; set; }
}

public class ListSecretScanningFindingsOptions
{
  public string? ProjectId { get; init; }
  public string? Status { get; init; }
  public string? DataSourceId { get; init; }
  public string? ResourceId { get; init; }
  /// <summary>"createdAt", "rule", "resourceName", "author" or "status".</summary>
  public string? OrderBy { get; init; }
  /// <summary>"asc" or "desc".</summary>
  public string? OrderDirection { get; init; }
  public int? Offset { get; init; }
  public int? Limit { get; init; }
}

public class SecretScanningFindingList
{
  public SecretScanningFindingList(SecretScanningFinding[] findings, int? totalCount)
  {
    Findings = findings;
    TotalCount = totalCount;
  }

  public SecretScanningFinding[] Findings { get; }
  public int? TotalCount { get; }
}

public class SecretScanningFindingUpdate
{
  [JsonPropertyName("findingId")]
  public string FindingId { get; init; } = string.Empty;

  /// <summary>See <see cref="SecretScanningFindingStatuses"/>.</summary>
  [JsonPropertyName("status")]
  public string? Status { get; init; }

  [JsonPropertyName("triageComment")]
  public string? TriageComment { get; init; }
}
