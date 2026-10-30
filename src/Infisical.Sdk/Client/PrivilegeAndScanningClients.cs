using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>Additional (custom, optionally temporary) project privileges of machine identities.</summary>
public class IdentityPrivilegesClient
{
  private const string BasePath = "/api/v2/identity-project-additional-privilege";
  private readonly ApiClient _apiClient;

  public IdentityPrivilegesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string id)
  {
    if (string.IsNullOrEmpty(id)) throw new InfisicalException("Id is required");
    return $"{BasePath}/{Uri.EscapeDataString(id)}";
  }

  public async Task<IdentityAdditionalPrivilege> CreateAsync(CreateIdentityPrivilegeOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<IdentityAdditionalPrivilege>(response, "privilege");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create identity privilege", e);
    }
  }

  public async Task<IdentityAdditionalPrivilege[]> ListAsync(string identityId, string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(identityId)) throw new InfisicalException("IdentityId is required");
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("identityId", identityId).Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<IdentityAdditionalPrivilege>(response, "privileges");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list identity privileges", e);
    }
  }

  public async Task<IdentityAdditionalPrivilege> GetAsync(string privilegeId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(privilegeId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<IdentityAdditionalPrivilege>(response, "privilege");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get identity privilege", e);
    }
  }

  public async Task<IdentityAdditionalPrivilege> GetBySlugAsync(string identityId, string projectSlug, string privilegeSlug)
  {
    try
    {
      if (string.IsNullOrEmpty(privilegeSlug)) throw new InfisicalException("PrivilegeSlug is required");
      var query = new QueryBuilder().Add("identityId", identityId).Add("projectSlug", projectSlug).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{BasePath}/slug/{Uri.EscapeDataString(privilegeSlug)}", null, query, true).ConfigureAwait(false);
      return ResponseReader.Entity<IdentityAdditionalPrivilege>(response, "privilege");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get identity privilege", e);
    }
  }

  public async Task<IdentityAdditionalPrivilege> UpdateAsync(UpdateIdentityPrivilegeOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.PrivilegeId), options).ConfigureAwait(false);
      return ResponseReader.Entity<IdentityAdditionalPrivilege>(response, "privilege");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update identity privilege", e);
    }
  }

  public async Task<IdentityAdditionalPrivilege> DeleteAsync(string privilegeId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(privilegeId)).ConfigureAwait(false);
      return ResponseReader.Entity<IdentityAdditionalPrivilege>(response, "privilege");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete identity privilege", e);
    }
  }
}

/// <summary>
/// Secret scanning (data sources, scans, findings). Note: these endpoints are not part of the public API reference,
/// they are the ones the Infisical web app uses and may change between Infisical versions.
/// </summary>
public class SecretScanningClient
{
  private const string BasePath = "/api/v2/secret-scanning";
  private readonly ApiClient _apiClient;

  public SecretScanningClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Source(string type)
  {
    if (string.IsNullOrEmpty(type)) throw new InfisicalException("Type is required");
    return $"{BasePath}/data-sources/{Uri.EscapeDataString(type)}";
  }

  private static string Source(string type, string dataSourceId)
  {
    if (string.IsNullOrEmpty(dataSourceId)) throw new InfisicalException("DataSourceId is required");
    return $"{Source(type)}/{Uri.EscapeDataString(dataSourceId)}";
  }

  // --- data sources -----------------------------------------------------------------------------

