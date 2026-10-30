using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>Certificates of the Infisical Certificate Manager.</summary>
public class CertificatesClient
{
  private const string BasePath = "/api/v1/cert-manager/certificates";
  private readonly ApiClient _apiClient;

  public CertificatesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string id)
  {
    if (string.IsNullOrEmpty(id)) throw new InfisicalException("Id is required");
    return $"{BasePath}/{Uri.EscapeDataString(id)}";
  }

  /// <summary>Requests a certificate using a certificate profile.</summary>
  public async Task<CertificateIssueResult> IssueAsync(IssueCertificateRequest request)
  {
    try
    {
      request.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, request).ConfigureAwait(false);
      return ReadIssueResult(response);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to issue certificate", e);
    }
  }

  public async Task<CertificateInfo> GetAsync(string certificateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(certificateId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateInfo>(response, "certificate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate", e);
    }
  }

  /// <summary>Gets the PEM encoded certificate and chain.</summary>
  public async Task<CertificateMaterial> GetCertificateAsync(string certificateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(certificateId) + "/certificate", null, null, true).ConfigureAwait(false);
      return ResponseReader.Deserialize<CertificateMaterial>(response) ?? new CertificateMaterial();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate", e);
    }
  }

  /// <summary>Gets certificate, chain and private key.</summary>
  public async Task<CertificateMaterial> GetBundleAsync(string certificateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(certificateId) + "/bundle", null, null, true).ConfigureAwait(false);
      return ResponseReader.Deserialize<CertificateMaterial>(response) ?? new CertificateMaterial();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate bundle", e);
    }
  }

  public async Task<CertificateMaterial> GetPrivateKeyAsync(string certificateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(certificateId) + "/private-key", null, null, true).ConfigureAwait(false);
      return ResponseReader.Deserialize<CertificateMaterial>(response) ?? new CertificateMaterial();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate private key", e);
    }
  }

  /// <summary>Exports the certificate as a PKCS#12 keystore (raw response, contains the base64 encoded keystore).</summary>
  public async Task<JsonElement> ExportPkcs12Async(string certificateId, string password, string alias)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(certificateId) + "/pkcs12", new { password, alias }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to export certificate as PKCS#12", e);
    }
  }

  public async Task<CertificateSearchResult> SearchAsync(SearchCertificatesOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.ProjectId)) throw new InfisicalException("ProjectId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"/api/v1/projects/{Uri.EscapeDataString(options.ProjectId)}/certificates/search", options).ConfigureAwait(false);
      int? total = response.ValueKind == JsonValueKind.Object && response.TryGetProperty("totalCount", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : (int?)null;
      return new CertificateSearchResult(ResponseReader.List<CertificateInfo>(response, "certificates"), total);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to search certificates", e);
    }
  }

  public async Task<CertificateIssueResult> RenewAsync(RenewCertificateOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(options.CertificateId) + "/renew", options).ConfigureAwait(false);
      return ReadIssueResult(response);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to renew certificate", e);
    }
  }

  /// <summary>Enables / configures automatic renewal.</summary>
  public async Task<JsonElement> UpdateRenewalConfigAsync(string certificateId, bool? enableAutoRenewal = null, int? renewBeforeDays = null)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(certificateId) + "/config", new { enableAutoRenewal, renewBeforeDays }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update certificate renewal config", e);
    }
  }

  /// <param name="revocationReason">CRL reason, e.g. "unspecified", "keyCompromise", "cessationOfOperation".</param>
  public async Task<JsonElement> RevokeAsync(string certificateId, string revocationReason = "unspecified")
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(certificateId) + "/revoke", new { revocationReason }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to revoke certificate", e);
    }
  }

  public async Task<CertificateInfo> DeleteAsync(string certificateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(certificateId)).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateInfo>(response, "certificate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete certificate", e);
    }
  }

  public async Task<JsonElement> ImportAsync(ImportCertificateOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.CertificatePem)) throw new InfisicalException("CertificatePem is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath + "/import-certificate", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to import certificate", e);
    }
  }

  // --- certificate requests ---------------------------------------------------------------------

  /// <summary>Gets the state of an (asynchronous) certificate request (raw response).</summary>
  public async Task<JsonElement> GetRequestAsync(string requestId)
  {
    try
    {
      if (string.IsNullOrEmpty(requestId)) throw new InfisicalException("RequestId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{BasePath}/certificate-requests/{Uri.EscapeDataString(requestId)}", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate request", e);
    }
  }

  public async Task<JsonElement> SearchRequestsAsync(SearchCertificateRequestsOptions options)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath + "/certificate-requests/search", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to search certificate requests", e);
    }
  }

  public async Task<JsonElement> CancelRequestAsync(string requestId)
  {
    try
    {
      if (string.IsNullOrEmpty(requestId)) throw new InfisicalException("RequestId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"{BasePath}/certificate-requests/{Uri.EscapeDataString(requestId)}/cancel").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to cancel certificate request", e);
    }
  }

  // The material is either nested ({ certificate: { certificate, privateKey, ... } }) or flat ({ certificate: "..." }).
  private static CertificateIssueResult ReadIssueResult(JsonElement response)
  {
    JsonElement source = response;
    if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("certificate", out var inner) && inner.ValueKind == JsonValueKind.Object)
    {
      source = inner;
    }

    CertificateMaterial? material = null;
    if (source.ValueKind == JsonValueKind.Object && source.TryGetProperty("certificate", out var pem) && pem.ValueKind == JsonValueKind.String)
    {
      material = ResponseReader.Deserialize<CertificateMaterial>(source);
    }

    string? Read(string name) => response.ValueKind == JsonValueKind.Object && response.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    return new CertificateIssueResult(material, Read("certificateRequestId"), Read("status"), response);
  }
}

