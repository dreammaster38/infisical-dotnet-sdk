using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

public class DynamicSecretsClient
{
  private readonly ApiClient _apiClient;

  public DynamicSecretsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static Dictionary<string, string> Query(DynamicSecretLocation location)
  {
    location.Validate();
    return new QueryBuilder()
      .Add("projectSlug", location.ProjectSlug)
      .Add("environmentSlug", location.EnvironmentSlug)
      .Add("path", location.Path)
      .Build();
  }

  public async Task<DynamicSecret> CreateAsync(CreateDynamicSecretOptions options)
  {
    try
    {
      options.Validate();
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v1/dynamic-secrets", options).ConfigureAwait(false);
      return ResponseReader.Entity<DynamicSecret>(response, "dynamicSecret");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create dynamic secret", e);
    }
  }

  public async Task<DynamicSecret> GetAsync(string name, DynamicSecretLocation location)
  {
    try
    {
      if (string.IsNullOrEmpty(name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/dynamic-secrets/{Uri.EscapeDataString(name)}", null, Query(location), true).ConfigureAwait(false);
      return ResponseReader.Entity<DynamicSecret>(response, "dynamicSecret");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get dynamic secret", e);
    }
  }

  public async Task<DynamicSecret[]> ListAsync(DynamicSecretLocation location)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/dynamic-secrets", null, Query(location), true).ConfigureAwait(false);
      return ResponseReader.List<DynamicSecret>(response, "dynamicSecrets");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list dynamic secrets", e);
    }
  }

  public async Task<DynamicSecret> UpdateAsync(UpdateDynamicSecretOptions options)
  {
    try
    {
      options.Validate();
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v1/dynamic-secrets/{Uri.EscapeDataString(options.Name)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<DynamicSecret>(response, "dynamicSecret");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update dynamic secret", e);
    }
  }

  public async Task<DynamicSecret> DeleteAsync(DeleteDynamicSecretOptions options)
  {
    try
    {
      options.Validate();
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v1/dynamic-secrets/{Uri.EscapeDataString(options.Name)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<DynamicSecret>(response, "dynamicSecret");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete dynamic secret", e);
    }
  }

  /// <summary>Lists the leases of a dynamic secret.</summary>
  public async Task<DynamicSecretLease[]> ListLeasesAsync(string name, DynamicSecretLocation location)
  {
    try
    {
      if (string.IsNullOrEmpty(name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/dynamic-secrets/{Uri.EscapeDataString(name)}/leases", null, Query(location), true).ConfigureAwait(false);
      return ResponseReader.List<DynamicSecretLease>(response, "leases");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list dynamic secret leases", e);
    }
  }

  /// <summary>Creates a lease, i.e. generates a set of temporary credentials.</summary>
  public async Task<DynamicSecretLeaseResult> CreateLeaseAsync(CreateDynamicSecretLeaseOptions options)
  {
    try
    {
      options.Validate();
      if (string.IsNullOrEmpty(options.DynamicSecretName)) throw new InfisicalException("DynamicSecretName is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v1/dynamic-secrets/leases", options).ConfigureAwait(false);

      var lease = ResponseReader.Entity<DynamicSecretLease>(response, "lease");
      var secret = response.TryGetProperty("dynamicSecret", out var ds) ? ResponseReader.Deserialize<DynamicSecret>(ds) ?? new DynamicSecret() : new DynamicSecret();
      var data = response.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Object
        ? ResponseReader.Deserialize<Dictionary<string, JsonElement>>(d) ?? new Dictionary<string, JsonElement>()
        : new Dictionary<string, JsonElement>();

      return new DynamicSecretLeaseResult(lease, secret, data);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create dynamic secret lease", e);
    }
  }

  public async Task<DynamicSecretLease> GetLeaseAsync(string leaseId, DynamicSecretLocation location)
  {
    try
    {
      if (string.IsNullOrEmpty(leaseId)) throw new InfisicalException("LeaseId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/dynamic-secrets/leases/{Uri.EscapeDataString(leaseId)}", null, Query(location), true).ConfigureAwait(false);
      return ResponseReader.Entity<DynamicSecretLease>(response, "lease");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get dynamic secret lease", e);
    }
  }

  public async Task<DynamicSecretLease> RenewLeaseAsync(RenewDynamicSecretLeaseOptions options)
  {
    try
    {
      options.Validate();
      if (string.IsNullOrEmpty(options.LeaseId)) throw new InfisicalException("LeaseId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"/api/v1/dynamic-secrets/leases/{Uri.EscapeDataString(options.LeaseId)}/renew", options).ConfigureAwait(false);
      return ResponseReader.Entity<DynamicSecretLease>(response, "lease");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to renew dynamic secret lease", e);
    }
  }

  public async Task<DynamicSecretLease> RevokeLeaseAsync(DeleteDynamicSecretLeaseOptions options)
  {
    try
    {
      options.Validate();
      if (string.IsNullOrEmpty(options.LeaseId)) throw new InfisicalException("LeaseId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v1/dynamic-secrets/leases/{Uri.EscapeDataString(options.LeaseId)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<DynamicSecretLease>(response, "lease");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to revoke dynamic secret lease", e);
    }
  }
}

public class GroupsClient
{
  private readonly ApiClient _apiClient;

  public GroupsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string groupId)
  {
    if (string.IsNullOrEmpty(groupId)) throw new InfisicalException("GroupId is required");
    return $"/api/v1/groups/{Uri.EscapeDataString(groupId)}";
  }

  public async Task<Group> CreateAsync(CreateGroupOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v1/groups", options).ConfigureAwait(false);
      return ResponseReader.Entity<Group>(response, "group");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create group", e);
    }
  }

  public async Task<Group[]> ListAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/groups", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<Group>(response, "groups");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list groups", e);
    }
  }

