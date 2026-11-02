using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>Infisical gateways (access to private networks).</summary>
public class GatewaysClient
{
  private const string BasePath = "/api/v3/gateways";
  private readonly ApiClient _apiClient;

  public GatewaysClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string gatewayId)
  {
    if (string.IsNullOrEmpty(gatewayId)) throw new InfisicalException("GatewayId is required");
    return $"{BasePath}/{Uri.EscapeDataString(gatewayId)}";
  }

  public async Task<Gateway> CreateAsync(CreateGatewayOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      if (string.IsNullOrEmpty(options.AuthMethod?.Method)) throw new InfisicalException("AuthMethod.Method is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<Gateway>(response, "gateway");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create gateway", e);
    }
  }

  public async Task<Gateway> GetAsync(string gatewayId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(gatewayId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<Gateway>(response, "gateway");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get gateway", e);
    }
  }

  public async Task<Gateway> UpdateAsync(UpdateGatewayOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.GatewayId), options).ConfigureAwait(false);
      return ResponseReader.Entity<Gateway>(response, "gateway");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update gateway", e);
    }
  }

  /// <summary>Revokes the gateway's access.</summary>
  public async Task<JsonElement?> RevokeAsync(string gatewayId)
  {
    try
    {
      return await _apiClient.SendAsync<JsonElement?>(HttpMethod.Post, Path(gatewayId) + "/revoke").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to revoke gateway", e);
    }
  }

  /// <summary>Generates an enrollment token for a gateway that uses the token auth method.</summary>
  public async Task<GatewayEnrollmentToken> GenerateEnrollmentTokenAsync(string gatewayId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(gatewayId) + "/token-auth/generate-enrollment-token").ConfigureAwait(false);
      var token = response.GetProperty("token").GetString() ?? string.Empty;
      var expiresAt = response.TryGetProperty("expiresAt", out var e) && e.ValueKind == JsonValueKind.String ? e.GetString() : null;
      return new GatewayEnrollmentToken(token, expiresAt);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to generate enrollment token", e);
    }
  }
}

/// <summary>Templates that define the environments, roles and members of newly created projects.</summary>
public class ProjectTemplatesClient
{
  private const string BasePath = "/api/v1/project-templates";
  private readonly ApiClient _apiClient;

  public ProjectTemplatesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string templateId)
  {
    if (string.IsNullOrEmpty(templateId)) throw new InfisicalException("TemplateId is required");
    return $"{BasePath}/{Uri.EscapeDataString(templateId)}";
  }

  public async Task<ProjectTemplate[]> ListAsync(string? type = null)
  {
    try
    {
      var query = new QueryBuilder().Add("type", type!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<ProjectTemplate>(response, "projectTemplates");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list project templates", e);
    }
  }

  public async Task<ProjectTemplate> GetAsync(string templateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(templateId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectTemplate>(response, "projectTemplate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get project template", e);
    }
  }

  public async Task<ProjectTemplate> CreateAsync(ProjectTemplateOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      if (string.IsNullOrEmpty(options.Type)) throw new InfisicalException("Type is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectTemplate>(response, "projectTemplate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create project template", e);
    }
  }

  public async Task<ProjectTemplate> UpdateAsync(ProjectTemplateOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.TemplateId)) throw new InfisicalException("TemplateId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.TemplateId!), options).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectTemplate>(response, "projectTemplate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update project template", e);
    }
  }

  public async Task<ProjectTemplate> DeleteAsync(string templateId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(templateId)).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectTemplate>(response, "projectTemplate");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete project template", e);
    }
  }
}
