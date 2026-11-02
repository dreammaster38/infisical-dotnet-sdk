using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>Organizations, organization memberships, roles, sub-organizations, SSO configuration and audit logs.</summary>
public class OrganizationsClient
{
  private readonly ApiClient _apiClient;
  private readonly OrganizationRolesClient _roles;
  private readonly SubOrganizationsClient _subOrganizations;
  private readonly SsoClient _sso;
  private readonly ScimClient _scim;

  public OrganizationsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
    _roles = new OrganizationRolesClient(apiClient);
    _subOrganizations = new SubOrganizationsClient(apiClient);
    _sso = new SsoClient(apiClient);
    _scim = new ScimClient(apiClient);
  }

  public OrganizationRolesClient Roles() => _roles;
  public SubOrganizationsClient SubOrganizations() => _subOrganizations;
  public SsoClient Sso() => _sso;

  /// <summary>SCIM tokens and events.</summary>
  public ScimClient Scim() => _scim;

  private static string Org(string organizationId)
  {
    if (string.IsNullOrEmpty(organizationId)) throw new InfisicalException("OrganizationId is required");
    return Uri.EscapeDataString(organizationId);
  }

  /// <summary>Lists the organizations of the current user / identity.</summary>
  public async Task<Organization[]> ListAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/organization", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<Organization>(response, "organizations");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list organizations", e);
    }
  }

  public async Task<Organization> GetAsync(string organizationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/organization/{Org(organizationId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<Organization>(response, "organization");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get organization", e);
    }
  }

  public async Task<Organization> CreateAsync(string name)
  {
    try
    {
      if (string.IsNullOrEmpty(name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v2/organizations", new { name }).ConfigureAwait(false);
      return ResponseReader.Entity<Organization>(response, "organization");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create organization", e);
    }
  }

  public async Task<Organization> UpdateAsync(UpdateOrganizationOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v1/organization/{Org(options.OrganizationId)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<Organization>(response, "organization");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update organization", e);
    }
  }

  public async Task<Organization> DeleteAsync(string organizationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v2/organizations/{Org(organizationId)}").ConfigureAwait(false);
      return ResponseReader.Entity<Organization>(response, "organization");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete organization", e);
    }
  }

  // --- user memberships ------------------------------------------------------------------------

  public async Task<OrganizationMembership[]> ListMembershipsAsync(string organizationId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v2/organizations/{Org(organizationId)}/memberships", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<OrganizationMembership>(response, "users", "memberships");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list organization memberships", e);
    }
  }

  public async Task<OrganizationMembership> GetMembershipAsync(string organizationId, string membershipId)
  {
    try
    {
      if (string.IsNullOrEmpty(membershipId)) throw new InfisicalException("MembershipId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v2/organizations/{Org(organizationId)}/memberships/{Uri.EscapeDataString(membershipId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<OrganizationMembership>(response, "membership");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get organization membership", e);
    }
  }

  public async Task<OrganizationMembership> UpdateMembershipAsync(UpdateOrganizationMembershipOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.MembershipId)) throw new InfisicalException("MembershipId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v2/organizations/{Org(options.OrganizationId)}/memberships/{Uri.EscapeDataString(options.MembershipId)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<OrganizationMembership>(response, "membership");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update organization membership", e);
    }
  }

  public async Task<OrganizationMembership> DeleteMembershipAsync(string organizationId, string membershipId)
  {
    try
    {
      if (string.IsNullOrEmpty(membershipId)) throw new InfisicalException("MembershipId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v2/organizations/{Org(organizationId)}/memberships/{Uri.EscapeDataString(membershipId)}").ConfigureAwait(false);
      return ResponseReader.Entity<OrganizationMembership>(response, "membership");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete organization membership", e);
    }
  }

  public async Task<JsonElement> DeleteMembershipsAsync(string organizationId, params string[] membershipIds)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v2/organizations/{Org(organizationId)}/memberships", new { membershipIds }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete organization memberships", e);
    }
  }

  /// <summary>Lists the project memberships of an organization member (raw response).</summary>
  public async Task<JsonElement> ListMembershipProjectsAsync(string organizationId, string membershipId)
  {
    try
    {
      if (string.IsNullOrEmpty(membershipId)) throw new InfisicalException("MembershipId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v2/organizations/{Org(organizationId)}/memberships/{Uri.EscapeDataString(membershipId)}/project-memberships", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list membership projects", e);
    }
  }

  /// <summary>Lists the projects of an organization (raw response).</summary>
  public async Task<JsonElement> ListProjectsAsync(string organizationId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v2/organizations/{Org(organizationId)}/workspaces", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list organization projects", e);
    }
  }

  /// <summary>Lists the groups of an organization (raw response).</summary>
  public async Task<JsonElement> ListGroupsAsync(string organizationId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/organization/{Org(organizationId)}/groups", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list organization groups", e);
    }
  }

  // --- machine identity memberships ------------------------------------------------------------

  /// <summary>Lists the machine identities of an organization (raw response).</summary>
  public async Task<JsonElement> ListIdentityMembershipsAsync(ListOrganizationIdentitiesOptions options)
  {
    try
    {
      var query = new QueryBuilder()
        .Add("offset", options.Offset!)
        .Add("limit", options.Limit!)
        .Add("orderBy", options.OrderBy!)
        .Add("orderDirection", options.OrderDirection!)
        .Add("search", options.Search!)
        .Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v2/organizations/{Org(options.OrganizationId)}/identity-memberships", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list organization identities", e);
    }
  }

  // --- audit logs ------------------------------------------------------------------------------

  /// <summary>Queries the audit logs of the organization (raw response: auditLogs).</summary>
  public async Task<JsonElement> ListAuditLogsAsync(AuditLogsQuery? options = null)
  {
    try
    {
      options ??= new AuditLogsQuery();
      var query = new QueryBuilder()
        .Add("projectId", options.ProjectId!)
        .Add("environment", options.Environment!)
        .Add("actorType", options.ActorType!)
        .Add("actor", options.Actor!)
        .Add("secretPath", options.SecretPath!)
        .Add("secretKey", options.SecretKey!)
        .Add("eventType", options.EventTypes == null ? null! : string.Join(",", options.EventTypes))
        .Add("userAgentType", options.UserAgentType!)
        .Add("startDate", options.StartDate!)
        .Add("endDate", options.EndDate!)
        .Add("offset", options.Offset!)
        .Add("limit", options.Limit!)
        .Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/organization/audit-logs", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list audit logs", e);
    }
  }

  public async Task<JsonElement> GetAuditLogSettingsAsync()
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/organization/audit-log-settings", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get audit log settings", e);
    }
  }

  /// <param name="settings">Settings object, see the "Update Organization Audit Log Settings" API reference.</param>
  public async Task<JsonElement> UpdateAuditLogSettingsAsync(object settings)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Put, "/api/v1/organization/audit-log-settings", settings).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update audit log settings", e);
    }
  }
}

