using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>
/// Saved views of the certificate inventory (filters and columns). To query the certificates themselves use
/// <see cref="CertificatesClient.SearchAsync"/>. Note: these endpoints are not part of the public API reference.
/// </summary>
public class CertificateInventoryClient
{
  private const string BasePath = "/api/v1/cert-manager/certificate-inventory-views";
  private readonly ApiClient _apiClient;

  public CertificateInventoryClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string viewId)
  {
    if (string.IsNullOrEmpty(viewId)) throw new InfisicalException("ViewId is required");
    return $"{BasePath}/{Uri.EscapeDataString(viewId)}";
  }

  public async Task<CertificateInventoryViews> ListViewsAsync(string? projectId = null, string? applicationId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Add("applicationId", applicationId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return new CertificateInventoryViews(
        ResponseReader.List<CertificateInventoryView>(response, "systemViews"),
        ResponseReader.List<CertificateInventoryView>(response, "sharedViews"),
        ResponseReader.List<CertificateInventoryView>(response, "customViews"));
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list inventory views", e);
    }
  }

  public async Task<CertificateInventoryView> CreateViewAsync(CertificateInventoryViewOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateInventoryView>(response, "view");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create inventory view", e);
    }
  }

  public async Task<CertificateInventoryView> UpdateViewAsync(CertificateInventoryViewOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.ViewId ?? string.Empty), options).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateInventoryView>(response, "view");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update inventory view", e);
    }
  }

  public async Task<CertificateInventoryView> DeleteViewAsync(string viewId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(viewId)).ConfigureAwait(false);
      return ResponseReader.Entity<CertificateInventoryView>(response, "view");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete inventory view", e);
    }
  }
}
