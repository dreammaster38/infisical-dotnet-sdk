using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

public class IdentitiesClient
{
  private readonly ApiClient _apiClient;
  private readonly IdentityAuthManagementClient _auth;

  public IdentitiesClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
    _auth = new IdentityAuthManagementClient(apiClient);
  }

  /// <summary>Manage the auth methods (Universal Auth, Token Auth, AWS, ...) attached to an identity.</summary>
  public IdentityAuthManagementClient Auth()
  {
    return _auth;
  }

  public async Task<Identity> CreateAsync(CreateIdentityOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v1/identities", options).ConfigureAwait(false);
      return ResponseReader.Entity<Identity>(response, "identity");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create identity", e);
    }
  }

  public async Task<Identity> GetAsync(string identityId)
  {
    try
    {
      if (string.IsNullOrEmpty(identityId)) throw new InfisicalException("IdentityId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/identities/{Uri.EscapeDataString(identityId)}", null, null, true).ConfigureAwait(false);
      return ReadMembershipIdentity(response);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get identity", e);
    }
  }

  /// <summary>Returns the identity the current access token belongs to.</summary>
  public async Task<Identity> GetCurrentAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/identities/details", null, null, true).ConfigureAwait(false);
      return ReadMembershipIdentity(response);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get identity details", e);
    }
  }

  public async Task<JsonElement[]> ListAsync(string organizationId)
  {
    try
    {
      if (string.IsNullOrEmpty(organizationId)) throw new InfisicalException("OrganizationId is required");
      var query = new QueryBuilder().Add("orgId", organizationId).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/identities", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<JsonElement>(response, "identities");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list identities", e);
    }
  }

  public async Task<IdentitySearchResult> SearchAsync(SearchIdentitiesOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v2/identities/search", options).ConfigureAwait(false);
      var items = ResponseReader.List<JsonElement>(response, "identities");
      int? total = null;
      if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("totalCount", out var count) && count.ValueKind == JsonValueKind.Number)
      {
        total = count.GetInt32();
      }
      return new IdentitySearchResult(items, total);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to search identities", e);
    }
  }

  public async Task<Identity> UpdateAsync(UpdateIdentityOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v1/identities/{Uri.EscapeDataString(options.IdentityId)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<Identity>(response, "identity");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update identity", e);
    }
  }

  public async Task<Identity> DeleteAsync(string identityId)
  {
    try
    {
      if (string.IsNullOrEmpty(identityId)) throw new InfisicalException("IdentityId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v1/identities/{Uri.EscapeDataString(identityId)}").ConfigureAwait(false);
      return ResponseReader.Entity<Identity>(response, "identity");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete identity", e);
    }
  }

  // GET responses wrap the identity into a membership: { identity: { identity: {...}, role: ... } }
  private static Identity ReadMembershipIdentity(JsonElement response)
  {
    var outer = ResponseReader.Unwrap(response, "identity");
    var inner = ResponseReader.Unwrap(outer, "identity");
    return ResponseReader.Deserialize<Identity>(inner) ?? throw new InfisicalException("Failed to deserialize response content");
  }
}

/// <summary>Groups the management clients of all identity auth methods.</summary>
public class IdentityAuthManagementClient
{
  private readonly UniversalAuthManagement _universal;
  private readonly TokenAuthManagement _token;
  private readonly IdentityAuthMethodManagement<AwsAuthConfig> _aws;
  private readonly IdentityAuthMethodManagement<AzureAuthConfig> _azure;
  private readonly IdentityAuthMethodManagement<GcpAuthConfig> _gcp;
  private readonly IdentityAuthMethodManagement<KubernetesAuthConfig> _kubernetes;
  private readonly IdentityAuthMethodManagement<OidcAuthConfig> _oidc;
  private readonly IdentityAuthMethodManagement<JwtAuthConfig> _jwt;
  private readonly IdentityAuthMethodManagement<OciAuthConfig> _oci;
  private readonly IdentityAuthMethodManagement<AlicloudAuthConfig> _alicloud;
  private readonly IdentityAuthMethodManagement<TlsCertAuthConfig> _tlsCert;
  private readonly IdentityAuthMethodManagement<LdapAuthConfig> _ldap;

