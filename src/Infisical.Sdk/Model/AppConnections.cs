using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Identifiers of the supported app connection types.</summary>
public static class AppConnectionTypes
{
  public const string GitHub = "github";
  public const string GitHubRadar = "github-radar";
  public const string AWS = "aws";
  public const string Databricks = "databricks";
  public const string GCP = "gcp";
  public const string AzureKeyVault = "azure-key-vault";
  public const string AzureAppConfiguration = "azure-app-configuration";
  public const string AzureClientSecrets = "azure-client-secrets";
  public const string AzureDevOps = "azure-devops";
  public const string AzureADCS = "azure-adcs";
  public const string ADCS = "adcs";
  public const string AzureDNS = "azure-dns";
  public const string Humanitec = "humanitec";
  public const string TerraformCloud = "terraform-cloud";
  public const string Vercel = "vercel";
  public const string Postgres = "postgres";
  public const string MsSql = "mssql";
  public const string MySql = "mysql";
  public const string Camunda = "camunda";
  public const string Windmill = "windmill";
  public const string Auth0 = "auth0";
  public const string HCVault = "hashicorp-vault";
  public const string LDAP = "ldap";
  public const string TeamCity = "teamcity";
  public const string OCI = "oci";
  public const string OracleDB = "oracledb";
  public const string OnePass = "1password";
  public const string Heroku = "heroku";
  public const string Render = "render";
  public const string Flyio = "flyio";
  public const string TriggerDev = "trigger-dev";
  public const string GitLab = "gitlab";
  public const string Cloudflare = "cloudflare";
  public const string DNSMadeEasy = "dns-made-easy";
  public const string UltraDNS = "ultradns";
  public const string Zabbix = "zabbix";
  public const string Railway = "railway";
  public const string Bitbucket = "bitbucket";
  public const string Checkly = "checkly";
  public const string Supabase = "supabase";
  public const string DigitalOcean = "digital-ocean";
  public const string Netlify = "netlify";
  public const string Okta = "okta";
  public const string Redis = "redis";
  public const string MongoDB = "mongodb";
  public const string LaravelForge = "laravel-forge";
  public const string Chef = "chef";
  public const string Northflank = "northflank";
  public const string OctopusDeploy = "octopus-deploy";
  public const string SSH = "ssh";
  public const string Dbt = "dbt";
  public const string SMB = "smb";
  public const string WinRM = "winrm";
  public const string OpenRouter = "open-router";
  public const string OpenAI = "openai";
  public const string CircleCI = "circleci";
  public const string AzureEntraId = "azure-entra-id";
  public const string Venafi = "venafi";
  public const string VenafiTpp = "venafi-tpp";
  public const string ExternalInfisical = "external-infisical";
  public const string Doppler = "doppler";
  public const string NetScaler = "netscaler";
  public const string PaloAltoNetworks = "palo-alto-networks";
  public const string Anthropic = "anthropic";
  public const string OVH = "ovh";
  public const string Devin = "devin";
  public const string Ona = "ona";
  public const string DigiCert = "digicert";
  public const string TravisCI = "travis-ci";
  public const string Salesforce = "salesforce";
  public const string Snowflake = "snowflake";
  public const string Datadog = "datadog";
  public const string F5BigIp = "f5-big-ip";
  public const string GoDaddy = "godaddy";
  public const string Convex = "convex";
  public const string Rundeck = "rundeck";
  public const string Portainer = "portainer";
  public const string HasuraCloud = "hasura-cloud";
  public const string Qovery = "qovery";
  public const string Cloud66 = "cloud-66";
  public const string LiteLLM = "litellm";
  public const string Fireworks = "fireworks";
  public const string KempLoadMaster = "kemp-loadmaster";
  public const string MicrosoftIntune = "microsoft-intune";
  public const string NutanixPrismCentral = "nutanix-prism-central";
  public const string PowerDns = "powerdns";
  public const string Spacelift = "spacelift";
  public const string Daytona = "daytona";
  public const string Stripe = "stripe";
  public const string Keeper = "keeper";
  public const string HpeIloRedFish = "hpe-ilo";
  public const string ServiceNow = "servicenow";
  public const string S3Compatible = "s3-compatible";
  public const string Jira = "jira";
}