  public async Task<Group> GetAsync(string groupId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(groupId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<Group>(response, "group");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get group", e);
    }
  }

  public async Task<Group> UpdateAsync(UpdateGroupOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.GroupId), options).ConfigureAwait(false);
      return ResponseReader.Entity<Group>(response, "group");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update group", e);
    }
  }

  public async Task<Group> DeleteAsync(string groupId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(groupId)).ConfigureAwait(false);
      return ResponseReader.Entity<Group>(response, "group");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete group", e);
    }
  }

  public async Task AddUserAsync(string groupId, string username)
  {
    try
    {
      if (string.IsNullOrEmpty(username)) throw new InfisicalException("Username is required");
      await _apiClient.SendAsync<JsonElement>(HttpMethod.Post, $"{Path(groupId)}/users/{Uri.EscapeDataString(username)}").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to add user to group", e);
    }
  }

  public async Task RemoveUserAsync(string groupId, string username)
  {
    try
    {
      if (string.IsNullOrEmpty(username)) throw new InfisicalException("Username is required");
      await _apiClient.SendAsync<JsonElement>(HttpMethod.Delete, $"{Path(groupId)}/users/{Uri.EscapeDataString(username)}").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to remove user from group", e);
    }
  }

  public async Task AddMachineIdentityAsync(string groupId, string machineIdentityId)
  {
    try
    {
      if (string.IsNullOrEmpty(machineIdentityId)) throw new InfisicalException("MachineIdentityId is required");
      await _apiClient.SendAsync<JsonElement>(HttpMethod.Post, $"{Path(groupId)}/machine-identities/{Uri.EscapeDataString(machineIdentityId)}").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to add machine identity to group", e);
    }
  }

  public async Task RemoveMachineIdentityAsync(string groupId, string machineIdentityId)
  {
    try
    {
      if (string.IsNullOrEmpty(machineIdentityId)) throw new InfisicalException("MachineIdentityId is required");
      await _apiClient.SendAsync<JsonElement>(HttpMethod.Delete, $"{Path(groupId)}/machine-identities/{Uri.EscapeDataString(machineIdentityId)}").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to remove machine identity from group", e);
    }
  }

  private async Task<JsonElement> ListSubResourceAsync(string groupId, string resource, int? offset, int? limit, string? search)
  {
    var query = new QueryBuilder().Add("offset", offset!).Add("limit", limit!).Add("search", search!).Build();
    return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Path(groupId)}/{resource}", null, query, true).ConfigureAwait(false);
  }

  /// <summary>Lists the users of a group (raw response: users and totalCount).</summary>
  public async Task<JsonElement> ListUsersAsync(string groupId, int? offset = null, int? limit = null, string? search = null)
  {
    try { return await ListSubResourceAsync(groupId, "users", offset, limit, search).ConfigureAwait(false); }
    catch (Exception e) { throw new InfisicalException("Failed to list group users", e); }
  }

  /// <summary>Lists the machine identities of a group (raw response).</summary>
  public async Task<JsonElement> ListMachineIdentitiesAsync(string groupId, int? offset = null, int? limit = null, string? search = null)
  {
    try { return await ListSubResourceAsync(groupId, "machine-identities", offset, limit, search).ConfigureAwait(false); }
    catch (Exception e) { throw new InfisicalException("Failed to list group machine identities", e); }
  }

  /// <summary>Lists users and machine identities of a group (raw response).</summary>
  public async Task<JsonElement> ListMembersAsync(string groupId, int? offset = null, int? limit = null, string? search = null)
  {
    try { return await ListSubResourceAsync(groupId, "members", offset, limit, search).ConfigureAwait(false); }
    catch (Exception e) { throw new InfisicalException("Failed to list group members", e); }
  }

  /// <summary>Lists the projects a group is assigned to (raw response).</summary>
  public async Task<JsonElement> ListProjectsAsync(string groupId, int? offset = null, int? limit = null, string? search = null)
  {
    try { return await ListSubResourceAsync(groupId, "projects", offset, limit, search).ConfigureAwait(false); }
    catch (Exception e) { throw new InfisicalException("Failed to list group projects", e); }
  }
}

