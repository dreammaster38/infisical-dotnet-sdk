using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ProxiedServiceCredentialRoles
{
  public const string HeaderRewrite = "header-rewrite";
  public const string CredentialSubstitution = "credential-substitution";
}

public static class ProxiedServiceHeaderPurposes
{
  public const string Username = "username";
  public const string Password = "password";
}

public static class ProxiedServiceSubstitutionSurfaces
{
  public const string Header = "header";
  public const string Path = "path";
  public const string Query = "query";
  public const string Body = "body";
}

public class ProxiedService : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("hostPattern")]
  public string? HostPattern { get; set; }

  [JsonPropertyName("isEnabled")]
  public bool? IsEnabled { get; set; }

  [JsonPropertyName("canProxy")]
  public bool? CanProxy { get; set; }

  [JsonPropertyName("credentials")]
  public JsonElement? Credentials { get; set; }
}

/// <summary>
/// A credential of a proxied service. The value comes from a secret (<see cref="SecretKey"/>) or from a field of a
/// dynamic secret (<see cref="DynamicSecretName"/> / <see cref="DynamicSecretField"/>).
/// </summary>
public class ProxiedServiceCredential
{
  [JsonPropertyName("secretKey")]
  public string? SecretKey { get; init; }

  [JsonPropertyName("dynamicSecretName")]
  public string? DynamicSecretName { get; init; }

  [JsonPropertyName("dynamicSecretField")]
  public string? DynamicSecretField { get; init; }

  /// <summary>See <see cref="ProxiedServiceCredentialRoles"/>.</summary>
  [JsonPropertyName("role")]
  public string Role { get; init; } = ProxiedServiceCredentialRoles.HeaderRewrite;

  // header-rewrite
  [JsonPropertyName("headerName")]
  public string? HeaderName { get; init; }

  [JsonPropertyName("headerPrefix")]
  public string? HeaderPrefix { get; init; }

  /// <summary>See <see cref="ProxiedServiceHeaderPurposes"/>.</summary>
  [JsonPropertyName("headerPurpose")]
  public string? HeaderPurpose { get; init; }

  // credential-substitution
  [JsonPropertyName("placeholderKey")]
  public string? PlaceholderKey { get; init; }

  [JsonPropertyName("placeholderValue")]
  public string? PlaceholderValue { get; init; }

  /// <summary>See <see cref="ProxiedServiceSubstitutionSurfaces"/>.</summary>
  [JsonPropertyName("substitutionSurfaces")]
  public string[]? SubstitutionSurfaces { get; init; }
}

public class ProxiedServiceLocation
{
  public string ProjectId { get; init; } = string.Empty;
  public string EnvironmentSlug { get; init; } = string.Empty;
  public string SecretPath { get; init; } = "/";

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
  }
}

public class CreateProxiedServiceOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("secretPath")]
  public string SecretPath { get; init; } = "/";

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  /// <summary>Hosts the service applies to, comma separated, e.g. "api.example.com,*.example.org:8443".</summary>
  [JsonPropertyName("hostPattern")]
  public string HostPattern { get; init; } = string.Empty;

  [JsonPropertyName("isEnabled")]
  public bool? IsEnabled { get; init; }

  [JsonPropertyName("credentials")]
  public ProxiedServiceCredential[] Credentials { get; init; } = Array.Empty<ProxiedServiceCredential>();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
    if (string.IsNullOrEmpty(HostPattern)) throw new InfisicalException("HostPattern is required");
  }
}

public class UpdateProxiedServiceOptions
{
  [JsonIgnore]
  public string ServiceId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("hostPattern")]
  public string? HostPattern { get; init; }

  [JsonPropertyName("isEnabled")]
  public bool? IsEnabled { get; init; }

  /// <summary>Replaces all credentials of the service.</summary>
  [JsonPropertyName("credentials")]
  public ProxiedServiceCredential[]? Credentials { get; init; }
}
