using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>Gateway pools (a group of gateways that share the load and provide failover).</summary>
public class GatewayPoolsClient
{
  private const string BasePath = "/api/v1/gateway-pools";
  private readonly ApiClient _apiClient;

  public GatewayPoolsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string poolId)
  {
    if (string.IsNullOrEmpty(poolId)) throw new InfisicalException("PoolId is required");
    return $"{BasePath}/{Uri.EscapeDataString(poolId)}";
  }

  public async Task<GatewayPool> CreateAsync(string name)
  {
    try
    {
      if (string.IsNullOrEmpty(name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, new { name }).ConfigureAwait(false);
      return ResponseReader.Entity<GatewayPool>(response, "gatewayPool", "pool");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create gateway pool", e);
    }
  }

  public async Task<GatewayPool[]> ListAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, null, true).ConfigureAwait(false);
      return ResponseReader.List<GatewayPool>(response, "gatewayPools", "pools");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list gateway pools", e);
    }
  }

  /// <summary>Gets a pool including its members (raw response).</summary>
  public async Task<JsonElement> GetAsync(string poolId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(poolId), null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get gateway pool", e);
    }
  }

  public async Task<GatewayPool> UpdateAsync(string poolId, string? name = null)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(poolId), new { name }).ConfigureAwait(false);
      return ResponseReader.Entity<GatewayPool>(response, "gatewayPool", "pool");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update gateway pool", e);
    }
  }

  public async Task<GatewayPool> DeleteAsync(string poolId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(poolId)).ConfigureAwait(false);
      return ResponseReader.Entity<GatewayPool>(response, "gatewayPool", "pool");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete gateway pool", e);
    }
  }

  public async Task<JsonElement> AddGatewayAsync(string poolId, string gatewayId)
  {
    try
    {
      if (string.IsNullOrEmpty(gatewayId)) throw new InfisicalException("GatewayId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(poolId) + "/memberships", new { gatewayId }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to add gateway to pool", e);
    }
  }

  public async Task<JsonElement> RemoveGatewayAsync(string poolId, string gatewayId)
  {
    try
    {
      if (string.IsNullOrEmpty(gatewayId)) throw new InfisicalException("GatewayId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"{Path(poolId)}/memberships/{Uri.EscapeDataString(gatewayId)}").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to remove gateway from pool", e);
    }
  }

  /// <summary>Lists the resources (e.g. app connections, discovery jobs) that use the pool (raw response).</summary>
  public async Task<JsonElement> ListResourcesAsync(string poolId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(poolId) + "/resources", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list gateway pool resources", e);
    }
  }
}

/// <summary>PKI discovery: scans of networks and servers for certificates, and the installations they find.</summary>
public class PkiDiscoveryClient
{
  private const string JobsPath = "/api/v1/cert-manager/discovery-jobs";
  private const string InstallationsPath = "/api/v1/cert-manager/installations";
  private readonly ApiClient _apiClient;

  public PkiDiscoveryClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Job(string discoveryId)
  {
    if (string.IsNullOrEmpty(discoveryId)) throw new InfisicalException("DiscoveryId is required");
    return $"{JobsPath}/{Uri.EscapeDataString(discoveryId)}";
  }

  private static string Installation(string installationId)
  {
    if (string.IsNullOrEmpty(installationId)) throw new InfisicalException("InstallationId is required");
    return $"{InstallationsPath}/{Uri.EscapeDataString(installationId)}";
  }

  /// <summary>Gets the limits and defaults of discovery jobs (ports, IPs, domains, CIDR size; raw response).</summary>
  public async Task<JsonElement> GetConfigAsync()
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, JobsPath + "/config", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get discovery config", e);
    }
  }

  public async Task<PkiDiscoveryJob> CreateJobAsync(PkiDiscoveryJobOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      if (options.TargetConfig == null) throw new InfisicalException("TargetConfig is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, JobsPath, options).ConfigureAwait(false);
      return ResponseReader.Entity<PkiDiscoveryJob>(response, "discovery");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create discovery job", e);
    }
  }

  public async Task<PkiDiscoveryJob[]> ListJobsAsync(string? projectId = null, string? search = null, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Add("search", search!).Add("offset", offset!).Add("limit", limit!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, JobsPath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<PkiDiscoveryJob>(response, "discoveries");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list discovery jobs", e);
    }
  }

  public async Task<PkiDiscoveryJob> GetJobAsync(string discoveryId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Job(discoveryId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<PkiDiscoveryJob>(response, "discovery");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get discovery job", e);
    }
  }

  public async Task<PkiDiscoveryJob> UpdateJobAsync(PkiDiscoveryJobOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Job(options.DiscoveryId ?? string.Empty), options).ConfigureAwait(false);
      return ResponseReader.Entity<PkiDiscoveryJob>(response, "discovery");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update discovery job", e);
    }
  }

  public async Task<PkiDiscoveryJob> DeleteJobAsync(string discoveryId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Job(discoveryId)).ConfigureAwait(false);
      return ResponseReader.Entity<PkiDiscoveryJob>(response, "discovery");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete discovery job", e);
    }
  }

  /// <summary>Starts a scan of the job.</summary>
  public async Task<JsonElement> TriggerScanAsync(string discoveryId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Job(discoveryId) + "/scan").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to trigger discovery scan", e);
    }
  }

  public async Task<JsonElement> GetLatestScanAsync(string discoveryId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Job(discoveryId) + "/latest-scan", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get latest discovery scan", e);
    }
  }

  public async Task<JsonElement> GetScanHistoryAsync(string discoveryId, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("offset", offset!).Add("limit", limit!).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Job(discoveryId) + "/scans", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get discovery scan history", e);
    }
  }

  // --- installations ----------------------------------------------------------------------------

  /// <summary>Lists the places where discovered certificates are installed.</summary>
  public async Task<PkiInstallation[]> ListInstallationsAsync(ListPkiInstallationsOptions? options = null)
  {
    try
    {
      options ??= new ListPkiInstallationsOptions();
      var query = new QueryBuilder()
        .Add("projectId", options.ProjectId!)
        .Add("discoveryId", options.DiscoveryId!)
        .Add("certificateId", options.CertificateId!)
        .Add("search", options.Search!)
        .Add("offset", options.Offset!)
        .Add("limit", options.Limit!)
        .Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, InstallationsPath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<PkiInstallation>(response, "installations");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list installations", e);
    }
  }

  public async Task<PkiInstallation> GetInstallationAsync(string installationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Installation(installationId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<PkiInstallation>(response, "installation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get installation", e);
    }
  }

  /// <param name="keystorePassword">Password of the keystore on the target, or null to leave it unchanged.</param>
  public async Task<PkiInstallation> UpdateInstallationAsync(string installationId, string? name = null, string? keystorePassword = null)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Installation(installationId), new { name, keystorePassword }).ConfigureAwait(false);
      return ResponseReader.Entity<PkiInstallation>(response, "installation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update installation", e);
    }
  }

  public async Task<PkiInstallation> DeleteInstallationAsync(string installationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Installation(installationId)).ConfigureAwait(false);
      return ResponseReader.Entity<PkiInstallation>(response, "installation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete installation", e);
    }
  }
}
