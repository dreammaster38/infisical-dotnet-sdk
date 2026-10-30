using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>
/// Secret validation rules (constraints for secret keys / values, dynamic secret and rotation passwords). The rule
/// type is selected with one of <see cref="SecretValidationRuleTypes"/>.
/// </summary>
public class SecretValidationRulesClient
{
  private readonly ApiClient _apiClient;

  public SecretValidationRulesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Base(string type)
  {
    if (string.IsNullOrEmpty(type)) throw new InfisicalException("Type is required");
    return $"/api/v1/secret-validation-rules/{Uri.EscapeDataString(type)}";
  }

  private static string Path(string type, string ruleId)
  {
    if (string.IsNullOrEmpty(ruleId)) throw new InfisicalException("RuleId is required");
    return $"{Base(type)}/{Uri.EscapeDataString(ruleId)}";
  }

  /// <summary>Lists the rules of all types of a project.</summary>
  public async Task<SecretValidationRule[]> ListAsync(string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/secret-validation-rules", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretValidationRule>(response, "secretValidationRules");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret validation rules", e);
    }
  }

  public async Task<SecretValidationRule[]> ListByTypeAsync(string type, string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Base(type), null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretValidationRule>(response, "secretValidationRules");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret validation rules", e);
    }
  }

  public async Task<SecretValidationRule> GetAsync(string type, string ruleId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(type, ruleId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretValidationRule>(response, "secretValidationRule");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get secret validation rule", e);
    }
  }

  public async Task<SecretValidationRule> CreateAsync(SecretValidationRuleOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      if (string.IsNullOrEmpty(options.ProjectId)) throw new InfisicalException("ProjectId is required");
      if (string.IsNullOrEmpty(options.SecretPath)) throw new InfisicalException("SecretPath is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Base(options.Type), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretValidationRule>(response, "secretValidationRule");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create secret validation rule", e);
    }
  }

  public async Task<SecretValidationRule> UpdateAsync(SecretValidationRuleOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.Type, options.RuleId ?? string.Empty), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretValidationRule>(response, "secretValidationRule");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update secret validation rule", e);
    }
  }

  public async Task<SecretValidationRule> DeleteAsync(string type, string ruleId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(type, ruleId)).ConfigureAwait(false);
      return ResponseReader.Entity<SecretValidationRule>(response, "secretValidationRule");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete secret validation rule", e);
    }
  }
}

/// <summary>
/// Agent Vault: access bundles, variables, skills, proxies, sessions and members. Responses that are not mapped to a
/// model are returned as <see cref="JsonElement"/>.
/// </summary>
public class AgentVaultClient
{
  private const string BasePath = "/api/v1/agent-vault";
  private readonly ApiClient _apiClient;

  public AgentVaultClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
    AccessBundles = new AgentVaultAccessBundles(apiClient);
    Variables = new AgentVaultVariables(apiClient);
    Skills = new AgentVaultSkills(apiClient);
    Proxies = new AgentVaultProxies(apiClient);
    Sessions = new AgentVaultSessions(apiClient);
    Members = new AgentVaultMembers(apiClient);
    Settings = new AgentVaultSettings(apiClient);
    AwsConnections = new AgentVaultAwsConnections(apiClient);
  }

  public AgentVaultAccessBundles AccessBundles { get; }
  public AgentVaultVariables Variables { get; }
  public AgentVaultSkills Skills { get; }
  public AgentVaultProxies Proxies { get; }
  public AgentVaultSessions Sessions { get; }
  public AgentVaultMembers Members { get; }
  public AgentVaultSettings Settings { get; }
  public AgentVaultAwsConnections AwsConnections { get; }

  internal static async Task<JsonElement> Call(ApiClient api, HttpMethod method, string path, object? body, Dictionary<string, string>? query, string error)
  {
    try
    {
      return await api.SendRequiredAsync<JsonElement>(method, BasePath + path, body, query, method == HttpMethod.Get).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException(error, e);
    }
  }

  internal static string Id(string value, string name)
  {
    if (string.IsNullOrEmpty(value)) throw new InfisicalException($"{name} is required");
    return Uri.EscapeDataString(value);
  }

  internal static Dictionary<string, string> ListQuery(AgentVaultListOptions? options)
    => new QueryBuilder().Add("search", options?.Search!).Add("limit", options?.Limit!).Add("offset", options?.Offset!).Build();
}

