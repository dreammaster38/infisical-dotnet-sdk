using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Machine identities
// ---------------------------------------------------------------------------------------------

public class Identity : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("orgId")]
  public string? OrgId { get; set; }

  [JsonPropertyName("authMethods")]
  public string[]? AuthMethods { get; set; }

  [JsonPropertyName("hasDeleteProtection")]
  public bool? HasDeleteProtection { get; set; }
}

public class IdentityMetadataEntry
{
  [JsonPropertyName("key")]
  public string Key { get; set; } = string.Empty;

  [JsonPropertyName("value")]
  public string Value { get; set; } = string.Empty;
}

public class CreateIdentityOptions
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("organizationId")]
  public string OrganizationId { get; init; } = string.Empty;

  /// <summary>Organization role slug, e.g. "no-access", "member" or "admin".</summary>
  [JsonPropertyName("role")]
  public string? Role { get; init; }

  [JsonPropertyName("hasDeleteProtection")]
  public bool? HasDeleteProtection { get; init; }

  [JsonPropertyName("metadata")]
  public IdentityMetadataEntry[]? Metadata { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
    if (string.IsNullOrEmpty(OrganizationId)) throw new InfisicalException("OrganizationId is required");
  }
}

public class UpdateIdentityOptions
{
  [JsonIgnore]
  public string IdentityId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("role")]
  public string? Role { get; init; }

  [JsonPropertyName("hasDeleteProtection")]
  public bool? HasDeleteProtection { get; init; }

  [JsonPropertyName("metadata")]
  public IdentityMetadataEntry[]? Metadata { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(IdentityId)) throw new InfisicalException("IdentityId is required");
  }
}

public class SearchIdentitiesOptions
{
  /// <summary>"name" | "role"</summary>
  [JsonPropertyName("orderBy")]
  public string? OrderBy { get; init; }

  /// <summary>"asc" | "desc"</summary>
  [JsonPropertyName("orderDirection")]
  public string? OrderDirection { get; init; }

  [JsonPropertyName("limit")]
  public int? Limit { get; init; }

  [JsonPropertyName("offset")]
  public int? Offset { get; init; }

  /// <summary>Free form search filter, see the "Search Machine Identities" API reference.</summary>
  [JsonPropertyName("search")]
  public Dictionary<string, object?>? Search { get; init; }
}

public class IdentitySearchResult
{
  public IdentitySearchResult(JsonElement[] identities, int? totalCount)
  {
    Identities = identities;
    TotalCount = totalCount;
  }

  /// <summary>Raw identity memberships (they contain the identity as well as role information).</summary>
  public JsonElement[] Identities { get; }

  public int? TotalCount { get; }
}

// ---------------------------------------------------------------------------------------------
// Identity auth configuration (attach / update)
// ---------------------------------------------------------------------------------------------

/// <summary>
/// Configuration shared by all identity auth methods. Properties that are not modelled can be passed through
/// <see cref="Extra"/>.
/// </summary>
public class IdentityAuthConfig
{
  [JsonPropertyName("accessTokenTrustedIps")]
  public IpEntry[]? AccessTokenTrustedIps { get; set; }

  [JsonPropertyName("accessTokenTTL")]
  public int? AccessTokenTTL { get; set; }

  [JsonPropertyName("accessTokenMaxTTL")]
  public int? AccessTokenMaxTTL { get; set; }

  [JsonPropertyName("accessTokenNumUsesLimit")]
  public int? AccessTokenNumUsesLimit { get; set; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; set; }
}

public class UniversalAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("clientSecretTrustedIps")]
  public IpEntry[]? ClientSecretTrustedIps { get; set; }

  [JsonPropertyName("accessTokenPeriod")]
  public int? AccessTokenPeriod { get; set; }

  [JsonPropertyName("lockoutEnabled")]
  public bool? LockoutEnabled { get; set; }

  [JsonPropertyName("lockoutThreshold")]
  public int? LockoutThreshold { get; set; }

  [JsonPropertyName("lockoutDurationSeconds")]
  public int? LockoutDurationSeconds { get; set; }

  [JsonPropertyName("lockoutCounterResetSeconds")]
  public int? LockoutCounterResetSeconds { get; set; }
}

public class TokenAuthConfig : IdentityAuthConfig { }

public class AwsAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("stsEndpoint")]
  public string? StsEndpoint { get; set; }

  /// <summary>Comma separated list of allowed principal ARNs.</summary>
  [JsonPropertyName("allowedPrincipalArns")]
  public string? AllowedPrincipalArns { get; set; }

  /// <summary>Comma separated list of allowed AWS account IDs.</summary>
  [JsonPropertyName("allowedAccountIds")]
  public string? AllowedAccountIds { get; set; }
}