  public IdentityAuthManagementClient(ApiClient apiClient)
  {
    _universal = new UniversalAuthManagement(apiClient);
    _token = new TokenAuthManagement(apiClient);
    _aws = new IdentityAuthMethodManagement<AwsAuthConfig>(apiClient, "aws-auth");
    _azure = new IdentityAuthMethodManagement<AzureAuthConfig>(apiClient, "azure-auth");
    _gcp = new IdentityAuthMethodManagement<GcpAuthConfig>(apiClient, "gcp-auth");
    _kubernetes = new IdentityAuthMethodManagement<KubernetesAuthConfig>(apiClient, "kubernetes-auth");
    _oidc = new IdentityAuthMethodManagement<OidcAuthConfig>(apiClient, "oidc-auth");
    _jwt = new IdentityAuthMethodManagement<JwtAuthConfig>(apiClient, "jwt-auth");
    _oci = new IdentityAuthMethodManagement<OciAuthConfig>(apiClient, "oci-auth");
    _alicloud = new IdentityAuthMethodManagement<AlicloudAuthConfig>(apiClient, "alicloud-auth");
    _tlsCert = new IdentityAuthMethodManagement<TlsCertAuthConfig>(apiClient, "tls-cert-auth");
    _ldap = new IdentityAuthMethodManagement<LdapAuthConfig>(apiClient, "ldap-auth");
  }

  public UniversalAuthManagement UniversalAuth() => _universal;
  public TokenAuthManagement TokenAuth() => _token;
  public IdentityAuthMethodManagement<AwsAuthConfig> AwsAuth() => _aws;
  public IdentityAuthMethodManagement<AzureAuthConfig> AzureAuth() => _azure;
  public IdentityAuthMethodManagement<GcpAuthConfig> GcpAuth() => _gcp;
  public IdentityAuthMethodManagement<KubernetesAuthConfig> KubernetesAuth() => _kubernetes;
  public IdentityAuthMethodManagement<OidcAuthConfig> OidcAuth() => _oidc;
  public IdentityAuthMethodManagement<JwtAuthConfig> JwtAuth() => _jwt;
  public IdentityAuthMethodManagement<OciAuthConfig> OciAuth() => _oci;
  public IdentityAuthMethodManagement<AlicloudAuthConfig> AlicloudAuth() => _alicloud;
  public IdentityAuthMethodManagement<TlsCertAuthConfig> TlsCertAuth() => _tlsCert;
  public IdentityAuthMethodManagement<LdapAuthConfig> LdapAuth() => _ldap;
}

/// <summary>Attach / read / update / remove the configuration of one auth method of an identity.</summary>
public class IdentityAuthMethodManagement<TConfig> where TConfig : IdentityAuthConfig
{
  protected readonly ApiClient ApiClient;
  private readonly string _method;

  public IdentityAuthMethodManagement(ApiClient apiClient, string method)
  {
    ApiClient = apiClient;
    _method = method;
  }

  protected string Path(string identityId)
  {
    if (string.IsNullOrEmpty(identityId)) throw new InfisicalException("IdentityId is required");
    return $"/api/v1/auth/{_method}/identities/{Uri.EscapeDataString(identityId)}";
  }

  public async Task<IdentityAuthConfigInfo> AttachAsync(string identityId, TConfig config)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(identityId), config).ConfigureAwait(false);
      return ReadConfig(response);
    }
    catch (Exception e)
    {
      throw new InfisicalException($"Failed to attach {_method} to identity", e);
    }
  }

  public async Task<IdentityAuthConfigInfo> GetAsync(string identityId)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(identityId), null, null, true).ConfigureAwait(false);
      return ReadConfig(response);
    }
    catch (Exception e)
    {
      throw new InfisicalException($"Failed to get {_method} config of identity", e);
    }
  }

  public async Task<IdentityAuthConfigInfo> UpdateAsync(string identityId, TConfig config)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(identityId), config).ConfigureAwait(false);
      return ReadConfig(response);
    }
    catch (Exception e)
    {
      throw new InfisicalException($"Failed to update {_method} config of identity", e);
    }
  }

  /// <summary>Removes the auth method from the identity.</summary>
  public async Task<IdentityAuthConfigInfo> RevokeAsync(string identityId)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(identityId)).ConfigureAwait(false);
      return ReadConfig(response);
    }
    catch (Exception e)
    {
      throw new InfisicalException($"Failed to revoke {_method} from identity", e);
    }
  }

  // The config is wrapped in a method specific key (e.g. "identityUniversalAuth"), take the first object property.
  private static IdentityAuthConfigInfo ReadConfig(JsonElement response)
  {
    var element = response;
    if (response.ValueKind == JsonValueKind.Object)
    {
      foreach (var prop in response.EnumerateObject())
      {
        if (prop.Value.ValueKind == JsonValueKind.Object)
        {
          element = prop.Value;
          break;
        }
      }
    }
    return ResponseReader.Deserialize<IdentityAuthConfigInfo>(element) ?? throw new InfisicalException("Failed to deserialize response content");
  }
}

