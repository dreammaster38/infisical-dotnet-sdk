using Infisical.Sdk.Api;
using Infisical.Sdk.Model;

namespace Infisical.Sdk.Client;


public class UniversalAuth
{

  public UniversalAuth(ApiClient apiClient, Action<string> setAccessTokenFunc)
  {
    _apiClient = apiClient;
    _setAccessTokenFunc = setAccessTokenFunc;
  }

  public async Task<MachineIdentityCredential> LoginAsync(string clientId, string clientSecret)
  {
    try
    {
      var loginRequest = new UniversalAuthLoginRequest(clientId, clientSecret);

      var response = await _apiClient.PostAsync<UniversalAuthLoginRequest, MachineIdentityCredential>("/api/v1/auth/universal-auth/login", loginRequest).ConfigureAwait(false);
      _setAccessTokenFunc(response.AccessToken);
      return response;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to login", e);
    }
  }

  private readonly ApiClient _apiClient;
  private readonly Action<string> _setAccessTokenFunc;
}

public class LdapAuth
{

  public LdapAuth(ApiClient apiClient, Action<string> setAccessTokenFunc)
  {
    _apiClient = apiClient;
    _setAccessTokenFunc = setAccessTokenFunc;
  }

  public async Task<MachineIdentityCredential> LoginAsync(string identityId, string username, string password)
  {
    try
    {
      var loginRequest = new LdapAuthLoginRequest(identityId, username, password);

      var response = await _apiClient.PostAsync<LdapAuthLoginRequest, MachineIdentityCredential>("/api/v1/auth/ldap-auth/login", loginRequest).ConfigureAwait(false);
      _setAccessTokenFunc(response.AccessToken);
      return response;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to login", e);
    }
  }

  private readonly ApiClient _apiClient;
  private readonly Action<string> _setAccessTokenFunc;
}


/// <summary>Base class for the login clients of the remaining auth methods.</summary>
public abstract class LoginBase
{
  protected LoginBase(ApiClient apiClient, Action<string> setAccessTokenFunc)
  {
    _apiClient = apiClient;
    _setAccessTokenFunc = setAccessTokenFunc;
  }

  protected async Task<MachineIdentityCredential> LoginInternalAsync(string path, object body)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<MachineIdentityCredential>(HttpMethod.Post, path, body).ConfigureAwait(false);
      _setAccessTokenFunc(response.AccessToken);
      return response;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to login", e);
    }
  }

  private readonly ApiClient _apiClient;
  private readonly Action<string> _setAccessTokenFunc;
}

/// <summary>Login with a JWT / token issued by the platform (Azure, GCP, Kubernetes, OIDC, JWT auth).</summary>
public class JwtBasedAuth : LoginBase
{
  private readonly string _path;

  public JwtBasedAuth(ApiClient apiClient, Action<string> setAccessTokenFunc, string method) : base(apiClient, setAccessTokenFunc)
  {
    _path = $"/api/v1/auth/{method}/login";
  }

  public Task<MachineIdentityCredential> LoginAsync(string identityId, string jwt, string? organizationSlug = null)
  {
    return LoginInternalAsync(_path, new JwtLoginRequest { IdentityId = identityId, Jwt = jwt, OrganizationSlug = organizationSlug });
  }
}

public class AwsAuth : LoginBase
{
  public AwsAuth(ApiClient apiClient, Action<string> setAccessTokenFunc) : base(apiClient, setAccessTokenFunc) { }

  public Task<MachineIdentityCredential> LoginAsync(AwsAuthLoginRequest request)
  {
    return LoginInternalAsync("/api/v1/auth/aws-auth/login", request);
  }
}

public class OciAuth : LoginBase
{
  public OciAuth(ApiClient apiClient, Action<string> setAccessTokenFunc) : base(apiClient, setAccessTokenFunc) { }