public class AgentVaultAccessBundles
{
  private readonly ApiClient _api;
  public AgentVaultAccessBundles(ApiClient api) { _api = api; }

  public async Task<AgentVaultResource[]> ListAsync(AgentVaultListOptions? options = null, bool? requiresSetup = null, bool? granted = null)
  {
    var query = AgentVaultClient.ListQuery(options);
    if (requiresSetup != null) query["requiresSetup"] = requiresSetup.Value ? "true" : "false";
    if (granted != null) query["granted"] = granted.Value ? "true" : "false";
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, "/access-bundles", null, query, "Failed to list access bundles").ConfigureAwait(false);
    return ResponseReader.List<AgentVaultResource>(response, "accessBundles");
  }

  public async Task<AgentVaultResource> GetAsync(string accessBundleId)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}", null, null, "Failed to get access bundle").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "accessBundle");
  }

  public async Task<AgentVaultResource> CreateAsync(string name, string? description = null)
  {
    if (string.IsNullOrEmpty(name)) throw new InfisicalException("Name is required");
    var response = await AgentVaultClient.Call(_api, HttpMethod.Post, "/access-bundles", new { name, description }, null, "Failed to create access bundle").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "accessBundle");
  }

  public async Task<AgentVaultResource> UpdateAsync(string accessBundleId, string? name = null, string? description = null)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethods.Patch, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}", new { name, description }, null, "Failed to update access bundle").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "accessBundle");
  }

  public async Task<AgentVaultResource> DeleteAsync(string accessBundleId)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Delete, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}", null, null, "Failed to delete access bundle").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "accessBundle");
  }

  // --- services ---------------------------------------------------------------------------------

  /// <summary>
  /// Adds a service (an HTTP API or MCP server with its credential) to an access bundle. The body is an object with a
  /// <c>type</c> ("http" or "mcp"), <c>name</c>, host / server settings and a <c>credential</c>; see the API reference.
  /// </summary>
  public Task<JsonElement> CreateServiceAsync(string accessBundleId, object service)
    => AgentVaultClient.Call(_api, HttpMethod.Post, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/services", service, null, "Failed to create service");

  public Task<JsonElement> UpdateServiceAsync(string accessBundleId, string serviceId, object service)
    => AgentVaultClient.Call(_api, HttpMethods.Patch, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/services/{AgentVaultClient.Id(serviceId, "ServiceId")}", service, null, "Failed to update service");

  public Task<JsonElement> DeleteServiceAsync(string accessBundleId, string serviceId)
    => AgentVaultClient.Call(_api, HttpMethod.Delete, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/services/{AgentVaultClient.Id(serviceId, "ServiceId")}", null, null, "Failed to delete service");

  /// <param name="values">Objects like <c>new { field = "credentialValue", value = "..." }</c> (field: "credentialUsername" or "credentialValue").</param>
  public Task<JsonElement> SetPersonalValuesAsync(string accessBundleId, string serviceId, object[] values, string? credentialType = null)
    => AgentVaultClient.Call(_api, HttpMethods.Patch, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/services/{AgentVaultClient.Id(serviceId, "ServiceId")}/personal-values", new { values, credentialType }, null, "Failed to set personal values");

  public Task<JsonElement> DeletePersonalValuesAsync(string accessBundleId, string serviceId)
    => AgentVaultClient.Call(_api, HttpMethod.Delete, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/services/{AgentVaultClient.Id(serviceId, "ServiceId")}/personal-values", null, null, "Failed to delete personal values");

  // --- skills -----------------------------------------------------------------------------------

  public Task<JsonElement> ListSkillsAsync(string accessBundleId)
    => AgentVaultClient.Call(_api, HttpMethod.Get, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/skills", null, null, "Failed to list bundle skills");

  public Task<JsonElement> AddSkillAsync(string accessBundleId, string skillId)
    => AgentVaultClient.Call(_api, HttpMethod.Post, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/skills", new { skillId }, null, "Failed to add skill to bundle");

  public Task<JsonElement> RemoveSkillAsync(string accessBundleId, string skillId)
    => AgentVaultClient.Call(_api, HttpMethod.Delete, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/skills/{AgentVaultClient.Id(skillId, "SkillId")}", null, null, "Failed to remove skill from bundle");

  // --- members ----------------------------------------------------------------------------------

  public Task<JsonElement> ListMembersAsync(string accessBundleId, AgentVaultListOptions? options = null)
    => AgentVaultClient.Call(_api, HttpMethod.Get, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/members", null, AgentVaultClient.ListQuery(options), "Failed to list bundle members");

  /// <summary>Lists users, identities and groups the bundle can still be granted to.</summary>
  public Task<JsonElement> ListAvailableMembersAsync(string accessBundleId, AgentVaultListOptions? options = null)
    => AgentVaultClient.Call(_api, HttpMethod.Get, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/members/available", null, AgentVaultClient.ListQuery(options), "Failed to list available members");

  public Task<JsonElement> GrantAsync(string accessBundleId, AgentVaultMemberIds members)
    => AgentVaultClient.Call(_api, HttpMethod.Post, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/members", members, null, "Failed to grant access bundle");

  public Task<JsonElement> RevokeAsync(string accessBundleId, AgentVaultMemberIds members)
    => AgentVaultClient.Call(_api, HttpMethod.Delete, $"/access-bundles/{AgentVaultClient.Id(accessBundleId, "AccessBundleId")}/members", members, null, "Failed to revoke access bundle");
}

public class AgentVaultVariables
{
  private readonly ApiClient _api;
  public AgentVaultVariables(ApiClient api) { _api = api; }

  public async Task<AgentVaultResource[]> ListAsync(AgentVaultListOptions? options = null, string? scope = null, string[]? keys = null)
  {
    var query = AgentVaultClient.ListQuery(options);
    if (scope != null) query["scope"] = scope;
    if (keys != null) query["keys"] = string.Join(",", keys);
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, "/variables", null, query, "Failed to list variables").ConfigureAwait(false);
    return ResponseReader.List<AgentVaultResource>(response, "variables");
  }

  public async Task<AgentVaultResource> CreateAsync(AgentVaultVariableOptions options)
  {
    if (string.IsNullOrEmpty(options.Key)) throw new InfisicalException("Key is required");
    var response = await AgentVaultClient.Call(_api, HttpMethod.Post, "/variables", options, null, "Failed to create variable").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "variable");
  }

  public async Task<AgentVaultResource> UpdateAsync(AgentVaultVariableOptions options)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethods.Patch, $"/variables/{AgentVaultClient.Id(options.VariableId ?? string.Empty, "VariableId")}", options, null, "Failed to update variable").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "variable");
  }

  public async Task<AgentVaultResource> DeleteAsync(string variableId)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Delete, $"/variables/{AgentVaultClient.Id(variableId, "VariableId")}", null, null, "Failed to delete variable").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "variable");
  }

  /// <summary>Reads the value of a variable (raw response).</summary>
  public Task<JsonElement> GetValueAsync(string variableId)
    => AgentVaultClient.Call(_api, HttpMethod.Get, $"/variables/{AgentVaultClient.Id(variableId, "VariableId")}/value", null, null, "Failed to get variable value");
}

public class AgentVaultSkills
{
  private readonly ApiClient _api;
  public AgentVaultSkills(ApiClient api) { _api = api; }

  public async Task<AgentVaultResource[]> ListAsync(AgentVaultListOptions? options = null, string? name = null)
  {
    var query = AgentVaultClient.ListQuery(options);
    if (name != null) query["name"] = name;
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, "/skills", null, query, "Failed to list skills").ConfigureAwait(false);
    return ResponseReader.List<AgentVaultResource>(response, "skills");
  }

  public async Task<AgentVaultResource> GetAsync(string skillId)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, $"/skills/{AgentVaultClient.Id(skillId, "SkillId")}", null, null, "Failed to get skill").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "skill");
  }

  /// <summary>Uploads a new skill. The files must contain a SKILL.md.</summary>
  public async Task<AgentVaultResource> CreateAsync(AgentVaultSkillOptions options)
  {
    if (options.Files == null || options.Files.Length == 0) throw new InfisicalException("At least one file (SKILL.md) is required");
    var response = await AgentVaultClient.Call(_api, HttpMethod.Post, "/skills", new { files = options.Files }, null, "Failed to create skill").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "skill");
  }

  /// <summary>Uploads a new version of a skill.</summary>
  public async Task<AgentVaultResource> UpdateAsync(AgentVaultSkillOptions options)
  {
    if (options.Files == null || options.Files.Length == 0) throw new InfisicalException("At least one file (SKILL.md) is required");
    var response = await AgentVaultClient.Call(_api, HttpMethods.Patch, $"/skills/{AgentVaultClient.Id(options.SkillId ?? string.Empty, "SkillId")}", new { files = options.Files, baseVersion = options.BaseVersion }, null, "Failed to update skill").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "skill");
  }

  public async Task<AgentVaultResource> DeleteAsync(string skillId)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Delete, $"/skills/{AgentVaultClient.Id(skillId, "SkillId")}", null, null, "Failed to delete skill").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "skill");
  }

  public Task<JsonElement> ListVersionsAsync(string skillId)
    => AgentVaultClient.Call(_api, HttpMethod.Get, $"/skills/{AgentVaultClient.Id(skillId, "SkillId")}/versions", null, null, "Failed to list skill versions");

  public Task<JsonElement> GetVersionAsync(string skillId, int version)
    => AgentVaultClient.Call(_api, HttpMethod.Get, $"/skills/{AgentVaultClient.Id(skillId, "SkillId")}/versions/{version}", null, null, "Failed to get skill version");

  public Task<JsonElement> RestoreVersionAsync(string skillId, int version)
    => AgentVaultClient.Call(_api, HttpMethod.Post, $"/skills/{AgentVaultClient.Id(skillId, "SkillId")}/versions/{version}/restore", null, null, "Failed to restore skill version");
}

