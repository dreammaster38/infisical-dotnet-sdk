using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Gateways
// ---------------------------------------------------------------------------------------------

public class Gateway : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("relayId")]
  public string? RelayId { get; set; }

  [JsonPropertyName("heartbeat")]
  public string? Heartbeat { get; set; }

  [JsonPropertyName("directAddress")]
  public string? DirectAddress { get; set; }

  [JsonPropertyName("canRevoke")]
  public bool? CanRevoke { get; set; }

  /// <summary>The auth method the gateway uses to enroll / log in (method type and its configuration).</summary>
  [JsonPropertyName("authMethod")]
  public JsonElement? AuthMethod { get; set; }
}

/// <summary>
/// Auth method of a gateway. Use the factory methods for the common cases; method specific settings that are not
/// modelled can be passed through <see cref="Extra"/>.
/// </summary>
public class GatewayAuthMethod
{
  /// <summary>"token", "aws", "gcp" or "kubernetes".</summary>
  [JsonPropertyName("method")]
  public string Method { get; init; } = string.Empty;

  // aws
  [JsonPropertyName("allowedPrincipalArns")]
  public string? AllowedPrincipalArns { get; init; }

  [JsonPropertyName("allowedAccountIds")]
  public string? AllowedAccountIds { get; init; }

  // gcp
  /// <summary>"iam" or "gce".</summary>
  [JsonPropertyName("type")]
  public string? Type { get; init; }

  [JsonPropertyName("allowedServiceAccounts")]
  public string? AllowedServiceAccounts { get; init; }

  [JsonPropertyName("allowedProjects")]
  public string? AllowedProjects { get; init; }

  [JsonPropertyName("allowedZones")]
  public string? AllowedZones { get; init; }

  // kubernetes
  [JsonPropertyName("kubernetesHost")]
  public string? KubernetesHost { get; init; }

  [JsonPropertyName("caCert")]
  public string? CaCert { get; init; }

  [JsonPropertyName("tokenReviewerJwt")]
  public string? TokenReviewerJwt { get; init; }

  /// <summary>"api" or "gateway".</summary>
  [JsonPropertyName("tokenReviewMode")]
  public string? TokenReviewMode { get; init; }

  [JsonPropertyName("allowedNamespaces")]
  public string? AllowedNamespaces { get; init; }

  [JsonPropertyName("allowedNames")]
  public string? AllowedNames { get; init; }

  [JsonPropertyName("allowedAudience")]
  public string? AllowedAudience { get; init; }

  [JsonPropertyName("gatewayId")]
  public string? GatewayId { get; init; }

  [JsonPropertyName("gatewayPoolId")]
  public string? GatewayPoolId { get; init; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; init; }

  /// <summary>The gateway enrolls with a one-time enrollment token.</summary>
  public static GatewayAuthMethod Token() => new GatewayAuthMethod { Method = "token" };

  public static GatewayAuthMethod Aws(string? allowedPrincipalArns = null, string? allowedAccountIds = null)
    => new GatewayAuthMethod { Method = "aws", AllowedPrincipalArns = allowedPrincipalArns ?? string.Empty, AllowedAccountIds = allowedAccountIds ?? string.Empty };
}

public class CreateGatewayOptions
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("authMethod")]
  public GatewayAuthMethod AuthMethod { get; init; } = GatewayAuthMethod.Token();
}

public class UpdateGatewayOptions
{
  [JsonIgnore]
  public string GatewayId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("authMethod")]
  public GatewayAuthMethod? AuthMethod { get; init; }
}

public class GatewayEnrollmentToken
{
  public GatewayEnrollmentToken(string token, string? expiresAt)
  {
    Token = token;
    ExpiresAt = expiresAt;
  }

  /// <summary>The enrollment token, only returned once.</summary>
  public string Token { get; }

  public string? ExpiresAt { get; }
}

// ---------------------------------------------------------------------------------------------
// Project templates
// ---------------------------------------------------------------------------------------------

public class ProjectTemplateEnvironment
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("slug")]
  public string Slug { get; init; } = string.Empty;

  [JsonPropertyName("position")]
  public int Position { get; init; }
}

public class ProjectTemplateRole
{
  [JsonPropertyName("slug")]
  public string Slug { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  /// <summary>Permission rules, e.g. <c>new { action = new[] { "read" }, subject = "secrets" }</c>.</summary>
  [JsonPropertyName("permissions")]
  public object[] Permissions { get; init; } = Array.Empty<object>();
}

public class ProjectTemplateUser
{
  [JsonPropertyName("username")]
  public string Username { get; init; } = string.Empty;

  [JsonPropertyName("roles")]
  public string[] Roles { get; init; } = Array.Empty<string>();
}

public class ProjectTemplateGroup
{
  [JsonPropertyName("groupSlug")]
  public string GroupSlug { get; init; } = string.Empty;

  [JsonPropertyName("roles")]
  public string[] Roles { get; init; } = Array.Empty<string>();
}

public class ProjectTemplateIdentity
{
  [JsonPropertyName("identityId")]
  public string IdentityId { get; init; } = string.Empty;

  [JsonPropertyName("roles")]
  public string[] Roles { get; init; } = Array.Empty<string>();
}

public class ProjectTemplateProjectManagedIdentity
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("roles")]
  public string[] Roles { get; init; } = Array.Empty<string>();
}

public class ProjectTemplate : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("type")]
  public string? Type { get; set; }

  [JsonPropertyName("environments")]
  public JsonElement? Environments { get; set; }

  [JsonPropertyName("roles")]
  public JsonElement? Roles { get; set; }

  [JsonPropertyName("users")]
  public JsonElement? Users { get; set; }

  [JsonPropertyName("groups")]
  public JsonElement? Groups { get; set; }

  [JsonPropertyName("identities")]
  public JsonElement? Identities { get; set; }

  [JsonPropertyName("projectManagedIdentities")]
  public JsonElement? ProjectManagedIdentities { get; set; }
}

public class ProjectTemplateOptions
{
  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? TemplateId { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>Project type, e.g. "secret-manager". Required when creating.</summary>
  [JsonPropertyName("type")]
  public string? Type { get; init; }

  [JsonPropertyName("roles")]
  public ProjectTemplateRole[]? Roles { get; init; }

  [JsonPropertyName("environments")]
  public ProjectTemplateEnvironment[]? Environments { get; init; }

  [JsonPropertyName("users")]
  public ProjectTemplateUser[]? Users { get; init; }

  [JsonPropertyName("groups")]
  public ProjectTemplateGroup[]? Groups { get; init; }

  [JsonPropertyName("identities")]
  public ProjectTemplateIdentity[]? Identities { get; init; }

  [JsonPropertyName("projectManagedIdentities")]
  public ProjectTemplateProjectManagedIdentity[]? ProjectManagedIdentities { get; init; }
}
