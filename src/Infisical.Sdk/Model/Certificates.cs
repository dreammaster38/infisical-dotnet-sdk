using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Certificates (Certificate Manager)
// ---------------------------------------------------------------------------------------------

public class SubjectAlternativeName
{
  /// <summary>"dns_name", "ip_address", "email" or "uri".</summary>
  [JsonPropertyName("type")]
  public string Type { get; init; } = string.Empty;

  [JsonPropertyName("value")]
  public string Value { get; init; } = string.Empty;
}

public class CertificateAttributes
{
  [JsonPropertyName("commonName")]
  public string? CommonName { get; init; }

  [JsonPropertyName("organization")]
  public string? Organization { get; init; }

  [JsonPropertyName("organizationalUnit")]
  public string? OrganizationalUnit { get; init; }

  [JsonPropertyName("country")]
  public string? Country { get; init; }

  [JsonPropertyName("state")]
  public string? State { get; init; }

  [JsonPropertyName("locality")]
  public string? Locality { get; init; }

  [JsonPropertyName("keyUsages")]
  public string[]? KeyUsages { get; init; }

  [JsonPropertyName("extendedKeyUsages")]
  public string[]? ExtendedKeyUsages { get; init; }

  [JsonPropertyName("altNames")]
  public SubjectAlternativeName[]? AltNames { get; init; }

  /// <summary>E.g. "RSA-SHA256", "ECDSA-SHA256".</summary>
  [JsonPropertyName("signatureAlgorithm")]
  public string? SignatureAlgorithm { get; init; }

  /// <summary>E.g. "RSA_2048", "EC_prime256v1".</summary>
  [JsonPropertyName("keyAlgorithm")]
  public string? KeyAlgorithm { get; init; }

  /// <summary>Validity as duration, e.g. "90d". Mutually exclusive with <see cref="NotBefore"/> / <see cref="NotAfter"/>.</summary>
  [JsonPropertyName("ttl")]
  public string? Ttl { get; init; }

  [JsonPropertyName("notBefore")]
  public string? NotBefore { get; init; }

  [JsonPropertyName("notAfter")]
  public string? NotAfter { get; init; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; init; }
}

public class IssueCertificateRequest
{
  [JsonPropertyName("profileId")]
  public string ProfileId { get; init; } = string.Empty;

  /// <summary>Optional PEM encoded CSR. If omitted, the server generates the key pair.</summary>
  [JsonPropertyName("csr")]
  public string? Csr { get; init; }

  [JsonPropertyName("attributes")]
  public CertificateAttributes? Attributes { get; init; }

  [JsonPropertyName("applicationId")]
  public string? ApplicationId { get; init; }

  [JsonPropertyName("removeRootsFromChain")]
  public bool? RemoveRootsFromChain { get; init; }

  [JsonPropertyName("metadata")]
  public IdentityMetadataEntry[]? Metadata { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProfileId)) throw new InfisicalException("ProfileId is required");
  }
}

/// <summary>PEM material of a certificate.</summary>
public class CertificateMaterial : ApiModel
{
  [JsonPropertyName("certificate")]
  public string? Certificate { get; set; }

  [JsonPropertyName("issuingCaCertificate")]
  public string? IssuingCaCertificate { get; set; }

  [JsonPropertyName("certificateChain")]
  public string? CertificateChain { get; set; }

  [JsonPropertyName("privateKey")]
  public string? PrivateKey { get; set; }

  [JsonPropertyName("serialNumber")]
  public string? SerialNumber { get; set; }

  [JsonPropertyName("certificateId")]
  public string? CertificateId { get; set; }
}

public class CertificateIssueResult
{
  public CertificateIssueResult(CertificateMaterial? material, string? certificateRequestId, string? status, JsonElement raw)
  {
    Material = material;
    CertificateRequestId = certificateRequestId;
    Status = status;
    Raw = raw;
  }

  /// <summary>The issued certificate. Null when issuance is asynchronous (e.g. external CAs); poll the request instead.</summary>
  public CertificateMaterial? Material { get; }

  public string? CertificateRequestId { get; }

  public string? Status { get; }

  /// <summary>The unmodified response.</summary>
  public JsonElement Raw { get; }
}

public class CertificateInfo : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("serialNumber")]
  public string? SerialNumber { get; set; }

  [JsonPropertyName("commonName")]
  public string? CommonName { get; set; }

  [JsonPropertyName("friendlyName")]
  public string? FriendlyName { get; set; }

  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("notBefore")]
  public string? NotBefore { get; set; }

  [JsonPropertyName("notAfter")]
  public string? NotAfter { get; set; }

  [JsonPropertyName("caId")]
  public string? CaId { get; set; }

  [JsonPropertyName("profileId")]
  public string? ProfileId { get; set; }

  [JsonPropertyName("revokedAt")]
  public string? RevokedAt { get; set; }

  [JsonPropertyName("revocationReason")]
  public int? RevocationReason { get; set; }
}