public class AgentVaultProxies
{
  private readonly ApiClient _api;
  public AgentVaultProxies(ApiClient api) { _api = api; }

  public async Task<AgentVaultResource[]> ListAsync(AgentVaultListOptions? options = null)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, "/proxies", null, AgentVaultClient.ListQuery(options), "Failed to list proxies").ConfigureAwait(false);
    return ResponseReader.List<AgentVaultResource>(response, "proxies");
  }

  public async Task<JsonElement> CreateAsync(AgentVaultProxyOptions options)
  {
    if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
    return await AgentVaultClient.Call(_api, HttpMethod.Post, "/proxies", options, null, "Failed to create proxy").ConfigureAwait(false);
  }

  public Task<JsonElement> UpdateAsync(AgentVaultProxyOptions options)
    => AgentVaultClient.Call(_api, HttpMethods.Patch, $"/proxies/{AgentVaultClient.Id(options.ProxyId ?? string.Empty, "ProxyId")}", options, null, "Failed to update proxy");

  public Task<JsonElement> DeleteAsync(string proxyId)
    => AgentVaultClient.Call(_api, HttpMethod.Delete, $"/proxies/{AgentVaultClient.Id(proxyId, "ProxyId")}", null, null, "Failed to delete proxy");

  public Task<JsonElement> RevokeAsync(string proxyId)
    => AgentVaultClient.Call(_api, HttpMethod.Post, $"/proxies/{AgentVaultClient.Id(proxyId, "ProxyId")}/revoke", null, null, "Failed to revoke proxy");

  /// <summary>Rotates the proxy token. The new token is only returned once.</summary>
  public Task<JsonElement> RotateTokenAsync(string proxyId)
    => AgentVaultClient.Call(_api, HttpMethod.Post, $"/proxies/{AgentVaultClient.Id(proxyId, "ProxyId")}/token-auth/proxy-token", null, null, "Failed to rotate proxy token");
}