/// <summary>Cryptographic keys of the Infisical KMS.</summary>
public class KmsClient
{
  private readonly ApiClient _apiClient;

  public KmsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string keyId)
  {
    if (string.IsNullOrEmpty(keyId)) throw new InfisicalException("KeyId is required");
    return $"/api/v1/kms/keys/{Uri.EscapeDataString(keyId)}";
  }

  public async Task<KmsKey> CreateKeyAsync(CreateKmsKeyOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.ProjectId)) throw new InfisicalException("ProjectId is required");
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v1/kms/keys", options).ConfigureAwait(false);
      return ResponseReader.Entity<KmsKey>(response, "key");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create KMS key", e);
    }
  }

  public async Task<KmsKeyList> ListKeysAsync(ListKmsKeysOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.ProjectId)) throw new InfisicalException("ProjectId is required");
      var query = new QueryBuilder()
        .Add("projectId", options.ProjectId)
        .Add("offset", options.Offset!)
        .Add("limit", options.Limit!)
        .Add("orderBy", options.OrderBy!)
        .Add("orderDirection", options.OrderDirection!)
        .Add("search", options.Search!)
        .Build();

      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/kms/keys", null, query, true).ConfigureAwait(false);
      int? total = response.ValueKind == JsonValueKind.Object && response.TryGetProperty("totalCount", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : (int?)null;
      return new KmsKeyList(ResponseReader.List<KmsKey>(response, "keys"), total);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list KMS keys", e);
    }
  }

  public async Task<KmsKey> GetKeyAsync(string keyId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(keyId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<KmsKey>(response, "key");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get KMS key", e);
    }
  }

  public async Task<KmsKey> GetKeyByNameAsync(string projectId, string keyName)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      if (string.IsNullOrEmpty(keyName)) throw new InfisicalException("KeyName is required");
      var query = new QueryBuilder().Add("projectId", projectId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/kms/keys/key-name/{Uri.EscapeDataString(keyName)}", null, query, true).ConfigureAwait(false);
      return ResponseReader.Entity<KmsKey>(response, "key");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get KMS key by name", e);
    }
  }

  public async Task<KmsKey> UpdateKeyAsync(UpdateKmsKeyOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.KeyId), options).ConfigureAwait(false);
      return ResponseReader.Entity<KmsKey>(response, "key");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update KMS key", e);
    }
  }

  public async Task<KmsKey> DeleteKeyAsync(string keyId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(keyId)).ConfigureAwait(false);
      return ResponseReader.Entity<KmsKey>(response, "key");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete KMS key", e);
    }
  }

  public async Task<JsonElement> RotateKeyAsync(string keyId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(keyId) + "/rotate").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to rotate KMS key", e);
    }
  }

  /// <summary>Encrypts data with the key. <paramref name="plaintext"/> has to be base64 encoded; the returned ciphertext is base64 as well.</summary>
  public async Task<string> EncryptAsync(string keyId, string plaintext)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(keyId) + "/encrypt", new { plaintext }).ConfigureAwait(false);
      return response.GetProperty("ciphertext").GetString() ?? string.Empty;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to encrypt data", e);
    }
  }

  /// <summary>Decrypts a base64 ciphertext. The returned plaintext is base64 encoded.</summary>
  public async Task<string> DecryptAsync(string keyId, string ciphertext)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(keyId) + "/decrypt", new { ciphertext }).ConfigureAwait(false);
      return response.GetProperty("plaintext").GetString() ?? string.Empty;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to decrypt data", e);
    }
  }

  public async Task<string> GetPublicKeyAsync(string keyId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(keyId) + "/public-key", null, null, true).ConfigureAwait(false);
      return response.GetProperty("publicKey").GetString() ?? string.Empty;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get public key", e);
    }
  }

  public async Task<string[]> GetSigningAlgorithmsAsync(string keyId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(keyId) + "/signing-algorithms", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<string>(response, "signingAlgorithms");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get signing algorithms", e);
    }
  }

  /// <summary>Signs base64 encoded data (or a digest if <paramref name="isDigest"/> is set).</summary>
  public async Task<KmsSignResult> SignAsync(string keyId, string data, string signingAlgorithm, bool isDigest = false)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(keyId) + "/sign", new { data, signingAlgorithm, isDigest }).ConfigureAwait(false);
      return ResponseReader.Deserialize<KmsSignResult>(response) ?? throw new InfisicalException("Failed to deserialize response content");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to sign data", e);
    }
  }

  public async Task<KmsVerifyResult> VerifyAsync(string keyId, string data, string signature, string signingAlgorithm, bool isDigest = false)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(keyId) + "/verify", new { data, signature, signingAlgorithm, isDigest }).ConfigureAwait(false);
      return ResponseReader.Deserialize<KmsVerifyResult>(response) ?? throw new InfisicalException("Failed to deserialize response content");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to verify signature", e);
    }
  }
}

