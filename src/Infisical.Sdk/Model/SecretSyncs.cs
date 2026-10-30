using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Identifiers of the supported secret sync destinations.</summary>
public static class SecretSyncDestinations
{
  public const string AWSParameterStore = "aws-parameter-store";
  public const string AWSSecretsManager = "aws-secrets-manager";
  public const string GitHub = "github";
  public const string GCPSecretManager = "gcp-secret-manager";
  public const string AzureKeyVault = "azure-key-vault";
  public const string AzureAppConfiguration = "azure-app-configuration";
  public const string AzureDevOps = "azure-devops";
  public const string Databricks = "databricks";
  public const string Humanitec = "humanitec";
  public const string TerraformCloud = "terraform-cloud";
  public const string Camunda = "camunda";
  public const string Vercel = "vercel";
  public const string Windmill = "windmill";
  public const string HCVault = "hashicorp-vault";
  public const string TeamCity = "teamcity";
  public const string OCIVault = "oci-vault";
  public const string OnePass = "1password";
  public const string Heroku = "heroku";
  public const string Render = "render";
  public const string Flyio = "flyio";
  public const string TriggerDev = "trigger-dev";
  public const string GitLab = "gitlab";
  public const string CloudflarePages = "cloudflare-pages";
  public const string CloudflareWorkers = "cloudflare-workers";
  public const string CloudflareSecretsStore = "cloudflare-secrets-store";
  public const string Supabase = "supabase";
  public const string Zabbix = "zabbix";
  public const string Railway = "railway";
  public const string Checkly = "checkly";
  public const string DigitalOceanAppPlatform = "digital-ocean-app-platform";
  public const string Netlify = "netlify";
  public const string Northflank = "northflank";
  public const string Bitbucket = "bitbucket";
  public const string LaravelForge = "laravel-forge";
  public const string Chef = "chef";
  public const string OctopusDeploy = "octopus-deploy";
  public const string CircleCI = "circleci";
  public const string AzureEntraIdScim = "azure-entra-id-scim";
  public const string ExternalInfisical = "external-infisical";
  public const string OVH = "ovh";
  public const string Devin = "devin";
  public const string Ona = "ona";
  public const string TravisCI = "travis-ci";
  public const string Snowflake = "snowflake";
  public const string Rundeck = "rundeck";
  public const string Portainer = "portainer";
  public const string HasuraCloud = "hasura-cloud";
  public const string Qovery = "qovery";
  public const string Cloud66 = "cloud-66";
  public const string Spacelift = "spacelift";
  public const string Daytona = "daytona";
  public const string Keeper = "keeper-password-manager";
}

public static class SecretSyncInitialSyncBehaviors
{
  public const string OverwriteDestination = "overwrite-destination";
  public const string ImportPrioritizeSource = "import-prioritize-source";
  public const string ImportPrioritizeDestination = "import-prioritize-destination";
}

public static class SecretSyncImportBehaviors
{
  public const string PrioritizeSource = "prioritize-source";
  public const string PrioritizeDestination = "prioritize-destination";
}

/// <summary>A secret sync as returned by the API.</summary>
public class SecretSyncInfo : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("destination")]
  public string Destination { get; set; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("isAutoSyncEnabled")]
  public bool? IsAutoSyncEnabled { get; set; }

  [JsonPropertyName("syncStatus")]
  public string? SyncStatus { get; set; }

  [JsonPropertyName("lastSyncMessage")]
  public string? LastSyncMessage { get; set; }

  [JsonPropertyName("importStatus")]
  public string? ImportStatus { get; set; }

  [JsonPropertyName("removeStatus")]
  public string? RemoveStatus { get; set; }

  /// <summary>Destination specific configuration (e.g. region / path for AWS Parameter Store).</summary>
  [JsonPropertyName("destinationConfig")]
  public JsonElement? DestinationConfig { get; set; }

  [JsonPropertyName("syncOptions")]
  public JsonElement? SyncOptions { get; set; }

  [JsonPropertyName("connection")]
  public JsonElement? Connection { get; set; }

  [JsonPropertyName("environment")]
  public JsonElement? Environment { get; set; }

  [JsonPropertyName("folder")]
  public JsonElement? Folder { get; set; }
}

/// <summary>Sync options. Destination specific options (e.g. tags, keyId) can be passed through <see cref="Extra"/>.</summary>
public class SecretSyncOptions
{
  /// <summary>See <see cref="SecretSyncInitialSyncBehaviors"/>.</summary>
  [JsonPropertyName("initialSyncBehavior")]
  public string? InitialSyncBehavior { get; set; }

  /// <summary>Key schema, must contain {{secretKey}}, e.g. "INFISICAL_{{secretKey}}".</summary>
  [JsonPropertyName("keySchema")]
  public string? KeySchema { get; set; }

  [JsonPropertyName("disableSecretDeletion")]
  public bool? DisableSecretDeletion { get; set; }

  [JsonPropertyName("includeAllSubFolders")]
  public bool? IncludeAllSubFolders { get; set; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; set; }
}

public class CreateSecretSyncOptions
{
  /// <summary>One of <see cref="SecretSyncDestinations"/>.</summary>
  [JsonIgnore]
  public string Destination { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>ID of the app connection that is used to access the destination.</summary>
  [JsonPropertyName("connectionId")]
  public string ConnectionId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("secretPath")]
  public string SecretPath { get; init; } = "/";

  [JsonPropertyName("isAutoSyncEnabled")]
  public bool? IsAutoSyncEnabled { get; init; }

  [JsonPropertyName("syncOptions")]
  public SecretSyncOptions SyncOptions { get; init; } = new SecretSyncOptions();

  /// <summary>
  /// Destination specific configuration, e.g. <c>{ ["region"] = "eu-central-1", ["path"] = "/my-app/" }</c> for
  /// AWS Parameter Store. See the API reference of the destination.
  /// </summary>
  [JsonPropertyName("destinationConfig")]
  public Dictionary<string, object?> DestinationConfig { get; init; } = new Dictionary<string, object?>();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(Destination)) throw new InfisicalException("Destination is required");
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(ConnectionId)) throw new InfisicalException("ConnectionId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (string.IsNullOrEmpty(SecretPath)) throw new InfisicalException("SecretPath is required");
  }
}

public class UpdateSecretSyncOptions
{
  [JsonIgnore]
  public string Destination { get; init; } = string.Empty;

  [JsonIgnore]
  public string SyncId { get; init; } = string.Empty;

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

  [JsonPropertyName("isAutoSyncEnabled")]
  public bool? IsAutoSyncEnabled { get; init; }

  [JsonPropertyName("syncOptions")]
  public SecretSyncOptions? SyncOptions { get; init; }

  [JsonPropertyName("destinationConfig")]
  public Dictionary<string, object?>? DestinationConfig { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(Destination)) throw new InfisicalException("Destination is required");
    if (string.IsNullOrEmpty(SyncId)) throw new InfisicalException("SyncId is required");
  }
}

public class CheckSecretSyncDestinationOptions
{
  [JsonIgnore]
  public string Destination { get; init; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("connectionId")]
  public string? ConnectionId { get; init; }

  [JsonPropertyName("destinationConfig")]
  public Dictionary<string, object?> DestinationConfig { get; init; } = new Dictionary<string, object?>();

  [JsonPropertyName("syncOptions")]
  public Dictionary<string, object?>? SyncOptions { get; init; }

  /// <summary>Ignore this sync when checking for conflicts (use when editing an existing sync).</summary>
  [JsonPropertyName("excludeSyncId")]
  public string? ExcludeSyncId { get; init; }
}
