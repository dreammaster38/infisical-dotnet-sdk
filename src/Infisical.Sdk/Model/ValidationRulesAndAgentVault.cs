using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Secret validation rules
// ---------------------------------------------------------------------------------------------

public static class SecretValidationRuleTypes
{
  public const string StaticSecrets = "static-secrets";
  public const string DynamicSecrets = "dynamic-secrets";
  public const string SecretRotations = "secret-rotations";
}

public class SecretValidationRule : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("type")]
  public string? Type { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("secretPath")]
  public string? SecretPath { get; set; }

  [JsonPropertyName("isActive")]
  public bool? IsActive { get; set; }

  [JsonPropertyName("environment")]
  public JsonElement? Environment { get; set; }

  [JsonPropertyName("keyConstraints")]
  public JsonElement? KeyConstraints { get; set; }

  [JsonPropertyName("valueConstraints")]
  public JsonElement? ValueConstraints { get; set; }
}

/// <summary>Constraints for a secret key, value or generated password. Fields that are left out are not enforced.</summary>
public class SecretConstraints
{
  [JsonPropertyName("minLength")]
  public int? MinLength { get; init; }

  [JsonPropertyName("maxLength")]
  public int? MaxLength { get; init; }

  [JsonPropertyName("regexPattern")]
  public string? RegexPattern { get; init; }

  [JsonPropertyName("requiredPrefix")]
  public string? RequiredPrefix { get; init; }

  [JsonPropertyName("requiredSuffix")]
  public string? RequiredSuffix { get; init; }

  /// <summary>The value must differ from its last N versions (secret values only).</summary>
  [JsonPropertyName("uniqueAcrossLastVersions")]
  public int? UniqueAcrossLastVersions { get; init; }

  /// <summary>The value must not be used by another secret in the scope (secret values only).</summary>
  [JsonPropertyName("uniqueWithinScope")]
  public bool? UniqueWithinScope { get; init; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; init; }
}

public class SecretValidationRuleOptions
{
  /// <summary>One of <see cref="SecretValidationRuleTypes"/>.</summary>
  [JsonIgnore]
  public string Type { get; init; } = string.Empty;

  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? RuleId { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  /// <summary>Only used when creating.</summary>
  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>Restricts the rule to an environment. Omit to apply it to all environments.</summary>
  [JsonPropertyName("environment")]
  public string? EnvironmentSlug { get; init; }

  [JsonPropertyName("secretPath")]
  public string? SecretPath { get; init; }

  [JsonPropertyName("isActive")]
  public bool? IsActive { get; init; }

  /// <summary>Static secrets: constraints for the secret key.</summary>
  [JsonPropertyName("keyConstraints")]
  public SecretConstraints? KeyConstraints { get; init; }

  /// <summary>Static secrets: constraints for the secret value.</summary>
  [JsonPropertyName("valueConstraints")]
  public SecretConstraints? ValueConstraints { get; init; }

  /// <summary>Type specific settings (e.g. provider and password constraints of dynamic secret / rotation rules).</summary>
  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; init; }
}

// ---------------------------------------------------------------------------------------------
// Agent Vault
// ---------------------------------------------------------------------------------------------

/// <summary>Generic Agent Vault resource (access bundle, variable, skill, proxy, session, ...).</summary>
public class AgentVaultResource : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("description")]
  public string? Description { get; set; }
}

public class AgentVaultListOptions
{
  public string? Search { get; init; }
  public int? Limit { get; init; }
  public int? Offset { get; init; }
}

public class AgentVaultMemberIds
{
  [JsonPropertyName("userIds")]
  public string[] UserIds { get; init; } = Array.Empty<string>();

  [JsonPropertyName("machineIdentityIds")]
  public string[] MachineIdentityIds { get; init; } = Array.Empty<string>();

  [JsonPropertyName("groupIds")]
  public string[] GroupIds { get; init; } = Array.Empty<string>();
}

public class AgentVaultProductMembers : AgentVaultMemberIds
{
  /// <summary>Emails of users to invite (lowercase). Only used when adding.</summary>
  [JsonPropertyName("emails")]
  public string[]? Emails { get; init; }

  /// <summary>"admin" or "member". Not used when removing.</summary>
  [JsonPropertyName("role")]
  public string? Role { get; init; }
}

public class AgentVaultVariableOptions
{
  [JsonIgnore]
  public string? VariableId { get; init; }

  /// <summary>Variable scope, see the API reference (e.g. "shared" or "personal").</summary>
  [JsonPropertyName("scope")]
  public string? Scope { get; init; }

  [JsonPropertyName("key")]
  public string? Key { get; init; }

  [JsonPropertyName("value")]
  public string? Value { get; init; }
}

public class AgentVaultSkillFile
{
  [JsonPropertyName("path")]
  public string Path { get; init; } = string.Empty;

  /// <summary>File mode as listed in the API reference.</summary>
  [JsonPropertyName("mode")]
  public string Mode { get; init; } = string.Empty;

  [JsonPropertyName("contentBase64")]
  public string ContentBase64 { get; init; } = string.Empty;

  public static AgentVaultSkillFile FromText(string path, string mode, string text)
    => new AgentVaultSkillFile { Path = path, Mode = mode, ContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text)) };
}

public class AgentVaultSkillOptions
{
  [JsonIgnore]
  public string? SkillId { get; init; }

  /// <summary>The skill files, at least SKILL.md.</summary>
  [JsonPropertyName("files")]
  public AgentVaultSkillFile[] Files { get; init; } = Array.Empty<AgentVaultSkillFile>();

  /// <summary>When updating: the version the change is based on (optimistic locking).</summary>
  [JsonPropertyName("baseVersion")]
  public int? BaseVersion { get; init; }
}

public class AgentVaultProxyOptions
{
  [JsonIgnore]
  public string? ProxyId { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  /// <summary>Traffic policy as listed in the API reference (default: any host).</summary>
  [JsonPropertyName("trafficPolicy")]
  public string? TrafficPolicy { get; init; }

  [JsonPropertyName("allowedHosts")]
  public string[]? AllowedHosts { get; init; }

  /// <summary>Poll interval in seconds (default 60).</summary>
  [JsonPropertyName("pollInterval")]
  public int? PollInterval { get; init; }
}

public class CreateAgentVaultSessionOptions
{
  /// <summary>Slugs of the access bundles the session may use.</summary>
  [JsonPropertyName("accessBundles")]
  public string[] AccessBundles { get; init; } = Array.Empty<string>();

  /// <summary>Session lifetime such as "30m", "8h", "7d" or "never".</summary>
  [JsonPropertyName("ttl")]
  public string? Ttl { get; init; }

  [JsonPropertyName("label")]
  public string? Label { get; init; }
}

public class ListAgentVaultSessionsOptions
{
  /// <summary>"mine" (default) or the other scopes of the API reference.</summary>
  public string? Scope { get; init; }
  public string[]? Statuses { get; init; }
  public string? Search { get; init; }
  public int? Limit { get; init; }
  public int? Offset { get; init; }
}