public class AgentVaultSessions
{
  private readonly ApiClient _api;
  public AgentVaultSessions(ApiClient api) { _api = api; }

  public async Task<AgentVaultResource[]> ListAsync(ListAgentVaultSessionsOptions? options = null)
  {
    var query = new QueryBuilder()
      .Add("scope", options?.Scope!)
      .Add("status", options?.Statuses == null ? null! : string.Join(",", options.Statuses))
      .Add("search", options?.Search!)
      .Add("limit", options?.Limit!)
      .Add("offset", options?.Offset!)
      .Build();
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, "/sessions", null, query, "Failed to list sessions").ConfigureAwait(false);
    return ResponseReader.List<AgentVaultResource>(response, "sessions");
  }

  public async Task<AgentVaultResource> GetAsync(string sessionId)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, $"/sessions/{AgentVaultClient.Id(sessionId, "SessionId")}", null, null, "Failed to get session").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "session");
  }

  /// <summary>Creates a session for the given access bundles (raw response, includes the session credentials).</summary>
  public Task<JsonElement> CreateAsync(CreateAgentVaultSessionOptions options)
  {
    if (options.AccessBundles == null || options.AccessBundles.Length == 0) throw new InfisicalException("At least one access bundle is required");
    return AgentVaultClient.Call(_api, HttpMethod.Post, "/sessions", options, null, "Failed to create session");
  }

  public async Task<AgentVaultResource> UpdateLabelAsync(string sessionId, string? label)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethods.Patch, $"/sessions/{AgentVaultClient.Id(sessionId, "SessionId")}", new { label }, null, "Failed to update session").ConfigureAwait(false);
    return ResponseReader.Entity<AgentVaultResource>(response, "session");
  }

  public Task<JsonElement> RevokeAsync(string sessionId)
    => AgentVaultClient.Call(_api, HttpMethod.Post, $"/sessions/{AgentVaultClient.Id(sessionId, "SessionId")}/revoke", null, null, "Failed to revoke session");

  /// <summary>Reads the stored request logs of a session. <paramref name="query"/> holds the filters of the API reference.</summary>
  public Task<JsonElement> ListLogsAsync(string sessionId, IDictionary<string, string>? query = null)
    => AgentVaultClient.Call(_api, HttpMethod.Get, $"/sessions/{AgentVaultClient.Id(sessionId, "SessionId")}/logs", null, query == null ? null : new Dictionary<string, string>(query), "Failed to list session logs");

  /// <summary>Reads the newest log entries of a session.</summary>
  public Task<JsonElement> TailLogsAsync(string sessionId, IDictionary<string, string>? query = null)
    => AgentVaultClient.Call(_api, HttpMethod.Get, $"/sessions/{AgentVaultClient.Id(sessionId, "SessionId")}/logs/tail", null, query == null ? null : new Dictionary<string, string>(query), "Failed to tail session logs");
}