public class SharedSecretsClient
{
  private readonly ApiClient _apiClient;

  public SharedSecretsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  /// <summary>Creates a shared secret and returns its ID.</summary>
  public async Task<string> CreateAsync(CreateSharedSecretOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v1/shared-secrets", options).ConfigureAwait(false);
      return response.GetProperty("id").GetString() ?? string.Empty;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create shared secret", e);
    }
  }

  public async Task<JsonElement> ListAsync(int offset = 0, int limit = 25)
  {
    try
    {
      var query = new QueryBuilder().Add("offset", offset).Add("limit", limit).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/shared-secrets", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list shared secrets", e);
    }
  }

  public async Task<JsonElement> GetAsync(string id)
  {
    try
    {
      if (string.IsNullOrEmpty(id)) throw new InfisicalException("Id is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/shared-secrets/{Uri.EscapeDataString(id)}", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get shared secret", e);
    }
  }

  /// <summary>Reveals the value of a shared secret (counts as a view).</summary>
  public async Task<JsonElement> AccessAsync(string id, string? password = null)
  {
    try
    {
      if (string.IsNullOrEmpty(id)) throw new InfisicalException("Id is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"/api/v1/shared-secrets/{Uri.EscapeDataString(id)}/access", new { password }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to access shared secret", e);
    }
  }

  public async Task DeleteAsync(string id)
  {
    try
    {
      if (string.IsNullOrEmpty(id)) throw new InfisicalException("Id is required");
      await _apiClient.SendAsync<JsonElement>(HttpMethod.Delete, $"/api/v1/shared-secrets/{Uri.EscapeDataString(id)}").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete shared secret", e);
    }
  }
}