public class SearchCertificatesOptions
{
  [JsonIgnore]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("offset")]
  public int? Offset { get; init; }

  [JsonPropertyName("limit")]
  public int? Limit { get; init; }

  [JsonPropertyName("search")]
  public string? Search { get; init; }

  [JsonPropertyName("commonName")]
  public string? CommonName { get; init; }

  [JsonPropertyName("friendlyName")]
  public string? FriendlyName { get; init; }

  /// <summary>E.g. "active", "expired", "revoked".</summary>
  [JsonPropertyName("status")]
  public string? Status { get; init; }

  [JsonPropertyName("profileIds")]
  public string[]? ProfileIds { get; init; }

  [JsonPropertyName("caIds")]
  public string[]? CaIds { get; init; }

  [JsonPropertyName("metadata")]
  public IdentityMetadataEntry[]? Metadata { get; init; }

  [JsonPropertyName("notAfterFrom")]
  public string? NotAfterFrom { get; init; }

  [JsonPropertyName("notAfterTo")]
  public string? NotAfterTo { get; init; }
}

public class CertificateSearchResult
{
  public CertificateSearchResult(CertificateInfo[] certificates, int? totalCount)
  {
    Certificates = certificates;
    TotalCount = totalCount;
  }

  public CertificateInfo[] Certificates { get; }
  public int? TotalCount { get; }
}

public class RenewCertificateOptions
{
  [JsonIgnore]
  public string CertificateId { get; init; } = string.Empty;

  [JsonPropertyName("removeRootsFromChain")]
  public bool? RemoveRootsFromChain { get; init; }

  /// <summary>"existing" or "new".</summary>
  [JsonPropertyName("renewalKeySource")]
  public string? RenewalKeySource { get; init; }

  [JsonPropertyName("csr")]
  public string? Csr { get; init; }

  [JsonPropertyName("attributes")]
  public CertificateAttributes? Attributes { get; init; }
}

public class ImportCertificateOptions
{
  [JsonPropertyName("certificatePem")]
  public string CertificatePem { get; init; } = string.Empty;

  [JsonPropertyName("privateKeyPem")]
  public string? PrivateKeyPem { get; init; }

  [JsonPropertyName("chainPem")]
  public string? ChainPem { get; init; }

  [JsonPropertyName("friendlyName")]
  public string? FriendlyName { get; init; }

  [JsonPropertyName("profileId")]
  public string? ProfileId { get; init; }

  [JsonPropertyName("applicationId")]
  public string? ApplicationId { get; init; }

  /// <summary>Project the certificate is imported into (needed when authenticating with an identity).</summary>
  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }
}

public class SearchCertificateRequestsOptions
{
  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("offset")]
  public int? Offset { get; init; }

  [JsonPropertyName("limit")]
  public int? Limit { get; init; }

  [JsonPropertyName("search")]
  public string? Search { get; init; }

  [JsonPropertyName("status")]
  public string? Status { get; init; }

  [JsonPropertyName("profileIds")]
  public string[]? ProfileIds { get; init; }

  [JsonPropertyName("sortBy")]
  public string? SortBy { get; init; }

  /// <summary>"asc" or "desc".</summary>
  [JsonPropertyName("sortOrder")]
  public string? SortOrder { get; init; }
}

// ---------------------------------------------------------------------------------------------
// Profiles and policies
// ---------------------------------------------------------------------------------------------

public class CertificateProfile : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string Slug { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("caId")]
  public string? CaId { get; set; }

  [JsonPropertyName("certificatePolicyId")]
  public string? CertificatePolicyId { get; set; }

  [JsonPropertyName("enrollmentType")]
  public string? EnrollmentType { get; set; }

  [JsonPropertyName("issuerType")]
  public string? IssuerType { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }
}

public class CertificateProfileOptions
{
  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? ProfileId { get; init; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("caId")]
  public string? CaId { get; init; }

  [JsonPropertyName("certificatePolicyId")]
  public string? CertificatePolicyId { get; init; }

  /// <summary>"api", "est", "acme" or "scep".</summary>
  [JsonPropertyName("enrollmentType")]
  public string? EnrollmentType { get; init; }

  /// <summary>"ca" or "self-signed".</summary>
  [JsonPropertyName("issuerType")]
  public string? IssuerType { get; init; }