/// <summary>Members of the Agent Vault product (users, machine identities, groups) and their role.</summary>
public class AgentVaultMembers
{
  private readonly ApiClient _api;
  public AgentVaultMembers(ApiClient api) { _api = api; }

  public Task<JsonElement> ListAsync(AgentVaultListOptions? options = null, string? actorType = null)
  {
    var query = AgentVaultClient.ListQuery(options);
    if (actorType != null) query["actorType"] = actorType;
    return AgentVaultClient.Call(_api, HttpMethod.Get, "/members", null, query, "Failed to list members");
  }

  public Task<JsonElement> ListAvailableAsync(AgentVaultListOptions? options = null, string? actorType = null)
  {
    var query = AgentVaultClient.ListQuery(options);
    if (actorType != null) query["actorType"] = actorType;
    return AgentVaultClient.Call(_api, HttpMethod.Get, "/members/available", null, query, "Failed to list available members");
  }

  public Task<JsonElement> AddAsync(AgentVaultProductMembers members)
    => AgentVaultClient.Call(_api, HttpMethod.Post, "/members", members, null, "Failed to add members");

  public Task<JsonElement> UpdateRoleAsync(AgentVaultProductMembers members)
    => AgentVaultClient.Call(_api, HttpMethods.Patch, "/members", members, null, "Failed to update member roles");

