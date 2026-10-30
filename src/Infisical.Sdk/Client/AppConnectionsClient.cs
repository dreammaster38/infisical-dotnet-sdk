using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>
/// App connections (credentials for external services such as AWS, GitHub or databases). All apps share the same API
/// shape, the app is selected with one of <see cref="AppConnectionTypes"/>.
/// </summary>
public class AppConnectionsClient
{
  private readonly ApiClient _apiClient;

  public AppConnectionsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Base(string app)
  {
    if (string.IsNullOrEmpty(app)) throw new InfisicalException("App is required");
    return $"/api/v1/app-connections/{Uri.EscapeDataString(app)}";
  }

  private static string Path(string app, string connectionId)
  {
    if (string.IsNullOrEmpty(connectionId)) throw new InfisicalException("ConnectionId is required");
    return $"{Base(app)}/{Uri.EscapeDataString(connectionId)}";
  }

  /// <summary>Lists the available app connection types and their authentication methods (raw response).</summary>
  public async Task<JsonElement[]> ListOptionsAsync(string? projectType = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectType", projectType!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/app-connections/options", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<JsonElement>(response, "appConnectionOptions");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list app connection options", e);
    }
  }

  /// <summary>Lists the connections of all apps. Pass a project ID to list the connections of a project.</summary>
  public async Task<AppConnectionInfo[]> ListAsync(string? projectId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/app-connections", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<AppConnectionInfo>(response, "appConnections");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list app connections", e);
    }
  }

  /// <summary>Lists the connections of one app.</summary>
  public async Task<AppConnectionInfo[]> ListByAppAsync(string app, string? projectId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Base(app), null, query, true).ConfigureAwait(false);
      return ResponseReader.List<AppConnectionInfo>(response, "appConnections");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list app connections", e);
    }
  }

  /// <summary>Lists the connections of one app that can be used in a project (includes organization connections).</summary>
  public async Task<AvailableAppConnection[]> ListAvailableAsync(string app, string? projectId = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Base(app) + "/available", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<AvailableAppConnection>(response, "appConnections");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list available app connections", e);
    }
  }

  public async Task<AppConnectionInfo> GetAsync(string app, string connectionId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(app, connectionId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get app connection", e);
    }
  }

  public async Task<AppConnectionInfo> GetByNameAsync(string app, string connectionName, string? projectId = null)
  {
    try
    {
      if (string.IsNullOrEmpty(connectionName)) throw new InfisicalException("ConnectionName is required");
      var query = new QueryBuilder().Add("projectId", projectId!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Base(app)}/connection-name/{Uri.EscapeDataString(connectionName)}", null, query, true).ConfigureAwait(false);
      return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get app connection", e);
    }
  }

  public async Task<AppConnectionInfo> CreateAsync(CreateAppConnectionOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Base(options.App), options).ConfigureAwait(false);
      return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create app connection", e);
    }
  }

  public async Task<AppConnectionInfo> UpdateAsync(UpdateAppConnectionOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.App, options.ConnectionId), options).ConfigureAwait(false);
      return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update app connection", e);
    }
  }

  public async Task<AppConnectionInfo> DeleteAsync(string app, string connectionId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(app, connectionId)).ConfigureAwait(false);
      return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete app connection", e);
    }
  }

  /// <summary>Rotates the credentials of a connection that supports credential rotation.</summary>
  public async Task<AppConnectionInfo> RotateCredentialsAsync(string app, string connectionId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(app, connectionId) + "/rotate-credentials").ConfigureAwait(false);
      return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to rotate app connection credentials", e);
    }
  }
}
