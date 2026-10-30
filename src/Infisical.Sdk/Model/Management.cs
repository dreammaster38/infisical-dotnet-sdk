using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Dynamic secrets
// ---------------------------------------------------------------------------------------------

public class DynamicSecret : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("type")]
  public string? Type { get; set; }

  [JsonPropertyName("defaultTTL")]
  public string? DefaultTTL { get; set; }

  [JsonPropertyName("maxTTL")]
  public string? MaxTTL { get; set; }

  [JsonPropertyName("status")]
  public string? Status { get; set; }
}

/// <summary>Location (project / environment / folder) of a dynamic secret.</summary>
public class DynamicSecretLocation
{
  [JsonPropertyName("projectSlug")]
  public string ProjectSlug { get; init; } = string.Empty;

  [JsonPropertyName("environmentSlug")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("path")]
  public string Path { get; init; } = "/";

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectSlug)) throw new InfisicalException("ProjectSlug is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
  }
}

public class DynamicSecretProvider
{
  /// <summary>Provider type, e.g. "sql-database", "aws-iam", "redis", ...</summary>
  [JsonPropertyName("type")]
  public string Type { get; init; } = string.Empty;

  /// <summary>Provider specific configuration, see the "Create Dynamic Secret" API reference.</summary>
  [JsonPropertyName("inputs")]
  public Dictionary<string, object?> Inputs { get; init; } = new Dictionary<string, object?>();
}

public class CreateDynamicSecretOptions : DynamicSecretLocation
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("provider")]
  public DynamicSecretProvider Provider { get; init; } = new DynamicSecretProvider();

  /// <summary>Default lease TTL, e.g. "1h".</summary>
  [JsonPropertyName("defaultTTL")]
  public string DefaultTTL { get; init; } = "1h";

  [JsonPropertyName("maxTTL")]
  public string? MaxTTL { get; init; }

  [JsonPropertyName("metadata")]
  public IdentityMetadataEntry[]? Metadata { get; init; }

  [JsonPropertyName("usernameTemplate")]
  public string? UsernameTemplate { get; init; }
}

public class UpdateDynamicSecretOptions : DynamicSecretLocation
{
  [JsonIgnore]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("data")]
  public UpdateDynamicSecretData Data { get; init; } = new UpdateDynamicSecretData();
}

public class UpdateDynamicSecretData
{
  [JsonPropertyName("newName")]
  public string? NewName { get; init; }

  [JsonPropertyName("defaultTTL")]
  public string? DefaultTTL { get; init; }

  [JsonPropertyName("maxTTL")]
  public string? MaxTTL { get; init; }

  [JsonPropertyName("inputs")]
  public Dictionary<string, object?>? Inputs { get; init; }

  [JsonPropertyName("metadata")]
  public IdentityMetadataEntry[]? Metadata { get; init; }

  [JsonPropertyName("usernameTemplate")]
  public string? UsernameTemplate { get; init; }
}

public class DeleteDynamicSecretOptions : DynamicSecretLocation
{
  [JsonIgnore]
  public string Name { get; init; } = string.Empty;

  /// <summary>Also delete the dynamic secret if revoking its leases fails.</summary>
  [JsonPropertyName("isForced")]
  public bool? IsForced { get; init; }
}

public class DynamicSecretLease : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("dynamicSecretId")]
  public string? DynamicSecretId { get; set; }

  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("expireAt")]
  public string? ExpireAt { get; set; }
}

public class CreateDynamicSecretLeaseOptions : DynamicSecretLocation
{
  [JsonPropertyName("dynamicSecretName")]
  public string DynamicSecretName { get; init; } = string.Empty;

  /// <summary>Lease TTL, e.g. "30m". Defaults to the default TTL of the dynamic secret.</summary>
  [JsonPropertyName("ttl")]
  public string? Ttl { get; init; }

  /// <summary>Provider specific lease configuration.</summary>
  [JsonPropertyName("config")]
  public Dictionary<string, object?>? Config { get; init; }
}

public class DynamicSecretLeaseResult
{
  public DynamicSecretLeaseResult(DynamicSecretLease lease, DynamicSecret dynamicSecret, Dictionary<string, JsonElement> data)
  {
    Lease = lease;
    DynamicSecret = dynamicSecret;
    Data = data;
  }

  public DynamicSecretLease Lease { get; }
  public DynamicSecret DynamicSecret { get; }

  /// <summary>The generated credentials (provider specific, e.g. DB_USERNAME / DB_PASSWORD).</summary>
  public Dictionary<string, JsonElement> Data { get; }
}

public class RenewDynamicSecretLeaseOptions : DynamicSecretLocation
{
  [JsonIgnore]
  public string LeaseId { get; init; } = string.Empty;

  [JsonPropertyName("ttl")]
  public string? Ttl { get; init; }
}

public class DeleteDynamicSecretLeaseOptions : DynamicSecretLocation
{
  [JsonIgnore]
  public string LeaseId { get; init; } = string.Empty;

  [JsonPropertyName("isForced")]
  public bool? IsForced { get; init; }
}

// ---------------------------------------------------------------------------------------------
// Groups
// ---------------------------------------------------------------------------------------------

public class Group : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string? Slug { get; set; }

  [JsonPropertyName("orgId")]
  public string? OrgId { get; set; }

  [JsonPropertyName("role")]
  public string? Role { get; set; }
}