  public Task<MachineIdentityCredential> LoginAsync(OciAuthLoginRequest request)
  {
    return LoginInternalAsync("/api/v1/auth/oci-auth/login", request);
  }
}

public class AlicloudAuth : LoginBase
{
  public AlicloudAuth(ApiClient apiClient, Action<string> setAccessTokenFunc) : base(apiClient, setAccessTokenFunc) { }

  public Task<MachineIdentityCredential> LoginAsync(AlicloudAuthLoginRequest request)
  {
    return LoginInternalAsync("/api/v1/auth/alicloud-auth/login", request);
  }
}

/// <summary>
/// TLS certificate auth. The client certificate is read by the server from the TLS handshake (or from the
/// header set by a trusted proxy), so the HTTP client has to present it.
/// </summary>
public class TlsCertAuth : LoginBase
{
  public TlsCertAuth(ApiClient apiClient, Action<string> setAccessTokenFunc) : base(apiClient, setAccessTokenFunc) { }

  public Task<MachineIdentityCredential> LoginAsync(string identityId, string? organizationSlug = null)
  {
    return LoginInternalAsync("/api/v1/auth/tls-cert-auth/login", new { identityId, organizationSlug });
  }
}


/// <summary>
/// Login as a user (email and password). Some endpoints only accept user tokens, e.g. listing organizations, SCIM
/// tokens or creating certificate inventory views. User tokens are short lived and are not refreshed by the SDK, call
/// <see cref="LoginAsync"/> again when one expires. Users with MFA enabled are not supported.
/// </summary>
public class UserAuth
{
  private readonly ApiClient _apiClient;
  private readonly Action<string> _setAccessTokenFunc;

  public UserAuth(ApiClient apiClient, Action<string> setAccessTokenFunc)
  {
    _apiClient = apiClient;
    _setAccessTokenFunc = setAccessTokenFunc;
  }

  /// <param name="email">Email address of the user.</param>
  /// <param name="password">Password of the user.</param>
  /// <param name="organizationId">Organization to scope the token to. Required for most endpoints.</param>
  public async Task<UserLoginResult> LoginAsync(string email, string password, string? organizationId = null)
  {
    try
    {
      if (string.IsNullOrEmpty(email)) throw new InfisicalException("Email is required");
      if (string.IsNullOrEmpty(password)) throw new InfisicalException("Password is required");

      var login = await _apiClient.SendRequiredAsync<System.Text.Json.JsonElement>(HttpMethod.Post, "/api/v3/auth/login", new { email, password }).ConfigureAwait(false);
      var accessToken = login.GetProperty("accessToken").GetString() ?? string.Empty;
      _setAccessTokenFunc(accessToken);

      return organizationId == null ? new UserLoginResult(accessToken, null) : await SelectOrganizationAsync(organizationId).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to login as user", e);
    }
  }

  /// <summary>Scopes the current user token to an organization and uses the new token for further requests.</summary>
  public async Task<UserLoginResult> SelectOrganizationAsync(string organizationId)
  {
    try
    {
      if (string.IsNullOrEmpty(organizationId)) throw new InfisicalException("OrganizationId is required");
      var response = await _apiClient.SendRequiredAsync<System.Text.Json.JsonElement>(HttpMethod.Post, "/api/v3/auth/select-organization", new { organizationId }).ConfigureAwait(false);

      if (response.TryGetProperty("isMfaEnabled", out var mfa) && mfa.ValueKind == System.Text.Json.JsonValueKind.True)
      {
        throw new InfisicalException("The user has MFA enabled for this organization, which the SDK does not support");
      }

      var token = response.GetProperty("token").GetString() ?? string.Empty;
      _setAccessTokenFunc(token);
      return new UserLoginResult(token, organizationId);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to select organization", e);
    }
  }
}

