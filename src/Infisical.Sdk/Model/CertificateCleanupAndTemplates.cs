using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CertificateCleanupConfig : ApiModel
{
  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("isEnabled")]
  public bool? IsEnabled { get; set; }

  /// <summary>Days after expiry until a certificate is deleted (1 - 30).</summary>
  [JsonPropertyName("postExpiryRetentionDays")]
  public int? PostExpiryRetentionDays { get; set; }

  /// <summary>Keep expired certificates that are still used by an active PKI sync.</summary>
  [JsonPropertyName("skipCertsWithActiveSyncs")]
  public bool? SkipCertsWithActiveSyncs { get; set; }
}

public class UpdateCertificateCleanupOptions
{
  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("isEnabled")]
  public bool? IsEnabled { get; init; }

  [JsonPropertyName("postExpiryRetentionDays")]
  public int? PostExpiryRetentionDays { get; init; }

  [JsonPropertyName("skipCertsWithActiveSyncs")]
  public bool? SkipCertsWithActiveSyncs { get; init; }
}

/// <summary>Legacy certificate template. New setups should use certificate profiles and policies.</summary>
public class CertificateTemplate : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("caId")]
  public string? CaId { get; set; }

  [JsonPropertyName("commonName")]
  public string? CommonName { get; set; }

  [JsonPropertyName("subjectAlternativeName")]
  public string? SubjectAlternativeName { get; set; }

  [JsonPropertyName("ttl")]
  public string? Ttl { get; set; }

  [JsonPropertyName("keyUsages")]
  public string[]? KeyUsages { get; set; }

  [JsonPropertyName("extendedKeyUsages")]
  public string[]? ExtendedKeyUsages { get; set; }
}

public class CertificateTemplateOptions
{
  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? TemplateId { get; init; }

  [JsonPropertyName("caId")]
  public string? CaId { get; init; }

  [JsonPropertyName("pkiCollectionId")]
  public string? PkiCollectionId { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  /// <summary>Regular expression the common name has to match.</summary>
  [JsonPropertyName("commonName")]
  public string? CommonName { get; init; }

  /// <summary>Regular expression the subject alternative names have to match.</summary>
  [JsonPropertyName("subjectAlternativeName")]
  public string? SubjectAlternativeName { get; init; }

  /// <summary>Maximum TTL, e.g. "90d".</summary>
  [JsonPropertyName("ttl")]
  public string? Ttl { get; init; }

  [JsonPropertyName("keyUsages")]
  public string[]? KeyUsages { get; init; }

  [JsonPropertyName("extendedKeyUsages")]
  public string[]? ExtendedKeyUsages { get; init; }
}

public class CertificateTemplateEstConfigOptions
{
  [JsonIgnore]
  public string TemplateId { get; init; } = string.Empty;

  /// <summary>PEM encoded CA chain used to validate bootstrap certificates.</summary>
  [JsonPropertyName("caChain")]
  public string? CaChain { get; init; }

  [JsonPropertyName("passphrase")]
  public string? Passphrase { get; init; }

  [JsonPropertyName("isEnabled")]
  public bool? IsEnabled { get; init; }

  [JsonPropertyName("disableBootstrapCertValidation")]
  public bool? DisableBootstrapCertValidation { get; init; }
}