  public Task<JsonElement> RevokeAsync(AgentVaultProductMembers members)
    => AgentVaultClient.Call(_api, HttpMethod.Delete, "/members", members, null, "Failed to revoke members");
}

public class AgentVaultSettings
{
  private readonly ApiClient _api;
  public AgentVaultSettings(ApiClient api) { _api = api; }

  public Task<JsonElement> GetSessionLogSettingsAsync()
    => AgentVaultClient.Call(_api, HttpMethod.Get, "/settings/session-logs", null, null, "Failed to get session log settings");

  /// <param name="settings">Settings object, see the "Update Session Log Settings" API reference.</param>
  public Task<JsonElement> UpdateSessionLogSettingsAsync(object settings)
    => AgentVaultClient.Call(_api, HttpMethods.Patch, "/settings/session-logs", settings, null, "Failed to update session log settings");
}

/// <summary>AWS connections that only Agent Vault can use.</summary>
public class AgentVaultAwsConnections
{
  private readonly ApiClient _api;
  public AgentVaultAwsConnections(ApiClient api) { _api = api; }

  public async Task<AppConnectionInfo[]> ListAsync()
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, "/app-connections/aws", null, null, "Failed to list AWS connections").ConfigureAwait(false);
    return ResponseReader.List<AppConnectionInfo>(response, "appConnections");
  }

  public async Task<AppConnectionInfo> GetAsync(string connectionId)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Get, $"/app-connections/aws/{AgentVaultClient.Id(connectionId, "ConnectionId")}", null, null, "Failed to get AWS connection").ConfigureAwait(false);
    return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
  }

  /// <summary>Creates a connection. <c>App</c> and <c>ProjectId</c> of the options are ignored.</summary>
  public async Task<AppConnectionInfo> CreateAsync(CreateAppConnectionOptions options)
  {
    if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
    if (string.IsNullOrEmpty(options.Method)) throw new InfisicalException("Method is required");
    var body = new CreateAppConnectionOptions
    {
      Name = options.Name,
      Description = options.Description,
      Method = options.Method,
      Credentials = options.Credentials,
      ProjectId = null,
      IsPlatformManagedCredentials = options.IsPlatformManagedCredentials,
      GatewayId = options.GatewayId,
      GatewayPoolId = options.GatewayPoolId
    };
    var response = await AgentVaultClient.Call(_api, HttpMethod.Post, "/app-connections/aws", body, null, "Failed to create AWS connection").ConfigureAwait(false);
    return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
  }

  public async Task<AppConnectionInfo> UpdateAsync(UpdateAppConnectionOptions options)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethods.Patch, $"/app-connections/aws/{AgentVaultClient.Id(options.ConnectionId, "ConnectionId")}", options, null, "Failed to update AWS connection").ConfigureAwait(false);
    return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
  }

  public async Task<AppConnectionInfo> DeleteAsync(string connectionId)
  {
    var response = await AgentVaultClient.Call(_api, HttpMethod.Delete, $"/app-connections/aws/{AgentVaultClient.Id(connectionId, "ConnectionId")}", null, null, "Failed to delete AWS connection").ConfigureAwait(false);
    return ResponseReader.Entity<AppConnectionInfo>(response, "appConnection");
  }
}