public class CertificateProfilesClient
{
  private const string BasePath = "/api/v1/cert-manager/certificate-profiles";
  private readonly ApiClient _apiClient;

  public CertificateProfilesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string id)
  {
    if (string.IsNullOrEmpty(id)) throw new InfisicalException("Id is required");
    return $"{BasePath}/{Uri.EscapeDataString(id)}";
  }

  public async Task<CertificateProfile> CreateAsync(CertificateProfileOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Slug)) throw new InfisicalException("Slug is required");
      if (string.IsNullOrEmpty(options.CertificatePolicyId)) throw new InfisicalException("CertificatePolicyId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateProfile>(response, "certificateProfile");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create certificate profile", e);
    }
  }

  public async Task<CertificateProfile[]> ListAsync(string projectId, string? search = null, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId).Add("search", search!).Add("offset", offset!).Add("limit", limit!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<CertificateProfile>(response, "certificateProfiles");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list certificate profiles", e);
    }
  }

  public async Task<CertificateProfile> GetAsync(string profileId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(profileId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateProfile>(response, "certificateProfile");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate profile", e);
    }
  }

  public async Task<CertificateProfile> GetBySlugAsync(string slug, string? projectId = null)
  {
    try
    {
      if (string.IsNullOrEmpty(slug)) throw new InfisicalException("Slug is required");
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{BasePath}/slug/{Uri.EscapeDataString(slug)}", null, query, true).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateProfile>(response, "certificateProfile");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate profile", e);
    }
  }

  public async Task<CertificateProfile> UpdateAsync(CertificateProfileOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.ProfileId)) throw new InfisicalException("ProfileId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.ProfileId!), options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateProfile>(response, "certificateProfile");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update certificate profile", e);
    }
  }

  public async Task<CertificateProfile> DeleteAsync(string profileId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(profileId)).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateProfile>(response, "certificateProfile");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete certificate profile", e);
    }
  }

  /// <summary>Lists the certificates issued through a profile (raw response).</summary>
  public async Task<JsonElement> ListCertificatesAsync(string profileId, string? status = null, string? search = null, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("status", status!).Add("search", search!).Add("offset", offset!).Add("limit", limit!).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(profileId) + "/certificates", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list profile certificates", e);
    }
  }

  /// <summary>Gets the latest active certificate including its private key.</summary>
  public async Task<CertificateMaterial> GetLatestActiveBundleAsync(string profileId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(profileId) + "/certificates/latest-active-bundle", null, null, true).ConfigureAwait(false);
      return ResponseReader.Deserialize<CertificateMaterial>(response) ?? new CertificateMaterial();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get latest active certificate bundle", e);
    }
  }
}

public class CertificatePoliciesClient
{
  private const string BasePath = "/api/v1/cert-manager/certificate-policies";
  private readonly ApiClient _apiClient;

  public CertificatePoliciesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string id)
  {
    if (string.IsNullOrEmpty(id)) throw new InfisicalException("Id is required");
    return $"{BasePath}/{Uri.EscapeDataString(id)}";
  }

  public async Task<CertificatePolicy> CreateAsync(CertificatePolicyOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificatePolicy>(response, "certificatePolicy");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create certificate policy", e);
    }
  }

  public async Task<CertificatePolicy[]> ListAsync(string projectId, string? search = null, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId).Add("search", search!).Add("offset", offset!).Add("limit", limit!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<CertificatePolicy>(response, "certificatePolicies");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list certificate policies", e);
    }
  }

  public async Task<CertificatePolicy> GetAsync(string policyId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(policyId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<CertificatePolicy>(response, "certificatePolicy");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate policy", e);
    }
  }

  public async Task<CertificatePolicy> UpdateAsync(CertificatePolicyOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.PolicyId)) throw new InfisicalException("PolicyId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.PolicyId!), options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificatePolicy>(response, "certificatePolicy");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update certificate policy", e);
    }
  }

  public async Task<CertificatePolicy> DeleteAsync(string policyId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(policyId)).ConfigureAwait(false);
      return ResponseReader.Entity<CertificatePolicy>(response, "certificatePolicy");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete certificate policy", e);
    }
  }
}

