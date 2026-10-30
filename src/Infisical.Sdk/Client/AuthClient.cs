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
