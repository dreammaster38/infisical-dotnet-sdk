using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>Proxied services: hosts whose requests are rewritten with credentials from secrets or dynamic secrets.</summary>
public class ProxiedServicesClient
{
  private const string BasePath = "/api/v1/proxied-services";
  private readonly ApiClient _apiClient;

  public ProxiedServicesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string serviceId)
  {
    if (string.IsNullOrEmpty(serviceId)) throw new InfisicalException("ServiceId is required");
    return $"{BasePath}/{Uri.EscapeDataString(serviceId)}";
  }

  private static Dictionary<string, string> Query(ProxiedServiceLocation location)
  {
    location.Validate();
    return new QueryBuilder()
      .Add("projectId", location.ProjectId)
      .Add("environment", location.EnvironmentSlug)
      .Add("secretPath", location.SecretPath)
      .Build();
  }

  public async Task<ProxiedService> CreateAsync(CreateProxiedServiceOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<ProxiedService>(response, "service");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create proxied service", e);
    }
  }

  public async Task<ProxiedService[]> ListAsync(ProxiedServiceLocation location)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, Query(location), true).ConfigureAwait(false);
      return ResponseReader.List<ProxiedService>(response, "services");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list proxied services", e);
    }
  }

  public async Task<ProxiedService> GetAsync(string serviceId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(serviceId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<ProxiedService>(response, "service");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get proxied service", e);
    }
  }

  public async Task<ProxiedService> GetByNameAsync(string name, ProxiedServiceLocation location)
  {
    try
    {
      if (string.IsNullOrEmpty(name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{BasePath}/slug/{Uri.EscapeDataString(name)}", null, Query(location), true).ConfigureAwait(false);
      return ResponseReader.Entity<ProxiedService>(response, "service");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get proxied service", e);
    }
  }

  public async Task<ProxiedService> UpdateAsync(UpdateProxiedServiceOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.ServiceId), options).ConfigureAwait(false);
      return ResponseReader.Entity<ProxiedService>(response, "service");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update proxied service", e);
    }
  }

  public async Task<ProxiedService> DeleteAsync(string serviceId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(serviceId)).ConfigureAwait(false);
      return ResponseReader.Entity<ProxiedService>(response, "service");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete proxied service", e);
    }
  }

  /// <summary>Reports that the service was used (called by the proxy).</summary>
  public async Task ReportUsageAsync(string serviceId)
  {
    try
    {
      await _apiClient.SendAsync<JsonElement>(HttpMethod.Post, Path(serviceId) + "/report-usage").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to report proxied service usage", e);
    }
  }
}