  /// <summary>E.g. <c>{ ["autoRenew"] = true, ["renewBeforeDays"] = 7 }</c>.</summary>
  [JsonPropertyName("apiConfig")]
  public Dictionary<string, object?>? ApiConfig { get; init; }

  [JsonPropertyName("estConfig")]
  public Dictionary<string, object?>? EstConfig { get; init; }

  [JsonPropertyName("acmeConfig")]
  public Dictionary<string, object?>? AcmeConfig { get; init; }

  [JsonPropertyName("scepConfig")]
  public Dictionary<string, object?>? ScepConfig { get; init; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; init; }
}

public class CertificatePolicy : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }
}

/// <summary>
/// Certificate policy. The rule sections (subject, sans, keyUsages, extendedKeyUsages, algorithms, validity,
/// basicConstraints, customExtensions) are passed as plain objects / dictionaries that follow the API reference.
/// </summary>
public class CertificatePolicyOptions
{
  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? PolicyId { get; init; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; init; }

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("subject")]
  public object[]? Subject { get; init; }

  [JsonPropertyName("sans")]
  public object[]? Sans { get; init; }

  [JsonPropertyName("keyUsages")]
  public object? KeyUsages { get; init; }

  [JsonPropertyName("extendedKeyUsages")]
  public object? ExtendedKeyUsages { get; init; }

  [JsonPropertyName("algorithms")]
  public object? Algorithms { get; init; }

  [JsonPropertyName("validity")]
  public object? Validity { get; init; }

  [JsonPropertyName("basicConstraints")]
  public object? BasicConstraints { get; init; }

  [JsonPropertyName("customExtensions")]
  public object[]? CustomExtensions { get; init; }
}

// ---------------------------------------------------------------------------------------------
// Certificate authorities
// ---------------------------------------------------------------------------------------------

public class CertificateAuthority : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("type")]
  public string? Type { get; set; }

  [JsonPropertyName("projectId")]
  public string? ProjectId { get; set; }

  [JsonPropertyName("configuration")]
  public JsonElement? Configuration { get; set; }
}

public class InternalCaConfiguration
{
  /// <summary>"root" or "intermediate".</summary>
  [JsonPropertyName("type")]
  public string Type { get; init; } = "root";

  [JsonPropertyName("friendlyName")]
  public string? FriendlyName { get; init; }

  [JsonPropertyName("commonName")]
  public string? CommonName { get; init; }

  [JsonPropertyName("organization")]
  public string? Organization { get; init; }

  [JsonPropertyName("ou")]
  public string? Ou { get; init; }

  [JsonPropertyName("country")]
  public string? Country { get; init; }

  [JsonPropertyName("province")]
  public string? Province { get; init; }

  [JsonPropertyName("locality")]
  public string? Locality { get; init; }

  [JsonPropertyName("notBefore")]
  public string? NotBefore { get; init; }

  [JsonPropertyName("notAfter")]
  public string? NotAfter { get; init; }

  [JsonPropertyName("maxPathLength")]
  public int? MaxPathLength { get; init; }

  /// <summary>E.g. "RSA_2048", "RSA_4096", "EC_prime256v1".</summary>
  [JsonPropertyName("keyAlgorithm")]
  public string? KeyAlgorithm { get; init; }

  [JsonPropertyName("crlDistributionPointUrls")]
  public string[]? CrlDistributionPointUrls { get; init; }

  [JsonPropertyName("isOcspEnabled")]
  public bool? IsOcspEnabled { get; init; }

  [JsonExtensionData]
  public Dictionary<string, object?>? Extra { get; init; }
}

public class CreateInternalCaOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  /// <summary>"active" (default) or "disabled".</summary>
  [JsonPropertyName("status")]
  public string Status { get; init; } = "active";

  [JsonPropertyName("configuration")]
  public InternalCaConfiguration Configuration { get; init; } = new InternalCaConfiguration();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
  }
}

public class UpdateInternalCaOptions
{
  [JsonIgnore]
  public string CaId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  /// <summary>"active" or "disabled".</summary>
  [JsonPropertyName("status")]
  public string? Status { get; init; }

  [JsonPropertyName("configuration")]
  public InternalCaConfiguration? Configuration { get; init; }
}

public class SignIntermediateOptions
{
  [JsonIgnore]
  public string CaId { get; init; } = string.Empty;

  [JsonPropertyName("csr")]
  public string Csr { get; init; } = string.Empty;

  [JsonPropertyName("notBefore")]
  public string? NotBefore { get; init; }

  [JsonPropertyName("notAfter")]
  public string NotAfter { get; init; } = string.Empty;

  [JsonPropertyName("maxPathLength")]
  public int? MaxPathLength { get; init; }
}