/// <summary>Project roles and memberships of machine identities and groups.</summary>
public class ProjectAccessClient
{
  private readonly ApiClient _apiClient;

  public ProjectAccessClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Project(string projectId)
  {
    if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
    return $"/api/v1/projects/{Uri.EscapeDataString(projectId)}";
  }

  // --- roles ------------------------------------------------------------------------------

  public async Task<ProjectRole> CreateRoleAsync(ProjectRoleOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Project(options.ProjectId) + "/roles", options).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create project role", e);
    }
  }

  public async Task<ProjectRole[]> ListRolesAsync(string projectId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Project(projectId) + "/roles", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<ProjectRole>(response, "roles");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list project roles", e);
    }
  }

  public async Task<ProjectRole> GetRoleBySlugAsync(string projectId, string roleSlug)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Project(projectId)}/roles/slug/{Uri.EscapeDataString(roleSlug)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get project role", e);
    }
  }

  public async Task<ProjectRole> GetRoleAsync(string roleId)
  {
    try
    {
      if (string.IsNullOrEmpty(roleId)) throw new InfisicalException("RoleId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/projects/roles/{Uri.EscapeDataString(roleId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get project role", e);
    }
  }

  public async Task<ProjectRole> UpdateRoleAsync(ProjectRoleOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.RoleId)) throw new InfisicalException("RoleId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"{Project(options.ProjectId)}/roles/{Uri.EscapeDataString(options.RoleId!)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update project role", e);
    }
  }

  public async Task<ProjectRole> DeleteRoleAsync(string projectId, string roleId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"{Project(projectId)}/roles/{Uri.EscapeDataString(roleId)}").ConfigureAwait(false);
      return ResponseReader.Entity<ProjectRole>(response, "role");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete project role", e);
    }
  }

  // --- machine identity memberships -----------------------------------------------------------

  public Task<JsonElement> AddIdentityAsync(string projectId, string identityId, params ProjectRoleAssignment[] roles)
    => MembershipCall(HttpMethod.Post, "identities", projectId, identityId, new { roles }, "add identity to project");

  public Task<JsonElement> UpdateIdentityRolesAsync(string projectId, string identityId, params ProjectRoleAssignment[] roles)
    => MembershipCall(HttpMethods.Patch, "identities", projectId, identityId, new { roles }, "update identity project roles");

  public Task<JsonElement> RemoveIdentityAsync(string projectId, string identityId)
    => MembershipCall(HttpMethod.Delete, "identities", projectId, identityId, null, "remove identity from project");

  public Task<JsonElement> GetIdentityMembershipAsync(string projectId, string identityId)
    => MembershipCall(HttpMethod.Get, "identities", projectId, identityId, null, "get identity project membership");

  public async Task<JsonElement> ListIdentityMembershipsAsync(string projectId, int? offset = null, int? limit = null, string? identityName = null)
  {
    try
    {
      var query = new QueryBuilder().Add("offset", offset!).Add("limit", limit!).Add("identityName", identityName!).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Project(projectId) + "/memberships/identities", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list project identity memberships", e);
    }
  }

  // --- group memberships --------------------------------------------------------------------

  public Task<JsonElement> AddGroupAsync(string projectId, string groupId, params ProjectRoleAssignment[] roles)
    => MembershipCall(HttpMethod.Post, "groups", projectId, groupId, new { roles }, "add group to project");

  public Task<JsonElement> UpdateGroupRolesAsync(string projectId, string groupId, params ProjectRoleAssignment[] roles)
    => MembershipCall(HttpMethods.Patch, "groups", projectId, groupId, new { roles }, "update group project roles");

  public Task<JsonElement> RemoveGroupAsync(string projectId, string groupId)
    => MembershipCall(HttpMethod.Delete, "groups", projectId, groupId, null, "remove group from project");

  public Task<JsonElement> GetGroupMembershipAsync(string projectId, string groupId)
    => MembershipCall(HttpMethod.Get, "groups", projectId, groupId, null, "get group project membership");

  public async Task<JsonElement> ListGroupMembershipsAsync(string projectId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Project(projectId) + "/memberships/groups", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list project group memberships", e);
    }
  }

  // --- user memberships ---------------------------------------------------------------------

  public async Task<JsonElement> ListUsersAsync(string projectId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Project(projectId) + "/memberships", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list project users", e);
    }
  }

  /// <summary>Invites existing organization users (by email and / or username) to a project.</summary>
  public async Task<JsonElement> AddUsersAsync(string projectId, string[]? emails, string[]? usernames, string[] roleSlugs)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Project(projectId) + "/memberships", new { emails, usernames, roleSlugs }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to add users to project", e);
    }
  }

  public async Task<JsonElement> UpdateUserRolesAsync(string projectId, string membershipId, params ProjectRoleAssignment[] roles)
  {
    try
    {
      if (string.IsNullOrEmpty(membershipId)) throw new InfisicalException("MembershipId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"{Project(projectId)}/memberships/{Uri.EscapeDataString(membershipId)}", new { roles }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update project user roles", e);
    }
  }

  public async Task<JsonElement> RemoveUsersAsync(string projectId, string[]? emails, string[]? usernames)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Project(projectId) + "/memberships", new { emails, usernames }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to remove users from project", e);
    }
  }

  private async Task<JsonElement> MembershipCall(HttpMethod method, string kind, string projectId, string id, object? body, string action)
  {
    try
    {
      if (string.IsNullOrEmpty(id)) throw new InfisicalException("Id is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(method, $"{Project(projectId)}/memberships/{kind}/{Uri.EscapeDataString(id)}", body, null, method == HttpMethod.Get).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException($"Failed to {action}", e);
    }
  }
}

