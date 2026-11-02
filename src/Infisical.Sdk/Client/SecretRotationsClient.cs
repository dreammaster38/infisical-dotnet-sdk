using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>
/// Secret rotations (automatic rotation of database credentials, API keys, ...). All rotation types share the same API
/// shape, the type is selected with one of <see cref="SecretRotationTypes"/>.
/// </summary>
public class SecretRotationsClient
{
  private readonly ApiClient _apiClient;

  public SecretRotationsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Base(string type)
  {
    if (string.IsNullOrEmpty(type)) throw new InfisicalException("Type is required");
    return $"/api/v2/secret-rotations/{Uri.EscapeDataString(type)}";
  }

  private static string Path(string type, string rotationId)
  {
    if (string.IsNullOrEmpty(rotationId)) throw new InfisicalException("RotationId is required");
    return $"{Base(type)}/{Uri.EscapeDataString(rotationId)}";
  }

  /// <summary>Lists the available rotation types and their capabilities (raw response).</summary>
  public async Task<JsonElement[]> ListOptionsAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v2/secret-rotations/options", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<JsonElement>(response, "secretRotationOptions");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret rotation options", e);
    }
  }

  /// <summary>Lists the rotations of all types of a project.</summary>
  public async Task<SecretRotationInfo[]> ListAsync(string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v2/secret-rotations", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretRotationInfo>(response, "secretRotations");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret rotations", e);
    }
  }

  /// <summary>Lists the rotations of one type of a project.</summary>
  public async Task<SecretRotationInfo[]> ListByTypeAsync(string type, string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Base(type), null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretRotationInfo>(response, "secretRotations");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret rotations", e);
    }
  }

  public async Task<SecretRotationInfo> GetAsync(string type, string rotationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(type, rotationId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretRotationInfo>(response, "secretRotation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get secret rotation", e);
    }
  }

  public async Task<SecretRotationInfo> GetByNameAsync(string type, string rotationName, string projectId, string secretPath, string environmentSlug)
  {
    try
    {
      if (string.IsNullOrEmpty(rotationName)) throw new InfisicalException("RotationName is required");
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Add("secretPath", secretPath).Add("environment", environmentSlug).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Base(type)}/rotation-name/{Uri.EscapeDataString(rotationName)}", null, query, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretRotationInfo>(response, "secretRotation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get secret rotation", e);
    }
  }

  public async Task<SecretRotationInfo> CreateAsync(CreateSecretRotationOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Base(options.Type), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretRotationInfo>(response, "secretRotation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create secret rotation", e);
    }
  }

  public async Task<SecretRotationInfo> UpdateAsync(UpdateSecretRotationOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.Type, options.RotationId), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretRotationInfo>(response, "secretRotation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update secret rotation", e);
    }
  }

  public async Task<SecretRotationInfo> DeleteAsync(DeleteSecretRotationOptions options)
  {
    try
    {
      var query = new QueryBuilder()
        .Add("deleteSecrets", options.DeleteSecrets)
        .Add("revokeGeneratedCredentials", options.RevokeGeneratedCredentials)
        .Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(options.Type, options.RotationId), null, query).ConfigureAwait(false);
      return ResponseReader.Entity<SecretRotationInfo>(response, "secretRotation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete secret rotation", e);
    }
  }

  /// <summary>Gets the credentials the rotation generated.</summary>
  public async Task<SecretRotationGeneratedCredentials> GetGeneratedCredentialsAsync(string type, string rotationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(type, rotationId) + "/generated-credentials", null, null, true).ConfigureAwait(false);
      var credentials = ResponseReader.List<JsonElement>(response, "generatedCredentials");
      int? activeIndex = response.ValueKind == JsonValueKind.Object && response.TryGetProperty("activeIndex", out var a) && a.ValueKind == JsonValueKind.Number ? a.GetInt32() : (int?)null;
      string? id = response.ValueKind == JsonValueKind.Object && response.TryGetProperty("rotationId", out var r) && r.ValueKind == JsonValueKind.String ? r.GetString() : null;
      return new SecretRotationGeneratedCredentials(credentials, activeIndex, id);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get generated credentials", e);
    }
  }

  /// <summary>Rotates the credentials immediately.</summary>
  public async Task<SecretRotationInfo> RotateSecretsAsync(string type, string rotationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(type, rotationId) + "/rotate-secrets").ConfigureAwait(false);
      return ResponseReader.Entity<SecretRotationInfo>(response, "secretRotation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to rotate secrets", e);
    }
  }

  /// <summary>Verifies that the current credentials of the rotation work.</summary>
  public async Task<SecretRotationInfo> CheckCredentialsAsync(string type, string rotationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(type, rotationId) + "/check-credentials").ConfigureAwait(false);
      return ResponseReader.Entity<SecretRotationInfo>(response, "secretRotation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to check credentials", e);
    }
  }

  /// <summary>Moves the rotation (and its secrets) to another environment / folder.</summary>
  public async Task<SecretRotationInfo> MoveAsync(string type, string rotationId, string destinationEnvironment, string destinationSecretPath, bool overwriteDestination = false)
  {
    try
    {
      if (string.IsNullOrEmpty(destinationEnvironment)) throw new InfisicalException("DestinationEnvironment is required");
      if (string.IsNullOrEmpty(destinationSecretPath)) throw new InfisicalException("DestinationSecretPath is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(type, rotationId) + "/move", new { destinationEnvironment, destinationSecretPath, overwriteDestination }).ConfigureAwait(false);
      return ResponseReader.Entity<SecretRotationInfo>(response, "secretRotation");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to move secret rotation", e);
    }
  }
}