/// <summary>Internal certificate authorities (root and intermediate CAs hosted by Infisical).</summary>
public class CertificateAuthoritiesClient
{
  private const string BasePath = "/api/v1/cert-manager/ca/internal";
  private readonly ApiClient _apiClient;

  public CertificateAuthoritiesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string caId)
  {
    if (string.IsNullOrEmpty(caId)) throw new InfisicalException("CaId is required");
    return $"{BasePath}/{Uri.EscapeDataString(caId)}";
  }

  public async Task<CertificateAuthority> CreateAsync(CreateInternalCaOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateAuthority>(response, "ca", "certificateAuthority");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create certificate authority", e);
    }
  }

  public async Task<CertificateAuthority[]> ListAsync(string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<CertificateAuthority>(response, "certificateAuthorities", "cas");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list certificate authorities", e);
    }
  }

  public async Task<CertificateAuthority> GetAsync(string caId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(caId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateAuthority>(response, "ca", "certificateAuthority");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate authority", e);
    }
  }

  public async Task<CertificateAuthority> UpdateAsync(UpdateInternalCaOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.CaId), options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateAuthority>(response, "ca", "certificateAuthority");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update certificate authority", e);
    }
  }

  public async Task<CertificateAuthority> DeleteAsync(string caId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(caId)).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateAuthority>(response, "ca", "certificateAuthority");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete certificate authority", e);
    }
  }

  /// <summary>Gets the CSR of an intermediate CA (to be signed by a parent CA).</summary>
  public async Task<string> GetCsrAsync(string caId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(caId) + "/csr", null, null, true).ConfigureAwait(false);
      return response.GetProperty("csr").GetString() ?? string.Empty;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get CA CSR", e);
    }
  }

  /// <summary>Gets the active CA certificate and chain.</summary>
  public async Task<CertificateMaterial> GetCertificateAsync(string caId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(caId) + "/certificate", null, null, true).ConfigureAwait(false);
      return ResponseReader.Deserialize<CertificateMaterial>(response) ?? new CertificateMaterial();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get CA certificate", e);
    }
  }

  /// <summary>Gets all certificates of a CA, e.g. after renewals (raw response).</summary>
  public async Task<JsonElement> GetCaCertificatesAsync(string caId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(caId) + "/ca-certificates", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get CA certificates", e);
    }
  }

  public async Task<JsonElement> GetCrlsAsync(string caId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(caId) + "/crls", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get CA CRLs", e);
    }
  }

  /// <summary>Renews the CA certificate. <paramref name="type"/> is "existing" or "new".</summary>
  public async Task<CertificateMaterial> RenewAsync(string caId, string type, string notAfter)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(caId) + "/renew", new { type, notAfter }).ConfigureAwait(false);
      return ResponseReader.Deserialize<CertificateMaterial>(response) ?? new CertificateMaterial();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to renew CA certificate", e);
    }
  }

  /// <summary>Signs the CSR of an intermediate CA with this CA.</summary>
  public async Task<CertificateMaterial> SignIntermediateAsync(SignIntermediateOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Csr)) throw new InfisicalException("Csr is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(options.CaId) + "/sign-intermediate", options).ConfigureAwait(false);
      return ResponseReader.Deserialize<CertificateMaterial>(response) ?? new CertificateMaterial();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to sign intermediate CA", e);
    }
  }

  /// <summary>Installs a (signed) certificate and chain for an intermediate CA.</summary>
  public async Task<JsonElement> ImportCertificateAsync(string caId, string certificate, string certificateChain)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(caId) + "/import-certificate", new { certificate, certificateChain }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to import CA certificate", e);
    }
  }

  public async Task<JsonElement> GetAutoRenewalAsync(string caId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(caId) + "/auto-renewal", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get CA auto-renewal config", e);
    }
  }

  public async Task<JsonElement> UpdateAutoRenewalAsync(string caId, bool? autoRenewalEnabled = null, int? autoRenewalDaysBeforeExpiry = null)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(caId) + "/auto-renewal", new { autoRenewalEnabled, autoRenewalDaysBeforeExpiry }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update CA auto-renewal config", e);
    }
  }

  /// <summary>Gets how the CA certificate is signed (raw response).</summary>
  public async Task<JsonElement> GetSigningConfigAsync(string caId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(caId) + "/signing-config", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get CA signing config", e);
    }
  }

  /// <summary>
  /// Creates the signing config. <paramref name="config"/> is an object with a <c>type</c> ("internal", "manual",
  /// "venafi", ...) and the type specific properties, see the API reference.
  /// </summary>
  public async Task<JsonElement> CreateSigningConfigAsync(string caId, object config)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(caId) + "/signing-config", config).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create CA signing config", e);
    }
  }

  public async Task<JsonElement> UpdateSigningConfigAsync(string caId, object config)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(caId) + "/signing-config", config).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update CA signing config", e);
    }
  }
}
