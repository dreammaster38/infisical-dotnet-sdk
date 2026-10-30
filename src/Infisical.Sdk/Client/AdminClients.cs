using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>SCIM tokens of an organization (used by identity providers to provision users and groups).</summary>
public class ScimClient
{
  private const string BasePath = "/api/v1/scim";
  private readonly ApiClient _apiClient;

  public ScimClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  /// <summary>Creates a SCIM token and returns it. The token is only shown once.</summary>
  /// <param name="ttlDays">Lifetime in days (max. 730), 0 means the token never expires.</param>
  public async Task<string> CreateTokenAsync(string organizationId, string? description = null, int ttlDays = 0)
  {
    try
    {
      if (string.IsNullOrEmpty(organizationId)) throw new InfisicalException("OrganizationId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath + "/scim-tokens", new { organizationId, description = description ?? string.Empty, ttlDays }).ConfigureAwait(false);
      return response.GetProperty("scimToken").GetString() ?? string.Empty;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create SCIM token", e);
    }
  }

  public async Task<ScimToken[]> ListTokensAsync(string organizationId)
  {
    try
    {
      if (string.IsNullOrEmpty(organizationId)) throw new InfisicalException("OrganizationId is required");
      var query = new QueryBuilder().Add("organizationId", organizationId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath + "/scim-tokens", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<ScimToken>(response, "scimTokens");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list SCIM tokens", e);
    }
  }

  public async Task<ScimToken> DeleteTokenAsync(string scimTokenId)
  {
    try
    {
      if (string.IsNullOrEmpty(scimTokenId)) throw new InfisicalException("ScimTokenId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"{BasePath}/scim-tokens/{Uri.EscapeDataString(scimTokenId)}").ConfigureAwait(false);
      return ResponseReader.Entity<ScimToken>(response, "scimToken");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete SCIM token", e);
    }
  }

  /// <summary>Lists the SCIM provisioning events of the organization (raw response).</summary>
  public async Task<JsonElement> ListEventsAsync(IDictionary<string, string>? query = null)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath + "/scim-events", null, query == null ? null : new Dictionary<string, string>(query), true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list SCIM events", e);
    }
  }
}

/// <summary>Organization administration: access to all projects of the organization.</summary>
public class OrganizationAdminClient
{
  private const string BasePath = "/api/v1/organization-admin";
  private readonly ApiClient _apiClient;

  public OrganizationAdminClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  /// <summary>Lists all projects of the organization, including the ones the caller is not a member of.</summary>
  public async Task<OrganizationAdminProjects> ListProjectsAsync(string? search = null, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("search", search!).Add("offset", offset!).Add("limit", limit!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath + "/projects", null, query, true).ConfigureAwait(false);
      int? count = response.ValueKind == JsonValueKind.Object && response.TryGetProperty("count", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : (int?)null;
      return new OrganizationAdminProjects(ResponseReader.List<Project>(response, "projects"), count);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list organization projects", e);
    }
  }

  /// <summary>Gives the caller admin access to a project (raw response: membership).</summary>
  public async Task<JsonElement> GrantProjectAdminAccessAsync(string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"{BasePath}/projects/{Uri.EscapeDataString(projectId)}/grant-admin-access").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to grant project admin access", e);
    }
  }
}

/// <summary>Instance level endpoints.</summary>
public class InstanceClient
{
  private readonly ApiClient _apiClient;

  public InstanceClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  /// <summary>Gets the status of the Infisical instance (raw response).</summary>
  public async Task<JsonElement> GetStatusAsync()
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/status", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get instance status", e);
    }
  }

  /// <summary>
  /// Bootstraps a fresh instance: creates the first admin user, the organization and a machine identity. This only
  /// works once on an instance without an admin. The response contains the credentials of the new machine identity.
  /// </summary>
  public async Task<JsonElement> BootstrapAsync(string email, string password, string organization)
  {
    try
    {
      if (string.IsNullOrEmpty(email)) throw new InfisicalException("Email is required");
      if (string.IsNullOrEmpty(password)) throw new InfisicalException("Password is required");
      if (string.IsNullOrEmpty(organization)) throw new InfisicalException("Organization is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v1/admin/bootstrap", new { email, password, organization }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to bootstrap instance", e);
    }
  }
}