/// <summary>An app connection as returned by the API (credentials are never returned in clear text).</summary>
public class AppConnectionInfo : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  /// <summary>The app, see <see cref="AppConnectionTypes"/>.</summary>
  [JsonPropertyName("app")]
  public string App { get; set; } = string.Empty;

  /// <summary>The authentication method of the connection, e.g. "access-key" or "assume-role" for AWS.</summary>
  [JsonPropertyName("method")]
  public string? Method { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("version")]
  public int? Version { get; set; }

  [JsonPropertyName("isPlatformManagedCredentials")]
  public bool? IsPlatformManagedCredentials { get; set; }

  [JsonPropertyName("gatewayId")]
  public string? GatewayId { get; set; }

  [JsonPropertyName("isAutoRotationEnabled")]
  public bool? IsAutoRotationEnabled { get; set; }

  /// <summary>Non-secret parts of the credentials (e.g. the access key ID).</summary>
  [JsonPropertyName("credentials")]
  public JsonElement? Credentials { get; set; }
}

/// <summary>An app connection that is available for use in a project (raw reference: id, name, app, ...).</summary>
public class AvailableAppConnection : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;
}

public class CreateAppConnectionOptions
{
  /// <summary>One of <see cref="AppConnectionTypes"/>.</summary>
  [JsonIgnore]
  public string App { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>Authentication method, e.g. "access-key", "assume-role", "personal-access-token". See the API reference of the app.</summary>
  [JsonPropertyName("method")]
  public string Method { get; init; } = string.Empty;

  /// <summary>
  /// Method specific credentials, e.g. <c>{ ["accessKeyId"] = "...", ["secretAccessKey"] = "..." }</c> for AWS
  /// access keys. See the API reference of the app.
  /// </summary>
  [JsonPropertyName("credentials")]
  public Dictionary<string, object?> Credentials { get; init; } = new Dictionary<string, object?>();

  /// <summary>Project to create a project-scoped connection in. Omit for an organization connection.</summary>
  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("isPlatformManagedCredentials")]
  public bool? IsPlatformManagedCredentials { get; init; }

  [JsonPropertyName("gatewayId")]
  public string? GatewayId { get; init; }

  [JsonPropertyName("gatewayPoolId")]
  public string? GatewayPoolId { get; init; }

  [JsonPropertyName("isAutoRotationEnabled")]
  public bool? IsAutoRotationEnabled { get; init; }

  /// <summary>Rotation schedule, required when <see cref="IsAutoRotationEnabled"/> is set.</summary>
  [JsonPropertyName("rotation")]
  public Dictionary<string, object?>? Rotation { get; init; }

  /// <summary>Additional, app specific configuration (only used by some apps).</summary>
  [JsonPropertyName("configuration")]
  public Dictionary<string, object?>? Configuration { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(App)) throw new InfisicalException("App is required");
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
    if (string.IsNullOrEmpty(Method)) throw new InfisicalException("Method is required");
  }
}

public class UpdateAppConnectionOptions
{
  [JsonIgnore]
  public string App { get; init; } = string.Empty;

  [JsonIgnore]
  public string ConnectionId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("credentials")]
  public Dictionary<string, object?>? Credentials { get; init; }

  [JsonPropertyName("isPlatformManagedCredentials")]
  public bool? IsPlatformManagedCredentials { get; init; }

  [JsonPropertyName("gatewayId")]
  public string? GatewayId { get; init; }

  [JsonPropertyName("gatewayPoolId")]
  public string? GatewayPoolId { get; init; }

  [JsonPropertyName("isAutoRotationEnabled")]
  public bool? IsAutoRotationEnabled { get; init; }

  [JsonPropertyName("rotation")]
  public Dictionary<string, object?>? Rotation { get; init; }

  [JsonPropertyName("configuration")]
  public Dictionary<string, object?>? Configuration { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(App)) throw new InfisicalException("App is required");
    if (string.IsNullOrEmpty(ConnectionId)) throw new InfisicalException("ConnectionId is required");
  }
}