public class UniversalAuthManagement : IdentityAuthMethodManagement<UniversalAuthConfig>
{
  public UniversalAuthManagement(ApiClient apiClient) : base(apiClient, "universal-auth") { }

  public async Task<CreatedClientSecret> CreateClientSecretAsync(CreateClientSecretOptions options)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(options.IdentityId) + "/client-secrets", options).ConfigureAwait(false);
      var secret = response.TryGetProperty("clientSecret", out var s) ? s.GetString() ?? string.Empty : string.Empty;
      var data = ResponseReader.Entity<ClientSecretData>(response, "clientSecretData");
      return new CreatedClientSecret(secret, data);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create client secret", e);
    }
  }

  public async Task<ClientSecretData[]> ListClientSecretsAsync(string identityId)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(identityId) + "/client-secrets", null, null, true).ConfigureAwait(false);
      return ResponseReader.List<ClientSecretData>(response, "clientSecretData");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list client secrets", e);
    }
  }

  public async Task<ClientSecretData> GetClientSecretAsync(string identityId, string clientSecretId)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Path(identityId)}/client-secrets/{Uri.EscapeDataString(clientSecretId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<ClientSecretData>(response, "clientSecretData");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get client secret", e);
    }
  }

  public async Task<ClientSecretData> RevokeClientSecretAsync(string identityId, string clientSecretId)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"{Path(identityId)}/client-secrets/{Uri.EscapeDataString(clientSecretId)}/revoke").ConfigureAwait(false);
      return ResponseReader.Entity<ClientSecretData>(response, "clientSecretData");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to revoke client secret", e);
    }
  }

  /// <summary>Clears the lockouts caused by failed login attempts.</summary>
  public async Task ClearLockoutsAsync(string identityId)
  {
    try
    {
      await ApiClient.SendAsync<JsonElement>(HttpMethod.Post, Path(identityId) + "/clear-lockouts").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to clear lockouts", e);
    }
  }
}

public class TokenAuthManagement : IdentityAuthMethodManagement<TokenAuthConfig>
{
  public TokenAuthManagement(ApiClient apiClient) : base(apiClient, "token-auth") { }

  /// <summary>Creates a new access token for the identity. The token is only returned once.</summary>
  public async Task<MachineIdentityCredential> CreateTokenAsync(string identityId, string? name = null)
  {
    try
    {
      var response = await ApiClient.SendRequiredAsync<MachineIdentityCredential>(HttpMethod.Post, Path(identityId) + "/tokens", new { name }).ConfigureAwait(false);
      return response;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create token", e);
    }
  }

  public async Task<IdentityAccessToken[]> ListTokensAsync(string identityId, int offset = 0, int limit = 20)
  {
    try
    {
      var query = new QueryBuilder().Add("offset", offset).Add("limit", limit).Build();
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(identityId) + "/tokens", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<IdentityAccessToken>(response, "tokens");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list tokens", e);
    }
  }

  public async Task<IdentityAccessToken> UpdateTokenAsync(string tokenId, string name)
  {
    try
    {
      if (string.IsNullOrEmpty(tokenId)) throw new InfisicalException("TokenId is required");
      var response = await ApiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v1/auth/token-auth/tokens/{Uri.EscapeDataString(tokenId)}", new { name }).ConfigureAwait(false);
      return ResponseReader.Entity<IdentityAccessToken>(response, "token");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update token", e);
    }
  }

  public async Task RevokeTokenAsync(string tokenId)
  {
    try
    {
      if (string.IsNullOrEmpty(tokenId)) throw new InfisicalException("TokenId is required");
      await ApiClient.SendAsync<JsonElement>(HttpMethod.Post, $"/api/v1/auth/token-auth/tokens/{Uri.EscapeDataString(tokenId)}/revoke").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to revoke token", e);
    }
  }
}