public class AuthClient
{
  private readonly ApiClient _apiClient;
  UniversalAuth _universalAuth;
  LdapAuth _ldapAuth;
  AwsAuth _awsAuth;
  JwtBasedAuth _azureAuth;
  JwtBasedAuth _gcpAuth;
  JwtBasedAuth _kubernetesAuth;
  JwtBasedAuth _oidcAuth;
  JwtBasedAuth _jwtAuth;
  OciAuth _ociAuth;
  AlicloudAuth _alicloudAuth;
  TlsCertAuth _tlsCertAuth;
  UserAuth _userAuth;
  private readonly Action<string> _setAccessTokenFunc;

  public AuthClient(ApiClient apiClient, Action<string> setAccessTokenFunc)
  {
    _apiClient = apiClient;
    _setAccessTokenFunc = setAccessTokenFunc;
    _universalAuth = new UniversalAuth(_apiClient, _setAccessTokenFunc);
    _ldapAuth = new LdapAuth(_apiClient, _setAccessTokenFunc);
    _awsAuth = new AwsAuth(_apiClient, _setAccessTokenFunc);
    _azureAuth = new JwtBasedAuth(_apiClient, _setAccessTokenFunc, "azure-auth");
    _gcpAuth = new JwtBasedAuth(_apiClient, _setAccessTokenFunc, "gcp-auth");
    _kubernetesAuth = new JwtBasedAuth(_apiClient, _setAccessTokenFunc, "kubernetes-auth");
    _oidcAuth = new JwtBasedAuth(_apiClient, _setAccessTokenFunc, "oidc-auth");
    _jwtAuth = new JwtBasedAuth(_apiClient, _setAccessTokenFunc, "jwt-auth");
    _ociAuth = new OciAuth(_apiClient, _setAccessTokenFunc);
    _alicloudAuth = new AlicloudAuth(_apiClient, _setAccessTokenFunc);
    _tlsCertAuth = new TlsCertAuth(_apiClient, _setAccessTokenFunc);
    _userAuth = new UserAuth(_apiClient, _setAccessTokenFunc);
  }

  public UniversalAuth UniversalAuth()
  {
    return _universalAuth;
  }

  public LdapAuth LdapAuth()
  {
    return _ldapAuth;
  }

  public AwsAuth AwsAuth() => _awsAuth;
  public JwtBasedAuth AzureAuth() => _azureAuth;
  public JwtBasedAuth GcpAuth() => _gcpAuth;
  public JwtBasedAuth KubernetesAuth() => _kubernetesAuth;
  public JwtBasedAuth OidcAuth() => _oidcAuth;
  public JwtBasedAuth JwtAuth() => _jwtAuth;
  public OciAuth OciAuth() => _ociAuth;
  public AlicloudAuth AlicloudAuth() => _alicloudAuth;
  public TlsCertAuth TlsCertAuth() => _tlsCertAuth;
  public UserAuth UserAuth() => _userAuth;

  /// <summary>
  /// Uses an existing access token (e.g. a token auth token or the token returned by the instance bootstrap) for all
  /// following requests, without logging in.
  /// </summary>
  public void SetAccessToken(string accessToken)
  {
    if (string.IsNullOrEmpty(accessToken)) throw new InfisicalException("AccessToken is required");
    _setAccessTokenFunc(accessToken);
  }

  /// <summary>Renews the given access token (extends its TTL) and uses the renewed token for further requests.</summary>
  public async Task<MachineIdentityCredential> RenewTokenAsync(string accessToken)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<MachineIdentityCredential>(HttpMethod.Post, "/api/v1/auth/token/renew", new { accessToken }).ConfigureAwait(false);
      _setAccessTokenFunc(response.AccessToken);
      return response;
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to renew token", e);
    }
  }

  /// <summary>Revokes the given access token.</summary>
  public async Task RevokeTokenAsync(string accessToken)
  {
    try
    {
      await _apiClient.SendAsync<System.Text.Json.JsonElement>(HttpMethod.Post, "/api/v1/auth/token/revoke", new { accessToken }).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to revoke token", e);
    }
  }
}
