using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Identifiers of the supported secret rotation types.</summary>
public static class SecretRotationTypes
{
  public const string PostgresCredentials = "postgres-credentials";
  public const string MsSqlCredentials = "mssql-credentials";
  public const string MySqlCredentials = "mysql-credentials";
  public const string OracleDBCredentials = "oracledb-credentials";
  public const string Auth0ClientSecret = "auth0-client-secret";
  public const string AzureClientSecret = "azure-client-secret";
  public const string AwsIamUserSecret = "aws-iam-user-secret";
  public const string LdapPassword = "ldap-password";
  public const string OktaClientSecret = "okta-client-secret";
  public const string RedisCredentials = "redis-credentials";
  public const string MongoDBCredentials = "mongodb-credentials";
  public const string DatabricksServicePrincipalSecret = "databricks-service-principal-secret";
  public const string UnixLinuxLocalAccount = "unix-linux-local-account";
  public const string DbtServiceToken = "dbt-service-token";
  public const string WindowsLocalAccount = "windows-local-account";
  public const string OpenRouterApiKey = "open-router-api-key";
  public const string LiteLLMApiKey = "litellm-api-key";
  public const string OpenAIServiceAccount = "openai-service-account";
  public const string HpIloLocalAccount = "hp-ilo-local-account";
  public const string SupabaseApiKey = "supabase-api-key";
  public const string SalesforceOauthCredentials = "salesforce-oauth-credentials";
  public const string DatadogApplicationKeySecret = "datadog-application-key-secret";
  public const string DatadogApiKey = "datadog-api-key";
  public const string ConvexAccessKey = "convex-access-key";
  public const string FireworksApiKey = "fireworks-api-key";
  public const string SnowflakeUserKeyPair = "snowflake-user-key-pair";
  public const string CloudflareApiToken = "cloudflare-api-token";
  public const string CloudflareR2AccessKey = "cloudflare-r2-access-key";
  public const string StripeApiKey = "stripe-api-key";
  public const string GcpServiceAccountKey = "gcp-service-account-key";
}

public class RotateAtUtc
{
  [JsonPropertyName("hours")]
  public int Hours { get; init; }

  [JsonPropertyName("minutes")]
  public int Minutes { get; init; }
}

/// <summary>A secret rotation as returned by the API.</summary>
public class SecretRotationInfo : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  /// <summary>The rotation type, see <see cref="SecretRotationTypes"/>.</summary>
  [JsonPropertyName("type")]
  public string Type { get; set; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("isAutoRotationEnabled")]
  public bool? IsAutoRotationEnabled { get; set; }

  /// <summary>Rotation interval in days.</summary>
  [JsonPropertyName("rotationInterval")]
  public int? RotationInterval { get; set; }

  [JsonPropertyName("rotateAtUtc")]
  public JsonElement? RotateAtUtc { get; set; }

  [JsonPropertyName("rotationStatus")]
  public string? RotationStatus { get; set; }

  [JsonPropertyName("lastRotationMessage")]
  public string? LastRotationMessage { get; set; }

  [JsonPropertyName("lastRotatedAt")]
  public string? LastRotatedAt { get; set; }

  [JsonPropertyName("nextRotationAt")]
  public string? NextRotationAt { get; set; }

  [JsonPropertyName("activeIndex")]
  public int? ActiveIndex { get; set; }

  [JsonPropertyName("parameters")]
  public JsonElement? Parameters { get; set; }

  [JsonPropertyName("secretsMapping")]
  public JsonElement? SecretsMapping { get; set; }

  [JsonPropertyName("connection")]
  public JsonElement? Connection { get; set; }

  [JsonPropertyName("environment")]
  public JsonElement? Environment { get; set; }

  [JsonPropertyName("folder")]
  public JsonElement? Folder { get; set; }
}