/// <summary>
/// Low level access to any endpoint of the Infisical API, including the ones that are not covered by the typed
/// clients (app connections, secret syncs, secret rotations, PKI, audit logs, ...). The authentication of the
/// <see cref="InfisicalClient"/> is applied automatically.
/// </summary>
public class RawClient
{
  private readonly ApiClient _apiClient;

  public RawClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  /// <param name="method">HTTP method, e.g. "GET", "POST", "PATCH", "PUT", "DELETE".</param>
  /// <param name="path">API path, e.g. "/api/v1/organization/audit-logs".</param>
  /// <param name="body">Optional request body (serialized as JSON, null properties are omitted).</param>
  /// <param name="query">Optional query parameters.</param>
  /// <returns>The parsed JSON response, or null if the response had no content.</returns>
  public async Task<JsonElement?> RequestAsync(string method, string path, object? body = null, IDictionary<string, string>? query = null)
  {
    try
    {
      if (string.IsNullOrEmpty(method)) throw new InfisicalException("Method is required");
      if (string.IsNullOrEmpty(path)) throw new InfisicalException("Path is required");
      var httpMethod = new HttpMethod(method.ToUpperInvariant());
      var dict = query == null ? null : new Dictionary<string, string>(query);
      return await _apiClient.SendAsync<JsonElement?>(httpMethod, path, body, dict, httpMethod == HttpMethod.Get).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException($"Failed to call {method} {path}", e);
    }
  }

  public Task<JsonElement?> GetAsync(string path, IDictionary<string, string>? query = null) => RequestAsync("GET", path, null, query);
  public Task<JsonElement?> PostAsync(string path, object? body = null) => RequestAsync("POST", path, body);
  public Task<JsonElement?> PatchAsync(string path, object? body = null) => RequestAsync("PATCH", path, body);
  public Task<JsonElement?> PutAsync(string path, object? body = null) => RequestAsync("PUT", path, body);
  public Task<JsonElement?> DeleteAsync(string path, object? body = null) => RequestAsync("DELETE", path, body);
}