public class CreateGroupOptions
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  /// <summary>Organization role of the group members, e.g. "no-access", "member".</summary>
  [JsonPropertyName("role")]
  public string? Role { get; init; }
}

public class UpdateGroupOptions
{
  [JsonIgnore]
  public string GroupId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("role")]
  public string? Role { get; init; }
}

// ---------------------------------------------------------------------------------------------
// KMS
// ---------------------------------------------------------------------------------------------

public class KmsKey : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("isDisabled")]
  public bool? IsDisabled { get; set; }

  [JsonPropertyName("keyUsage")]
  public string? KeyUsage { get; set; }

  [JsonPropertyName("encryptionAlgorithm")]
  public string? EncryptionAlgorithm { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("version")]
  public int? Version { get; set; }
}

public class CreateKmsKeyOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>"encrypt-decrypt" (default) or "sign-verify".</summary>
  [JsonPropertyName("keyUsage")]
  public string? KeyUsage { get; init; }

  /// <summary>E.g. "aes-256-gcm", "aes-128-gcm", "RSA_4096", "ECC_NIST_P256".</summary>
  [JsonPropertyName("encryptionAlgorithm")]
  public string? EncryptionAlgorithm { get; init; }

  [JsonPropertyName("isExportable")]
  public bool? IsExportable { get; init; }

  [JsonPropertyName("hasDeleteProtection")]
  public bool? HasDeleteProtection { get; init; }
}

public class UpdateKmsKeyOptions
{
  [JsonIgnore]
  public string KeyId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("isDisabled")]
  public bool? IsDisabled { get; init; }

  [JsonPropertyName("hasDeleteProtection")]
  public bool? HasDeleteProtection { get; init; }
}

public class ListKmsKeysOptions
{
  public string ProjectId { get; init; } = string.Empty;
  public int? Offset { get; init; }
  public int? Limit { get; init; }
  /// <summary>"name" (default).</summary>
  public string? OrderBy { get; init; }
  /// <summary>"asc" (default) or "desc".</summary>
  public string? OrderDirection { get; init; }
  public string? Search { get; init; }
}

public class KmsKeyList
{
  public KmsKeyList(KmsKey[] keys, int? totalCount)
  {
    Keys = keys;
    TotalCount = totalCount;
  }

  public KmsKey[] Keys { get; }
  public int? TotalCount { get; }
}

public class KmsSignResult
{
  [JsonPropertyName("signature")]
  public string Signature { get; set; } = string.Empty;

  [JsonPropertyName("keyId")]
  public string? KeyId { get; set; }

  [JsonPropertyName("signingAlgorithm")]
  public string? SigningAlgorithm { get; set; }
}

public class KmsVerifyResult
{
  [JsonPropertyName("signatureValid")]
  public bool SignatureValid { get; set; }

  [JsonPropertyName("keyId")]
  public string? KeyId { get; set; }

  [JsonPropertyName("signingAlgorithm")]
  public string? SigningAlgorithm { get; set; }
}

// ---------------------------------------------------------------------------------------------
// Shared secrets
// ---------------------------------------------------------------------------------------------

public class SharedSecret : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("expiresAt")]
  public string? ExpiresAt { get; set; }

  [JsonPropertyName("expiresAfterViews")]
  public int? ExpiresAfterViews { get; set; }
}

public class CreateSharedSecretOptions
{
  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("secretValue")]
  public string SecretValue { get; init; } = string.Empty;

  [JsonPropertyName("password")]
  public string? Password { get; init; }

  /// <summary>Lifetime of the shared secret, between "5m" and "30d". Defaults to "30d".</summary>
  [JsonPropertyName("expiresIn")]
  public string? ExpiresIn { get; init; }

  [JsonPropertyName("maxViews")]
  public int? MaxViews { get; init; }

  /// <summary>"anyone" or "organization".</summary>
  [JsonPropertyName("accessType")]
  public string? AccessType { get; init; }

  [JsonPropertyName("authorizedEmails")]
  public string[]? AuthorizedEmails { get; init; }

  [JsonPropertyName("allowExternalEmails")]
  public bool? AllowExternalEmails { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(SecretValue)) throw new InfisicalException("SecretValue is required");
  }
}

// ---------------------------------------------------------------------------------------------
// Project memberships and roles
// ---------------------------------------------------------------------------------------------

public class ProjectRoleAssignment
{
  [JsonPropertyName("role")]
  public string Role { get; init; } = string.Empty;

  [JsonPropertyName("isTemporary")]
  public bool IsTemporary { get; init; } = false;

  /// <summary>"relative" - only for temporary roles.</summary>
  [JsonPropertyName("temporaryMode")]
  public string? TemporaryMode { get; init; }

  /// <summary>E.g. "1h" - only for temporary roles.</summary>
  [JsonPropertyName("temporaryRange")]
  public string? TemporaryRange { get; init; }

  /// <summary>ISO-8601 timestamp - only for temporary roles.</summary>
  [JsonPropertyName("temporaryAccessStartTime")]
  public string? TemporaryAccessStartTime { get; init; }
}

public class ProjectRole : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string Slug { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("permissions")]
  public JsonElement? Permissions { get; set; }
}

public class ProjectRoleOptions
{
  [JsonIgnore]
  public string ProjectId { get; init; } = string.Empty;

  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? RoleId { get; init; }

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>Permission rules, e.g. <c>new { subject = "secrets", action = new[] { "read" } }</c>.</summary>
  [JsonPropertyName("permissions")]
  public object[]? Permissions { get; init; }
}