public class AzureAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("tenantId")]
  public string? TenantId { get; set; }

  [JsonPropertyName("resource")]
  public string? Resource { get; set; }

  [JsonPropertyName("allowedServicePrincipalIds")]
  public string? AllowedServicePrincipalIds { get; set; }
}

public class GcpAuthConfig : IdentityAuthConfig
{
  /// <summary>"iam" or "gce"</summary>
  [JsonPropertyName("type")]
  public string? Type { get; set; }

  [JsonPropertyName("allowedServiceAccounts")]
  public string? AllowedServiceAccounts { get; set; }

  [JsonPropertyName("allowedProjects")]
  public string? AllowedProjects { get; set; }

  [JsonPropertyName("allowedZones")]
  public string? AllowedZones { get; set; }
}

public class KubernetesAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("kubernetesHost")]
  public string? KubernetesHost { get; set; }

  [JsonPropertyName("caCert")]
  public string? CaCert { get; set; }

  [JsonPropertyName("tokenReviewerJwt")]
  public string? TokenReviewerJwt { get; set; }

  [JsonPropertyName("allowedNamespaces")]
  public string? AllowedNamespaces { get; set; }

  [JsonPropertyName("allowedNames")]
  public string? AllowedNames { get; set; }

  [JsonPropertyName("allowedAudience")]
  public string? AllowedAudience { get; set; }

  [JsonPropertyName("verifyTlsCertificate")]
  public bool? VerifyTlsCertificate { get; set; }
}

public class OidcAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("oidcDiscoveryUrl")]
  public string? OidcDiscoveryUrl { get; set; }

  [JsonPropertyName("caCert")]
  public string? CaCert { get; set; }

  [JsonPropertyName("boundIssuer")]
  public string? BoundIssuer { get; set; }

  [JsonPropertyName("boundAudiences")]
  public string? BoundAudiences { get; set; }

  [JsonPropertyName("boundClaims")]
  public Dictionary<string, string>? BoundClaims { get; set; }

  [JsonPropertyName("boundSubject")]
  public string? BoundSubject { get; set; }
}

public class JwtAuthConfig : IdentityAuthConfig
{
  /// <summary>"jwks" or "static"</summary>
  [JsonPropertyName("configurationType")]
  public string? ConfigurationType { get; set; }

  [JsonPropertyName("jwksUrl")]
  public string? JwksUrl { get; set; }

  [JsonPropertyName("jwksCaCert")]
  public string? JwksCaCert { get; set; }

  [JsonPropertyName("publicKeys")]
  public string[]? PublicKeys { get; set; }

  [JsonPropertyName("boundIssuer")]
  public string? BoundIssuer { get; set; }

  [JsonPropertyName("boundAudiences")]
  public string? BoundAudiences { get; set; }

  [JsonPropertyName("boundClaims")]
  public Dictionary<string, string>? BoundClaims { get; set; }

  [JsonPropertyName("boundSubject")]
  public string? BoundSubject { get; set; }
}

public class OciAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("tenancyOcid")]
  public string? TenancyOcid { get; set; }

  [JsonPropertyName("allowedUsernames")]
  public string? AllowedUsernames { get; set; }
}

public class AlicloudAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("allowedArns")]
  public string? AllowedArns { get; set; }
}

public class TlsCertAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("caCertificate")]
  public string? CaCertificate { get; set; }

  [JsonPropertyName("allowedCommonNames")]
  public string? AllowedCommonNames { get; set; }

  [JsonPropertyName("allowedSubjectAltNames")]
  public string? AllowedSubjectAltNames { get; set; }

  [JsonPropertyName("verifyClientCertificateChain")]
  public bool? VerifyClientCertificateChain { get; set; }
}

public class LdapAuthConfig : IdentityAuthConfig
{
  [JsonPropertyName("url")]
  public string? Url { get; set; }

  [JsonPropertyName("bindDN")]
  public string? BindDN { get; set; }

  [JsonPropertyName("bindPass")]
  public string? BindPass { get; set; }

  [JsonPropertyName("searchBase")]
  public string? SearchBase { get; set; }

  [JsonPropertyName("searchFilter")]
  public string? SearchFilter { get; set; }

  [JsonPropertyName("ldapCaCertificate")]
  public string? LdapCaCertificate { get; set; }
}

/// <summary>An auth configuration of an identity as returned by the API.</summary>
public class IdentityAuthConfigInfo : ApiModel
{
  [JsonPropertyName("id")]
  public string? Id { get; set; }

  [JsonPropertyName("identityId")]
  public string? IdentityId { get; set; }

  [JsonPropertyName("accessTokenTTL")]
  public int? AccessTokenTTL { get; set; }

  [JsonPropertyName("accessTokenMaxTTL")]
  public int? AccessTokenMaxTTL { get; set; }

  [JsonPropertyName("accessTokenNumUsesLimit")]
  public int? AccessTokenNumUsesLimit { get; set; }

