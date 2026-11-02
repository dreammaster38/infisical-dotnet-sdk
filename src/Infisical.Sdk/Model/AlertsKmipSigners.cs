using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Alerts
// ---------------------------------------------------------------------------------------------

public class Alert : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("resourceType")]
  public string ResourceType { get; set; } = string.Empty;

  [JsonPropertyName("resourceId")]
  public string? ResourceId { get; set; }

  [JsonPropertyName("eventType")]
  public string EventType { get; set; } = string.Empty;

  [JsonPropertyName("triggerType")]
  public string? TriggerType { get; set; }

  [JsonPropertyName("condition")]
  public JsonElement? Condition { get; set; }

  [JsonPropertyName("enabled")]
  public bool? Enabled { get; set; }

  [JsonPropertyName("orgId")]
  public string? OrgId { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("channels")]
  public JsonElement? Channels { get; set; }
}

public class AlertRecipient
{
  /// <summary>"user", "group" or "project" (see the API reference).</summary>
  [JsonPropertyName("principalType")]
  public string PrincipalType { get; init; } = string.Empty;

  [JsonPropertyName("principalId")]
  public string PrincipalId { get; init; } = string.Empty;
}

public class AlertChannel
{
  /// <summary>Only used when updating an existing channel.</summary>
  [JsonPropertyName("id")]
  public string? Id { get; init; }

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  /// <summary>E.g. "email", "slack", "webhook". See the API reference for the available channel types.</summary>
  [JsonPropertyName("channelType")]
  public string ChannelType { get; init; } = string.Empty;

  /// <summary>Channel type specific configuration.</summary>
  [JsonPropertyName("config")]
  public Dictionary<string, object?>? Config { get; init; }

  [JsonPropertyName("enabled")]
  public bool? Enabled { get; init; }

  [JsonPropertyName("recipients")]
  public AlertRecipient[]? Recipients { get; init; }
}

public class CreateAlertOptions
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>The kind of resource the alert watches, e.g. "certificate".</summary>
  [JsonPropertyName("resourceType")]
  public string ResourceType { get; init; } = string.Empty;

  [JsonPropertyName("resourceId")]
  public string? ResourceId { get; init; }

  /// <summary>The event that triggers the alert, e.g. "expiration".</summary>
  [JsonPropertyName("eventType")]
  public string EventType { get; init; } = string.Empty;

  /// <summary>Event specific condition, e.g. <c>new { daysBeforeExpiry = 30 }</c>.</summary>
  [JsonPropertyName("condition")]
  public object? Condition { get; init; }

  [JsonPropertyName("enabled")]
  public bool? Enabled { get; init; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("channels")]
  public AlertChannel[] Channels { get; init; } = Array.Empty<AlertChannel>();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
    if (string.IsNullOrEmpty(ResourceType)) throw new InfisicalException("ResourceType is required");
    if (string.IsNullOrEmpty(EventType)) throw new InfisicalException("EventType is required");
    if (Channels == null || Channels.Length == 0) throw new InfisicalException("At least one channel is required");
  }
}

public class UpdateAlertOptions
{
  [JsonIgnore]
  public string AlertId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("condition")]
  public object? Condition { get; init; }

  [JsonPropertyName("enabled")]
  public bool? Enabled { get; init; }

  /// <summary>Replaces the channels of the alert.</summary>
  [JsonPropertyName("channels")]
  public AlertChannel[]? Channels { get; init; }
}

public class ListAlertsOptions
{
  public string ResourceType { get; init; } = string.Empty;
  public string? ResourceId { get; init; }
  public string? ProjectId { get; init; }
  public bool? Enabled { get; init; }
}

public class TestAlertChannelOptions
{
  [JsonPropertyName("resourceType")]
  public string ResourceType { get; init; } = string.Empty;

  [JsonPropertyName("resourceId")]
  public string? ResourceId { get; init; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  /// <summary>Test an existing alert / channel instead of an unsaved one.</summary>
  [JsonPropertyName("alertId")]
  public string? AlertId { get; init; }

  [JsonPropertyName("channelId")]
  public string? ChannelId { get; init; }

  [JsonPropertyName("channelType")]
  public string ChannelType { get; init; } = string.Empty;

  [JsonPropertyName("config")]
  public Dictionary<string, object?>? Config { get; init; }

  [JsonPropertyName("recipients")]
  public AlertRecipient[]? Recipients { get; init; }
}

public class AlertChannelTestResult
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("deliveredTo")]
  public int? DeliveredTo { get; set; }

  [JsonPropertyName("error")]
  public string? Error { get; set; }
}

// ---------------------------------------------------------------------------------------------
// KMIP servers
// ---------------------------------------------------------------------------------------------

public class KmipServer : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("hostnamesOrIps")]
  public string? HostnamesOrIps { get; set; }

  [JsonPropertyName("ttl")]
  public string? Ttl { get; set; }

  [JsonPropertyName("keyAlgorithm")]
  public string? KeyAlgorithm { get; set; }

  [JsonPropertyName("canRevoke")]
  public bool? CanRevoke { get; set; }

  [JsonPropertyName("authMethod")]
  public JsonElement? AuthMethod { get; set; }
}

public class CreateKmipServerOptions
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  /// <summary>Comma separated hostnames and / or IP addresses of the server certificate.</summary>
  [JsonPropertyName("hostnamesOrIps")]
  public string HostnamesOrIps { get; init; } = string.Empty;

  /// <summary>Certificate lifetime, at least "1h" (default "1y").</summary>
  [JsonPropertyName("ttl")]
  public string? Ttl { get; init; }

  [JsonPropertyName("keyAlgorithm")]
  public string? KeyAlgorithm { get; init; }

  /// <summary>"token" or "aws" (see <see cref="GatewayAuthMethod"/>).</summary>
  [JsonPropertyName("authMethod")]
  public GatewayAuthMethod AuthMethod { get; init; } = GatewayAuthMethod.Token();
}

