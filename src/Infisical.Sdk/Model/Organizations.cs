using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Organizations, sub-organizations
// ---------------------------------------------------------------------------------------------

public class Organization : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string? Slug { get; set; }

  [JsonPropertyName("authEnforced")]
  public bool? AuthEnforced { get; set; }

  [JsonPropertyName("scimEnabled")]
  public bool? ScimEnabled { get; set; }

  [JsonPropertyName("enforceMfa")]
  public bool? EnforceMfa { get; set; }

  [JsonPropertyName("defaultMembershipRole")]
  public string? DefaultMembershipRole { get; set; }
}

public class UpdateOrganizationOptions
{
  [JsonIgnore]
  public string OrganizationId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("authEnforced")]
  public bool? AuthEnforced { get; init; }

  [JsonPropertyName("googleSsoAuthEnforced")]
  public bool? GoogleSsoAuthEnforced { get; init; }

  [JsonPropertyName("scimEnabled")]
  public bool? ScimEnabled { get; init; }

  [JsonPropertyName("defaultMembershipRoleSlug")]
  public string? DefaultMembershipRoleSlug { get; init; }

  [JsonPropertyName("enforceMfa")]
  public bool? EnforceMfa { get; init; }

  [JsonPropertyName("selectedMfaMethod")]
  public string? SelectedMfaMethod { get; init; }

  [JsonPropertyName("allowSecretSharingOutsideOrganization")]
  public bool? AllowSecretSharingOutsideOrganization { get; init; }

  [JsonPropertyName("bypassOrgAuthEnabled")]
  public bool? BypassOrgAuthEnabled { get; init; }

  /// <summary>Duration like "1d", "12h", "2w".</summary>
  [JsonPropertyName("userTokenExpiration")]
  public string? UserTokenExpiration { get; init; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; init; }
}

public class OrganizationMembership : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("role")]
  public string? Role { get; set; }

  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("isActive")]
  public bool? IsActive { get; set; }

  [JsonPropertyName("user")]
  public JsonElement? User { get; set; }
}

public class UpdateOrganizationMembershipOptions
{
  [JsonIgnore]
  public string OrganizationId { get; init; } = string.Empty;

  [JsonIgnore]
  public string MembershipId { get; init; } = string.Empty;

  /// <summary>Organization role slug.</summary>
  [JsonPropertyName("role")]
  public string? Role { get; init; }

  [JsonPropertyName("isActive")]
  public bool? IsActive { get; init; }

  [JsonPropertyName("metadata")]
  public IdentityMetadataEntry[]? Metadata { get; init; }
}

public class ListOrganizationIdentitiesOptions
{
  public string OrganizationId { get; init; } = string.Empty;
  public int? Offset { get; init; }
  public int? Limit { get; init; }
  public string? OrderBy { get; init; }
  public string? OrderDirection { get; init; }
  public string? Search { get; init; }
}

public class OrganizationRole : ApiModel
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

public class OrganizationRoleOptions
{
  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? RoleId { get; init; }

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>Permission rules, e.g. <c>new { subject = "workspace", action = new[] { "create" } }</c>.</summary>
  [JsonPropertyName("permissions")]
  public object[]? Permissions { get; init; }
}

public class SubOrganization : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string? Slug { get; set; }
}

public class ListSubOrganizationsOptions
{
  public int? Limit { get; init; }
  public int? Offset { get; init; }
  public string? Search { get; init; }
  public string? OrderBy { get; init; }
  public string? OrderDirection { get; init; }
  public bool? IsAccessible { get; init; }
}

// ---------------------------------------------------------------------------------------------
// Single sign-on
// ---------------------------------------------------------------------------------------------

public class LdapConfigOptions
{
  [JsonPropertyName("organizationId")]
  public string OrganizationId { get; init; } = string.Empty;

  [JsonPropertyName("isActive")]
  public bool? IsActive { get; init; }

  [JsonPropertyName("url")]
  public string? Url { get; init; }

  [JsonPropertyName("bindDN")]
  public string? BindDN { get; init; }