  /// <summary>Lists the available data source types (raw response).</summary>
  public async Task<JsonElement[]> ListDataSourceOptionsAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath + "/data-sources/options", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<JsonElement>(response, "dataSourceOptions");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list data source options", e);
    }
  }

  /// <summary>Lists the data sources of all types of a project.</summary>
  public async Task<SecretScanningDataSource[]> ListDataSourcesAsync(string? projectId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath + "/data-sources", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretScanningDataSource>(response, "dataSources");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list data sources", e);
    }
  }

  public async Task<SecretScanningDataSource> GetDataSourceAsync(string type, string dataSourceId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Source(type, dataSourceId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretScanningDataSource>(response, "dataSource");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get data source", e);
    }
  }

  public async Task<SecretScanningDataSource> GetDataSourceByNameAsync(string type, string name, string? projectId = null)
  {
    try
    {
      if (string.IsNullOrEmpty(name)) throw new InfisicalException("Name is required");
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Source(type)}/data-source-name/{Uri.EscapeDataString(name)}", null, query, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretScanningDataSource>(response, "dataSource");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get data source", e);
    }
  }

  public async Task<SecretScanningDataSource> CreateDataSourceAsync(CreateSecretScanningDataSourceOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Source(options.Type), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretScanningDataSource>(response, "dataSource");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create data source", e);
    }
  }

  public async Task<SecretScanningDataSource> UpdateDataSourceAsync(UpdateSecretScanningDataSourceOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Source(options.Type, options.DataSourceId), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretScanningDataSource>(response, "dataSource");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update data source", e);
    }
  }

  public async Task<SecretScanningDataSource> DeleteDataSourceAsync(string type, string dataSourceId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Source(type, dataSourceId)).ConfigureAwait(false);
      return ResponseReader.Entity<SecretScanningDataSource>(response, "dataSource");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete data source", e);
    }
  }

  /// <summary>Triggers a scan of all resources of the data source.</summary>
  public async Task<SecretScanningDataSource> TriggerScanAsync(string type, string dataSourceId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Source(type, dataSourceId) + "/scan").ConfigureAwait(false);
      return ResponseReader.Entity<SecretScanningDataSource>(response, "dataSource");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to trigger scan", e);
    }
  }

  // --- resources and scans (raw responses) ---------------------------------------------------------

  public async Task<JsonElement> ListResourcesAsync(string type, string dataSourceId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Source(type, dataSourceId) + "/resources", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list resources", e);
    }
  }

  /// <summary>Lists the resources (e.g. repositories) that could be added to the data source.</summary>
  public async Task<JsonElement> ListAvailableResourcesAsync(string type, string dataSourceId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Source(type, dataSourceId) + "/available-resources", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list available resources", e);
    }
  }

  public async Task<JsonElement> AddResourceAsync(string type, string dataSourceId, string resourceName)
  {
    try
    {
      if (string.IsNullOrEmpty(resourceName)) throw new InfisicalException("ResourceName is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Source(type, dataSourceId) + "/resources", new { resourceName }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to add resource", e);
    }
  }

  /// <summary>Adds all available resources to the data source.</summary>
  public async Task<JsonElement> AddAllResourcesAsync(string type, string dataSourceId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Source(type, dataSourceId) + "/resources/all").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to add resources", e);
    }
  }

  public async Task<JsonElement> RemoveResourceAsync(string type, string dataSourceId, string resourceId)
  {
    try
    {
      if (string.IsNullOrEmpty(resourceId)) throw new InfisicalException("ResourceId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"{Source(type, dataSourceId)}/resources/{Uri.EscapeDataString(resourceId)}").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to remove resource", e);
    }
  }

  /// <summary>Triggers a scan of a single resource.</summary>
  public async Task<JsonElement> TriggerResourceScanAsync(string type, string dataSourceId, string resourceId)
  {
    try
    {
      if (string.IsNullOrEmpty(resourceId)) throw new InfisicalException("ResourceId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"{Source(type, dataSourceId)}/resources/{Uri.EscapeDataString(resourceId)}/scan").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to trigger resource scan", e);
    }
  }

  public async Task<JsonElement> ListScansAsync(string type, string dataSourceId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Source(type, dataSourceId) + "/scans", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list scans", e);
    }
  }

  // --- findings ---------------------------------------------------------------------------------

  public async Task<SecretScanningFindingList> ListFindingsAsync(ListSecretScanningFindingsOptions? options = null)
  {
    try
    {
      options ??= new ListSecretScanningFindingsOptions();
      var query = new QueryBuilder()
        .Add("projectId", options.ProjectId!)
        .Add("status", options.Status!)
        .Add("dataSourceId", options.DataSourceId!)
        .Add("resourceId", options.ResourceId!)
        .Add("orderBy", options.OrderBy!)
        .Add("orderDirection", options.OrderDirection!)
        .Add("offset", options.Offset!)
        .Add("limit", options.Limit!)
        .Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath + "/findings", null, query, true).ConfigureAwait(false);
      int? total = response.ValueKind == JsonValueKind.Object && response.TryGetProperty("totalCount", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : (int?)null;
      return new SecretScanningFindingList(ResponseReader.List<SecretScanningFinding>(response, "findings"), total);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list findings", e);
    }
  }

  /// <summary>Triages a finding (status and / or comment).</summary>
  public async Task<SecretScanningFinding> UpdateFindingAsync(string findingId, string? status = null, string? triageComment = null)
  {
    try
    {
      if (string.IsNullOrEmpty(findingId)) throw new InfisicalException("FindingId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"{BasePath}/findings/{Uri.EscapeDataString(findingId)}", new { status, triageComment }).ConfigureAwait(false);
      return ResponseReader.Entity<SecretScanningFinding>(response, "finding");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update finding", e);
    }
  }

  /// <summary>Triages up to 500 findings at once.</summary>
  public async Task<SecretScanningFinding[]> UpdateFindingsAsync(params SecretScanningFindingUpdate[] updates)
  {
    try
    {
      if (updates == null || updates.Length == 0) throw new InfisicalException("At least one update is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, BasePath + "/findings", updates).ConfigureAwait(false);
      return ResponseReader.List<SecretScanningFinding>(response, "findings");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update findings", e);
    }
  }

  public async Task<int> GetUnresolvedFindingsCountAsync(string? projectId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath + "/unresolved-findings-count", null, query, true).ConfigureAwait(false);
      return response.GetProperty("unresolvedFindings").GetInt32();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get unresolved findings count", e);
    }
  }

  // --- config -----------------------------------------------------------------------------------

  /// <summary>Gets the scanning configuration (custom gitleaks config content).</summary>
  public async Task<JsonElement> GetConfigAsync(string? projectId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath + "/configs", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get scanning config", e);
    }
  }

  /// <param name="content">The config file content, or null to reset it.</param>
  public async Task<JsonElement> UpdateConfigAsync(string? content, string? projectId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, BasePath + "/configs", new SecretScanningConfigBody(content), query).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update scanning config", e);
    }
  }

  // content must be sent as null (not omitted) to reset the config
  private sealed class SecretScanningConfigBody
  {
    public SecretScanningConfigBody(string? content)
    {
      Content = content;
    }

    [System.Text.Json.Serialization.JsonPropertyName("content")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]
    public string? Content { get; }
  }
}