public class UpdateKmipServerOptions
{
  [JsonIgnore]
  public string KmipServerId { get; init; } = string.Empty;

  [JsonPropertyName("hostnamesOrIps")]
  public string? HostnamesOrIps { get; init; }

  [JsonPropertyName("ttl")]
  public string? Ttl { get; init; }

  [JsonPropertyName("keyAlgorithm")]
  public string? KeyAlgorithm { get; init; }

  [JsonPropertyName("authMethod")]
  public GatewayAuthMethod? AuthMethod { get; init; }
}

// ---------------------------------------------------------------------------------------------
// Code signing
// ---------------------------------------------------------------------------------------------

public class Signer : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("certificateId")]
  public string? CertificateId { get; set; }

  [JsonPropertyName("certificateCommonName")]
  public string? CertificateCommonName { get; set; }
}

public class SignerMember
{
  /// <summary>"user", "identity" or "group".</summary>
  [JsonPropertyName("kind")]
  public string Kind { get; init; } = string.Empty;

  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;

  [JsonPropertyName("role")]
  public string Role { get; init; } = string.Empty;
}

public class CreateSignerOptions
{
  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  /// <summary>CA that issues the signing certificate.</summary>
  [JsonPropertyName("caId")]
  public string? CaId { get; init; }

  [JsonPropertyName("commonName")]
  public string? CommonName { get; init; }

  [JsonPropertyName("certificateTtlDays")]
  public int? CertificateTtlDays { get; init; }

  [JsonPropertyName("certificateRenewBeforeDays")]
  public int? CertificateRenewBeforeDays { get; init; }

  /// <summary>"RSA_2048", "RSA_3072", "RSA_4096", "ECDSA_P256", "ECDSA_P384" or "ECDSA_P521".</summary>
  [JsonPropertyName("keyAlgorithm")]
  public string? KeyAlgorithm { get; init; }

  /// <summary>Use an existing certificate instead of issuing a new one.</summary>
  [JsonPropertyName("certificateId")]
  public string? CertificateId { get; init; }

  /// <summary>Key storage, e.g. <c>{ ["keySource"] = "hsm", ["hsmConnectorId"] = "..." }</c>.</summary>
  [JsonPropertyName("certificate")]
  public Dictionary<string, object?>? Certificate { get; init; }

  [JsonPropertyName("externalConfiguration")]
  public Dictionary<string, object?>? ExternalConfiguration { get; init; }

  [JsonPropertyName("approvalPolicyId")]
  public string? ApprovalPolicyId { get; init; }

  [JsonPropertyName("members")]
  public SignerMember[]? Members { get; init; }

  [JsonPropertyName("approvalPolicy")]
  public object? ApprovalPolicy { get; init; }
}

public class UpdateSignerOptions
{
  [JsonIgnore]
  public string SignerId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("certificateRenewBeforeDays")]
  public int? CertificateRenewBeforeDays { get; init; }
}

public class ReissueSignerCertificateOptions
{
  [JsonIgnore]
  public string SignerId { get; init; } = string.Empty;

  [JsonPropertyName("caId")]
  public string CaId { get; init; } = string.Empty;

  [JsonPropertyName("commonName")]
  public string? CommonName { get; init; }

  [JsonPropertyName("certificateTtlDays")]
  public int? CertificateTtlDays { get; init; }

  [JsonPropertyName("keyAlgorithm")]
  public string? KeyAlgorithm { get; init; }

  [JsonPropertyName("certificate")]
  public Dictionary<string, object?>? Certificate { get; init; }

  [JsonPropertyName("externalConfiguration")]
  public Dictionary<string, object?>? ExternalConfiguration { get; init; }
}

public class SignDataOptions
{
  [JsonIgnore]
  public string SignerId { get; init; } = string.Empty;

  /// <summary>Base64 encoded data (at most 128 bytes) or digest.</summary>
  [JsonPropertyName("data")]
  public string Data { get; init; } = string.Empty;

  /// <summary>Signing algorithm as listed in the API reference, e.g. "RSASSA_PKCS1_V1_5_SHA_256".</summary>
  [JsonPropertyName("signingAlgorithm")]
  public string SigningAlgorithm { get; init; } = string.Empty;

  [JsonPropertyName("isDigest")]
  public bool? IsDigest { get; init; }

  /// <summary>Optional audit metadata (tool, signingApplication, hostname, command, osUsername, ...).</summary>
  [JsonPropertyName("clientMetadata")]
  public Dictionary<string, object?>? ClientMetadata { get; init; }
}

public class SigningRequestOptions
{
  [JsonIgnore]
  public string SignerId { get; init; } = string.Empty;

  [JsonPropertyName("justification")]
  public string Justification { get; init; } = string.Empty;

  [JsonPropertyName("requestedSignings")]
  public int? RequestedSignings { get; init; }

  [JsonPropertyName("requestedWindowDuration")]
  public string? RequestedWindowDuration { get; init; }

  [JsonPropertyName("scope")]
  public object? Scope { get; init; }

  /// <summary>Only for pre-approvals: the user that receives the approval.</summary>
  [JsonPropertyName("granteeUserId")]
  public string? GranteeUserId { get; init; }

  /// <summary>Only for pre-approvals: the machine identity that receives the approval.</summary>
  [JsonPropertyName("granteeIdentityId")]
  public string? GranteeIdentityId { get; init; }
}
