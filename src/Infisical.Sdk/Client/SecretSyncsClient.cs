using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>
/// Secret syncs to external destinations (AWS Parameter Store, GitHub, Vercel, ...). All destinations share the same
/// API shape, the destination is selected with one of <see cref="SecretSyncDestinations"/>.
/// </summary>
public class SecretSyncsClient
{
  private readonly ApiClient _apiClient;

  public SecretSyncsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Base(string destination)
  {
    if (string.IsNullOrEmpty(destination)) throw new InfisicalException("Destination is required");
    return $"/api/v1/secret-syncs/{Uri.EscapeDataString(destination)}";
  }

  private static string Path(string destination, string syncId)
  {
    if (string.IsNullOrEmpty(syncId)) throw new InfisicalException("SyncId is required");
    return $"{Base(destination)}/{Uri.EscapeDataString(syncId)}";
  }

  /// <summary>Lists the available sync destinations and their capabilities (raw response).</summary>
  public async Task<JsonElement[]> ListOptionsAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/secret-syncs/options", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<JsonElement>(response, "secretSyncOptions");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret sync options", e);
    }
  }

  /// <summary>Lists the syncs of all destinations of a project.</summary>
  public async Task<SecretSyncInfo[]> ListAsync(string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/secret-syncs", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretSyncInfo>(response, "secretSyncs");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret syncs", e);
    }
  }

  /// <summary>Lists the syncs of a single destination of a project.</summary>
  public async Task<SecretSyncInfo[]> ListByDestinationAsync(string destination, string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Base(destination), null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretSyncInfo>(response, "secretSyncs");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret syncs", e);
    }
  }

  public async Task<SecretSyncInfo> GetAsync(string destination, string syncId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(destination, syncId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretSyncInfo>(response, "secretSync");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get secret sync", e);
    }
  }

  public async Task<SecretSyncInfo> GetByNameAsync(string destination, string syncName, string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(syncName)) throw new InfisicalException("SyncName is required");
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Base(destination)}/sync-name/{Uri.EscapeDataString(syncName)}", null, query, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretSyncInfo>(response, "secretSync");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get secret sync", e);
    }
  }

  public async Task<SecretSyncInfo> CreateAsync(CreateSecretSyncOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Base(options.Destination), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretSyncInfo>(response, "secretSync");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create secret sync", e);
    }
  }

  public async Task<SecretSyncInfo> UpdateAsync(UpdateSecretSyncOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.Destination, options.SyncId), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretSyncInfo>(response, "secretSync");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update secret sync", e);
    }
  }

  /// <param name="removeSecrets">Also remove the synced secrets from the destination.</param>
  public async Task<SecretSyncInfo> DeleteAsync(string destination, string syncId, bool removeSecrets = false)
  {
    try
    {
      var query = new QueryBuilder().Add("removeSecrets", removeSecrets).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(destination, syncId), null, query).ConfigureAwait(false);
      return ResponseReader.Entity<SecretSyncInfo>(response, "secretSync");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete secret sync", e);
    }
  }

  /// <summary>Triggers a sync of the secrets to the destination.</summary>
  public async Task<SecretSyncInfo> SyncSecretsAsync(string destination, string syncId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(destination, syncId) + "/sync-secrets").ConfigureAwait(false);
      return ResponseReader.Entity<SecretSyncInfo>(response, "secretSync");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to trigger secret sync", e);
    }
  }

  /// <summary>Imports the secrets of the destination into Infisical. See <see cref="SecretSyncImportBehaviors"/>.</summary>
  public async Task<SecretSyncInfo> ImportSecretsAsync(string destination, string syncId, string importBehavior)
  {
    try
    {
      if (string.IsNullOrEmpty(importBehavior)) throw new InfisicalException("ImportBehavior is required");
      var query = new QueryBuilder().Add("importBehavior", importBehavior).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(destination, syncId) + "/import-secrets", null, query).ConfigureAwait(false);
      return ResponseReader.Entity<SecretSyncInfo>(response, "secretSync");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to import secrets", e);
    }
  }

  /// <summary>Removes the synced secrets from the destination.</summary>
  public async Task<SecretSyncInfo> RemoveSecretsAsync(string destination, string syncId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(destination, syncId) + "/remove-secrets").ConfigureAwait(false);
      return ResponseReader.Entity<SecretSyncInfo>(response, "secretSync");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to remove secrets from destination", e);
    }
  }

  /// <summary>Checks whether the destination configuration conflicts with another sync (raw response).</summary>
  public async Task<JsonElement> CheckDestinationAsync(CheckSecretSyncDestinationOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.ProjectId)) throw new InfisicalException("ProjectId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Base(options.Destination) + "/check-destination", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to check sync destination", e);
    }
  }
}