public class CreateSecretRotationOptions
{
  /// <summary>One of <see cref="SecretRotationTypes"/>.</summary>
  [JsonIgnore]
  public string Type { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>ID of the app connection that is used to rotate the credentials.</summary>
  [JsonPropertyName("connectionId")]
  public string ConnectionId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  /// <summary>Folder the rotated secrets are written to.</summary>
  [JsonPropertyName("secretPath")]
  public string SecretPath { get; init; } = "/";

  [JsonPropertyName("isAutoRotationEnabled")]
  public bool? IsAutoRotationEnabled { get; init; }

  /// <summary>Rotation interval in days.</summary>
  [JsonPropertyName("rotationInterval")]
  public int? RotationInterval { get; init; }

  [JsonPropertyName("rotateAtUtc")]
  public RotateAtUtc? RotateAtUtc { get; init; }

  /// <summary>
  /// Type specific parameters, e.g. <c>{ ["username1"] = "app_a", ["username2"] = "app_b" }</c> for SQL credentials.
  /// See the API reference of the rotation type.
  /// </summary>
  [JsonPropertyName("parameters")]
  public Dictionary<string, object?> Parameters { get; init; } = new Dictionary<string, object?>();

  /// <summary>Maps the generated credentials to secret names, e.g. <c>{ ["username"] = "DB_USER", ["password"] = "DB_PASS" }</c>.</summary>
  [JsonPropertyName("secretsMapping")]
  public Dictionary<string, object?> SecretsMapping { get; init; } = new Dictionary<string, object?>();

  /// <summary>Parameters that are only needed to set up the rotation and are not stored (only some types).</summary>
  [JsonPropertyName("temporaryParameters")]
  public Dictionary<string, object?>? TemporaryParameters { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(Type)) throw new InfisicalException("Type is required");
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(ConnectionId)) throw new InfisicalException("ConnectionId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (string.IsNullOrEmpty(SecretPath)) throw new InfisicalException("SecretPath is required");
  }
}

public class UpdateSecretRotationOptions
{
  [JsonIgnore]
  public string Type { get; init; } = string.Empty;

  [JsonIgnore]
  public string RotationId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("connectionId")]
  public string? ConnectionId { get; init; }

  [JsonPropertyName("environment")]
  public string? EnvironmentSlug { get; init; }

  [JsonPropertyName("secretPath")]
  public string? SecretPath { get; init; }

  [JsonPropertyName("isAutoRotationEnabled")]
  public bool? IsAutoRotationEnabled { get; init; }

  [JsonPropertyName("rotationInterval")]
  public int? RotationInterval { get; init; }

  [JsonPropertyName("rotateAtUtc")]
  public RotateAtUtc? RotateAtUtc { get; init; }

  [JsonPropertyName("parameters")]
  public Dictionary<string, object?>? Parameters { get; init; }

  [JsonPropertyName("secretsMapping")]
  public Dictionary<string, object?>? SecretsMapping { get; init; }

  [JsonPropertyName("temporaryParameters")]
  public Dictionary<string, object?>? TemporaryParameters { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(Type)) throw new InfisicalException("Type is required");
    if (string.IsNullOrEmpty(RotationId)) throw new InfisicalException("RotationId is required");
  }
}

public class DeleteSecretRotationOptions
{
  public string Type { get; init; } = string.Empty;
  public string RotationId { get; init; } = string.Empty;

  /// <summary>Also delete the secrets the rotation created.</summary>
  public bool DeleteSecrets { get; init; } = false;

  /// <summary>Revoke the credentials the rotation generated at the target system.</summary>
  public bool RevokeGeneratedCredentials { get; init; } = false;
}

public class SecretRotationGeneratedCredentials
{
  public SecretRotationGeneratedCredentials(JsonElement[] generatedCredentials, int? activeIndex, string? rotationId)
  {
    GeneratedCredentials = generatedCredentials;
    ActiveIndex = activeIndex;
    RotationId = rotationId;
  }

  /// <summary>The credential sets (type specific, usually two for zero-downtime rotation).</summary>
  public JsonElement[] GeneratedCredentials { get; }

  /// <summary>Index of the credential set that is currently active.</summary>
  public int? ActiveIndex { get; }

  public string? RotationId { get; }
}