public class OrganizationRolesClient
{
  private const string BasePath = "/api/v1/organization/roles";
  private readonly ApiClient _apiClient;

  public OrganizationRolesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  public async Task<OrganizationRole> CreateAsync(OrganizationRoleOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Slug)) throw new InfisicalException("Slug is required");
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<OrganizationRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create organization role", e);
    }
  }

  public async Task<OrganizationRole[]> ListAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, null, true).ConfigureAwait(false);
      return ResponseReader.List<OrganizationRole>(response, "roles");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list organization roles", e);
    }
  }

  public async Task<OrganizationRole> GetAsync(string roleId)
  {
    try
    {
      if (string.IsNullOrEmpty(roleId)) throw new InfisicalException("RoleId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{BasePath}/{Uri.EscapeDataString(roleId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<OrganizationRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get organization role", e);
    }
  }

  public async Task<OrganizationRole> GetBySlugAsync(string roleSlug)
  {
    try
    {
      if (string.IsNullOrEmpty(roleSlug)) throw new InfisicalException("RoleSlug is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{BasePath}/slug/{Uri.EscapeDataString(roleSlug)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<OrganizationRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get organization role", e);
    }
  }

  public async Task<OrganizationRole> UpdateAsync(OrganizationRoleOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.RoleId)) throw new InfisicalException("RoleId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"{BasePath}/{Uri.EscapeDataString(options.RoleId!)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<OrganizationRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update organization role", e);
    }
  }

  public async Task<OrganizationRole> DeleteAsync(string roleId)
  {
    try
    {
      if (string.IsNullOrEmpty(roleId)) throw new InfisicalException("RoleId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"{BasePath}/{Uri.EscapeDataString(roleId)}").ConfigureAwait(false);
      return ResponseReader.Entity<OrganizationRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete organization role", e);
    }
  }
}

public class SubOrganizationsClient
{
  private const string BasePath = "/api/v1/sub-organizations";
  private readonly ApiClient _apiClient;