  [JsonPropertyName("bindPass")]
  public string? BindPass { get; init; }

  [JsonPropertyName("uniqueUserAttribute")]
  public string? UniqueUserAttribute { get; init; }

  [JsonPropertyName("searchBase")]
  public string? SearchBase { get; init; }

  [JsonPropertyName("searchFilter")]
  public string? SearchFilter { get; init; }

  [JsonPropertyName("groupSearchBase")]
  public string? GroupSearchBase { get; init; }

  [JsonPropertyName("groupSearchFilter")]
  public string? GroupSearchFilter { get; init; }

  [JsonPropertyName("caCert")]
  public string? CaCert { get; init; }

  [JsonPropertyName("clientCertificate")]
  public string? ClientCertificate { get; init; }

  [JsonPropertyName("clientKeyCertificate")]
  public string? ClientKeyCertificate { get; init; }
}

public class OidcConfigOptions
{
  [JsonPropertyName("organizationId")]
  public string OrganizationId { get; init; } = string.Empty;

  /// <summary>"discoveryURL" or "custom".</summary>
  [JsonPropertyName("configurationType")]
  public string? ConfigurationType { get; init; }

  [JsonPropertyName("discoveryURL")]
  public string? DiscoveryUrl { get; init; }

  [JsonPropertyName("issuer")]
  public string? Issuer { get; init; }

  [JsonPropertyName("authorizationEndpoint")]
  public string? AuthorizationEndpoint { get; init; }

  [JsonPropertyName("jwksUri")]
  public string? JwksUri { get; init; }

  [JsonPropertyName("tokenEndpoint")]
  public string? TokenEndpoint { get; init; }

  [JsonPropertyName("userinfoEndpoint")]
  public string? UserinfoEndpoint { get; init; }

  [JsonPropertyName("clientId")]
  public string? ClientId { get; init; }

  [JsonPropertyName("clientSecret")]
  public string? ClientSecret { get; init; }

  /// <summary>Comma separated list.</summary>
  [JsonPropertyName("allowedEmailDomains")]
  public string? AllowedEmailDomains { get; init; }

  [JsonPropertyName("isActive")]
  public bool? IsActive { get; init; }

  [JsonPropertyName("manageGroupMemberships")]
  public bool? ManageGroupMemberships { get; init; }

  [JsonPropertyName("jwtSignatureAlgorithm")]
  public string? JwtSignatureAlgorithm { get; init; }
}

public class SamlConfigOptions
{
  [JsonPropertyName("organizationId")]
  public string OrganizationId { get; init; } = string.Empty;

  /// <summary>E.g. "okta-saml", "azure-saml", "jumpcloud-saml", "google-saml", "keycloak-saml".</summary>
  [JsonPropertyName("authProvider")]
  public string? AuthProvider { get; init; }

  [JsonPropertyName("isActive")]
  public bool? IsActive { get; init; }

  [JsonPropertyName("entryPoint")]
  public string? EntryPoint { get; init; }

  [JsonPropertyName("issuer")]
  public string? Issuer { get; init; }

  [JsonPropertyName("cert")]
  public string? Cert { get; init; }

  [JsonPropertyName("enableGroupSync")]
  public bool? EnableGroupSync { get; init; }
}

public class AuditLogsQuery
{
  public string? ProjectId { get; init; }
  public string? Environment { get; init; }
  /// <summary>"user", "service", "identity", "kmip-client", ...</summary>
  public string? ActorType { get; init; }
  public string? Actor { get; init; }
  public string? SecretPath { get; init; }
  public string? SecretKey { get; init; }
  /// <summary>Event types, e.g. "create-secret", "get-secrets".</summary>
  public string[]? EventTypes { get; init; }
  public string? UserAgentType { get; init; }
  /// <summary>ISO-8601 timestamp.</summary>
  public string? StartDate { get; init; }
  /// <summary>ISO-8601 timestamp, at most 3 months after <see cref="StartDate"/>.</summary>
  public string? EndDate { get; init; }
  public int? Offset { get; init; }
  public int? Limit { get; init; }
}
