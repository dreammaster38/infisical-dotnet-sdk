using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>Automatic deletion of expired certificates.</summary>
public class CertificateCleanupClient
{
  private const string BasePath = "/api/v1/cert-manager/certificate-cleanup";
  private readonly ApiClient _apiClient;

  public CertificateCleanupClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  /// <summary>Gets the cleanup configuration, or null if none was set up yet.</summary>
  public async Task<CertificateCleanupConfig?> GetConfigAsync(string? projectId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      var config = ResponseReader.Unwrap(response, "config");
      return config.ValueKind == JsonValueKind.Object ? ResponseReader.Deserialize<CertificateCleanupConfig>(config) : null;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate cleanup config", e);
    }
  }

  public async Task<CertificateCleanupConfig> UpdateConfigAsync(UpdateCertificateCleanupOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Put, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateCleanupConfig>(response, "config");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update certificate cleanup config", e);
    }
  }
}

/// <summary>
/// Legacy certificate templates (deprecated by certificate profiles and policies, see
/// <see cref="CertificateProfilesClient"/> / <see cref="CertificatePoliciesClient"/>).
/// </summary>
public class CertificateTemplatesClient
{
  private const string BasePath = "/api/v1/pki/certificate-templates";
  private readonly ApiClient _apiClient;

  public CertificateTemplatesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string templateId)
  {
    if (string.IsNullOrEmpty(templateId)) throw new InfisicalException("TemplateId is required");
    return $"{BasePath}/{Uri.EscapeDataString(templateId)}";
  }

  public async Task<CertificateTemplate> CreateAsync(CertificateTemplateOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.CaId)) throw new InfisicalException("CaId is required");
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateTemplate>(response, "certificateTemplate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create certificate template", e);
    }
  }

  public async Task<CertificateTemplate> GetAsync(string templateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(templateId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateTemplate>(response, "certificateTemplate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get certificate template", e);
    }
  }

  public async Task<CertificateTemplate> UpdateAsync(CertificateTemplateOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.TemplateId ?? string.Empty), options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateTemplate>(response, "certificateTemplate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update certificate template", e);
    }
  }

  public async Task<CertificateTemplate> DeleteAsync(string templateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(templateId)).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateTemplate>(response, "certificateTemplate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete certificate template", e);
    }
  }

  // --- EST --------------------------------------------------------------------------------------

  /// <summary>Gets the EST (enrollment over secure transport) configuration of the template (raw response).</summary>
  public async Task<JsonElement> GetEstConfigAsync(string templateId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(templateId) + "/est-config", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get EST config", e);
    }
  }

  public async Task<JsonElement> CreateEstConfigAsync(CertificateTemplateEstConfigOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Passphrase)) throw new InfisicalException("Passphrase is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(options.TemplateId) + "/est-config", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create EST config", e);
    }
  }

  public async Task<JsonElement> UpdateEstConfigAsync(CertificateTemplateEstConfigOptions options)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.TemplateId) + "/est-config", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update EST config", e);
    }
  }
}