  public SubOrganizationsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  public async Task<SubOrganization> CreateAsync(string name, string? slug = null)
  {
    try
    {
      if (string.IsNullOrEmpty(name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, new { name, slug }).ConfigureAwait(false);
      return ResponseReader.Entity<SubOrganization>(response, "organization");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create sub-organization", e);
    }
  }

  public async Task<SubOrganization[]> ListAsync(ListSubOrganizationsOptions? options = null)
  {
    try
    {
      options ??= new ListSubOrganizationsOptions();
      var query = new QueryBuilder()
        .Add("limit", options.Limit!)
        .Add("offset", options.Offset!)
        .Add("search", options.Search!)
        .Add("orderBy", options.OrderBy!)
        .Add("orderDirection", options.OrderDirection!)
        .Add("isAccessible", options.IsAccessible!)
        .Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SubOrganization>(response, "organizations");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list sub-organizations", e);
    }
  }

  public async Task<SubOrganization> UpdateAsync(string subOrgId, string? name = null, string? slug = null)
  {
    try
    {
      if (string.IsNullOrEmpty(subOrgId)) throw new InfisicalException("SubOrgId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"{BasePath}/{Uri.EscapeDataString(subOrgId)}", new { name, slug }).ConfigureAwait(false);
      return ResponseReader.Entity<SubOrganization>(response, "organization");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update sub-organization", e);
    }
  }

  public async Task<SubOrganization> DeleteAsync(string subOrgId)
  {
    try
    {
      if (string.IsNullOrEmpty(subOrgId)) throw new InfisicalException("SubOrgId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"{BasePath}/{Uri.EscapeDataString(subOrgId)}").ConfigureAwait(false);
      return ResponseReader.Entity<SubOrganization>(response, "organization");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete sub-organization", e);
    }
  }

  /// <summary>Adds the calling identity / user as member of the sub-organization.</summary>
  public async Task<JsonElement> CreateMembershipAsync(string subOrgId)
  {
    try
    {
      if (string.IsNullOrEmpty(subOrgId)) throw new InfisicalException("SubOrgId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"{BasePath}/{Uri.EscapeDataString(subOrgId)}/memberships").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create sub-organization membership", e);
    }
  }
}

/// <summary>SSO configuration of an organization (LDAP, OIDC, SAML) and SCIM group mappings. Responses are returned raw.</summary>
public class SsoClient
{
  private readonly ApiClient _apiClient;

  public SsoClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private async Task<JsonElement> Call(HttpMethod method, string path, object? body, string? organizationId, string error)
  {
    try
    {
      var query = organizationId == null ? null : new QueryBuilder().Add("organizationId", organizationId).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(method, path, body, query, method == HttpMethod.Get).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException(error, e);
    }
  }

  // --- LDAP -------------------------------------------------------------------------------------
  public Task<JsonElement> GetLdapConfigAsync(string organizationId) => Call(HttpMethod.Get, "/api/v1/ldap/config", null, organizationId, "Failed to get LDAP config");
  public Task<JsonElement> CreateLdapConfigAsync(LdapConfigOptions options) => Call(HttpMethod.Post, "/api/v1/ldap/config", options, null, "Failed to create LDAP config");
  public Task<JsonElement> UpdateLdapConfigAsync(LdapConfigOptions options) => Call(HttpMethods.Patch, "/api/v1/ldap/config", options, null, "Failed to update LDAP config");
  public Task<JsonElement> TestLdapConnectionAsync(LdapConfigOptions options) => Call(HttpMethod.Post, "/api/v1/ldap/config/test-connection", options, null, "Failed to test LDAP connection");
  public Task<JsonElement> ListLdapGroupMapsAsync(string configId) => Call(HttpMethod.Get, $"/api/v1/ldap/config/{Uri.EscapeDataString(configId)}/group-maps", null, null, "Failed to list LDAP group maps");
  public Task<JsonElement> CreateLdapGroupMapAsync(string configId, string ldapGroupCN, string groupSlug) => Call(HttpMethod.Post, $"/api/v1/ldap/config/{Uri.EscapeDataString(configId)}/group-maps", new { ldapGroupCN, groupSlug }, null, "Failed to create LDAP group map");
  public Task<JsonElement> DeleteLdapGroupMapAsync(string configId, string groupMapId) => Call(HttpMethod.Delete, $"/api/v1/ldap/config/{Uri.EscapeDataString(configId)}/group-maps/{Uri.EscapeDataString(groupMapId)}", null, null, "Failed to delete LDAP group map");

  // --- OIDC -------------------------------------------------------------------------------------
  public Task<JsonElement> GetOidcConfigAsync(string organizationId) => Call(HttpMethod.Get, "/api/v1/sso/oidc/config", null, organizationId, "Failed to get OIDC config");
  public Task<JsonElement> CreateOidcConfigAsync(OidcConfigOptions options) => Call(HttpMethod.Post, "/api/v1/sso/oidc/config", options, null, "Failed to create OIDC config");
  public Task<JsonElement> UpdateOidcConfigAsync(OidcConfigOptions options) => Call(HttpMethods.Patch, "/api/v1/sso/oidc/config", options, null, "Failed to update OIDC config");

  // --- SAML -------------------------------------------------------------------------------------
  public Task<JsonElement> GetSamlConfigAsync(string organizationId) => Call(HttpMethod.Get, "/api/v1/sso/config", null, organizationId, "Failed to get SAML config");
  public Task<JsonElement> CreateSamlConfigAsync(SamlConfigOptions options) => Call(HttpMethod.Post, "/api/v1/sso/config", options, null, "Failed to create SAML config");
  public Task<JsonElement> UpdateSamlConfigAsync(SamlConfigOptions options) => Call(HttpMethods.Patch, "/api/v1/sso/config", options, null, "Failed to update SAML config");

  // --- SCIM -------------------------------------------------------------------------------------
  public Task<JsonElement> GetScimGroupOrgRoleMappingsAsync() => Call(HttpMethod.Get, "/api/v1/scim/group-org-role-mappings", null, null, "Failed to get SCIM group mappings");

  /// <param name="mappings">Request body, see the "Update SCIM Group Org Role Mappings" API reference.</param>
  public Task<JsonElement> UpdateScimGroupOrgRoleMappingsAsync(object mappings) => Call(HttpMethod.Put, "/api/v1/scim/group-org-role-mappings", mappings, null, "Failed to update SCIM group mappings");
}