  [JsonPropertyName("accessTokenTrustedIps")]
  public IpEntry[]? AccessTokenTrustedIps { get; set; }
}

// ---------------------------------------------------------------------------------------------
// Universal auth client secrets / token auth tokens
// ---------------------------------------------------------------------------------------------

public class ClientSecretData : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("clientSecretPrefix")]
  public string? ClientSecretPrefix { get; set; }

  [JsonPropertyName("clientSecretNumUses")]
  public int? NumUses { get; set; }

  [JsonPropertyName("clientSecretNumUsesLimit")]
  public int? NumUsesLimit { get; set; }

  [JsonPropertyName("clientSecretTTL")]
  public int? Ttl { get; set; }

  [JsonPropertyName("isClientSecretRevoked")]
  public bool? IsRevoked { get; set; }
}

public class CreateClientSecretOptions
{
  [JsonIgnore]
  public string IdentityId { get; init; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("numUsesLimit")]
  public int? NumUsesLimit { get; init; }

  /// <summary>TTL in seconds, 0 means the secret never expires.</summary>
  [JsonPropertyName("ttl")]
  public int? Ttl { get; init; }
}

public class CreatedClientSecret
{
  public CreatedClientSecret(string clientSecret, ClientSecretData data)
  {
    ClientSecret = clientSecret;
    Data = data;
  }

  /// <summary>The plain client secret. It is only returned once.</summary>
  public string ClientSecret { get; }

  public ClientSecretData Data { get; }
}

public class IdentityAccessToken : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("identityId")]
  public string? IdentityId { get; set; }

  [JsonPropertyName("accessTokenTTL")]
  public int? AccessTokenTTL { get; set; }

  [JsonPropertyName("accessTokenMaxTTL")]
  public int? AccessTokenMaxTTL { get; set; }

  [JsonPropertyName("isAccessTokenRevoked")]
  public bool? IsRevoked { get; set; }
}

// ---------------------------------------------------------------------------------------------
// Login requests of the additional auth methods
// ---------------------------------------------------------------------------------------------

public class AwsAuthLoginRequest
{
  [JsonPropertyName("identityId")]
  public string IdentityId { get; init; } = string.Empty;

  [JsonPropertyName("iamHttpRequestMethod")]
  public string IamHttpRequestMethod { get; init; } = "POST";

  /// <summary>Body of the signed sts:GetCallerIdentity request.</summary>
  [JsonPropertyName("iamRequestBody")]
  public string IamRequestBody { get; init; } = string.Empty;

  /// <summary>JSON-encoded headers of the signed sts:GetCallerIdentity request.</summary>
  [JsonPropertyName("iamRequestHeaders")]
  public string IamRequestHeaders { get; init; } = string.Empty;

  [JsonPropertyName("organizationSlug")]
  public string? OrganizationSlug { get; init; }
}

public class JwtLoginRequest
{
  [JsonPropertyName("identityId")]
  public string IdentityId { get; init; } = string.Empty;

  [JsonPropertyName("jwt")]
  public string Jwt { get; init; } = string.Empty;

  [JsonPropertyName("organizationSlug")]
  public string? OrganizationSlug { get; init; }
}

public class OciAuthLoginRequest
{
  [JsonPropertyName("identityId")]
  public string IdentityId { get; init; } = string.Empty;

  [JsonPropertyName("userOcid")]
  public string UserOcid { get; init; } = string.Empty;

  /// <summary>Signed request headers; must contain "authorization", "host" and "date" or "x-date".</summary>
  [JsonPropertyName("headers")]
  public Dictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

  [JsonPropertyName("organizationSlug")]
  public string? OrganizationSlug { get; init; }
}

public class AlicloudAuthLoginRequest
{
  [JsonPropertyName("identityId")]
  public string IdentityId { get; init; } = string.Empty;

  [JsonPropertyName("Action")]
  public string Action { get; init; } = "GetCallerIdentity";

  [JsonPropertyName("Format")]
  public string Format { get; init; } = "JSON";

  [JsonPropertyName("Version")]
  public string Version { get; init; } = "2015-04-01";

  [JsonPropertyName("AccessKeyId")]
  public string AccessKeyId { get; init; } = string.Empty;

  [JsonPropertyName("SignatureMethod")]
  public string SignatureMethod { get; init; } = "HMAC-SHA1";

  [JsonPropertyName("Timestamp")]
  public string Timestamp { get; init; } = string.Empty;

  [JsonPropertyName("SignatureVersion")]
  public string SignatureVersion { get; init; } = "1.0";

  [JsonPropertyName("SignatureNonce")]
  public string SignatureNonce { get; init; } = string.Empty;

  [JsonPropertyName("Signature")]
  public string Signature { get; init; } = string.Empty;

  [JsonPropertyName("organizationSlug")]
  public string? OrganizationSlug { get; init; }
}
