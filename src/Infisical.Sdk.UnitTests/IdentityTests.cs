using Xunit;

namespace Infisical.Sdk.Tests;

public class IdentityTests
{
  [Fact]
  public async Task Identities_get_reads_the_nested_membership_response()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/identities/i1", "{\"identity\":{\"identity\":{\"id\":\"i1\",\"name\":\"ci\"},\"role\":\"admin\"}}");

    var identity = await h.Client.Identities().GetAsync("i1");

    Assert.Equal("ci", identity.Name);
  }

  [Fact]
  public async Task Identities_create_sends_organization_and_role()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/identities", "{\"identity\":{\"id\":\"i1\",\"name\":\"ci\"}}");

    await h.Client.Identities().CreateAsync(new CreateIdentityOptions { Name = "ci", OrganizationId = "o1", Role = "member" });

    Assert.Equal("o1", h.Last.Json.GetProperty("organizationId").GetString());
    Assert.Equal("member", h.Last.Json.GetProperty("role").GetString());
  }

  [Fact]
  public async Task Universal_auth_attach_sends_ttl_ips_and_extra_properties()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/auth/universal-auth/identities/i1", "{\"identityUniversalAuth\":{\"id\":\"u1\",\"accessTokenTTL\":60}}");

    var info = await h.Client.Identities().Auth().UniversalAuth().AttachAsync("i1", new UniversalAuthConfig
    {
      AccessTokenTTL = 60,
      AccessTokenTrustedIps = new[] { new IpEntry("10.0.0.0/8") },
      Extra = new Dictionary<string, object?> { ["custom"] = "value" }
    });

    Assert.Equal(60, info.AccessTokenTTL);
    var body = h.Last.Json;
    Assert.Equal("10.0.0.0/8", body.GetProperty("accessTokenTrustedIps")[0].GetProperty("ipAddress").GetString());
    Assert.Equal("value", body.GetProperty("custom").GetString());
  }

  [Theory]
  [InlineData("aws-auth")]
  [InlineData("azure-auth")]
  [InlineData("gcp-auth")]
  [InlineData("kubernetes-auth")]
  [InlineData("oidc-auth")]
  [InlineData("jwt-auth")]
  [InlineData("oci-auth")]
  [InlineData("alicloud-auth")]
  [InlineData("tls-cert-auth")]
  [InlineData("ldap-auth")]
  [InlineData("token-auth")]
  public async Task Every_auth_method_can_be_read_and_revoked_under_its_own_path(string method)
  {
    using var h = new Harness();
    var path = $"/api/v1/auth/{method}/identities/i1";
    h.On("GET", path, "{\"cfg\":{\"id\":\"c1\"}}").On("DELETE", path, "{\"cfg\":{\"id\":\"c1\"}}");
    var auth = h.Client.Identities().Auth();

    IdentityAuthConfigInfo got = method switch
    {
      "aws-auth" => await auth.AwsAuth().GetAsync("i1"),
      "azure-auth" => await auth.AzureAuth().GetAsync("i1"),
      "gcp-auth" => await auth.GcpAuth().GetAsync("i1"),
      "kubernetes-auth" => await auth.KubernetesAuth().GetAsync("i1"),
      "oidc-auth" => await auth.OidcAuth().GetAsync("i1"),
      "jwt-auth" => await auth.JwtAuth().GetAsync("i1"),
      "oci-auth" => await auth.OciAuth().GetAsync("i1"),
      "alicloud-auth" => await auth.AlicloudAuth().GetAsync("i1"),
      "tls-cert-auth" => await auth.TlsCertAuth().GetAsync("i1"),
      "ldap-auth" => await auth.LdapAuth().GetAsync("i1"),
      _ => await auth.TokenAuth().GetAsync("i1")
    };

    Assert.Equal("c1", got.Id);
    Assert.Equal(path, h.Last.Path);
  }

  [Fact]
  public async Task Client_secret_creation_returns_the_secret_and_its_metadata()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/auth/universal-auth/identities/i1/client-secrets", "{\"clientSecret\":\"s3cret\",\"clientSecretData\":{\"id\":\"c1\",\"description\":\"d\"}}");

    var created = await h.Client.Identities().Auth().UniversalAuth().CreateClientSecretAsync(new CreateClientSecretOptions { IdentityId = "i1", Description = "d", Ttl = 3600 });

    Assert.Equal("s3cret", created.ClientSecret);
    Assert.Equal("c1", created.Data.Id);
    Assert.Equal(3600, h.Last.Json.GetProperty("ttl").GetInt32());
  }

  [Fact]
  public async Task Token_auth_tokens_can_be_listed_with_paging()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/auth/token-auth/identities/i1/tokens", "{\"tokens\":[{\"id\":\"t1\",\"name\":\"ci\"}]}");

    var tokens = await h.Client.Identities().Auth().TokenAuth().ListTokensAsync("i1", offset: 5, limit: 10);

    Assert.Equal("ci", tokens[0].Name);
    Assert.Equal("5", h.Last.Query["offset"]);
    Assert.Equal("10", h.Last.Query["limit"]);
  }

  [Fact]
  public async Task Jwt_based_logins_post_to_their_method_and_store_the_token()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/auth/oidc-auth/login", "{\"accessToken\":\"oidc-token\",\"expiresIn\":60,\"accessTokenMaxTTL\":60,\"tokenType\":\"Bearer\"}");
    h.On("GET", "/api/v1/projects", "{\"projects\":[]}");

    await h.Client.Auth().OidcAuth().LoginAsync("i1", "the.jwt", "my-org");
    await h.Client.Projects().ListAsync();

    Assert.Equal("the.jwt", h.Requests[0].Json.GetProperty("jwt").GetString());
    Assert.Equal("my-org", h.Requests[0].Json.GetProperty("organizationSlug").GetString());
    Assert.Equal("Bearer oidc-token", h.Requests[1].Authorization);
  }

  [Fact]
  public async Task Aws_login_sends_the_signed_request()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/auth/aws-auth/login", "{\"accessToken\":\"t\",\"expiresIn\":1,\"accessTokenMaxTTL\":1,\"tokenType\":\"Bearer\"}");

    await h.Client.Auth().AwsAuth().LoginAsync(new AwsAuthLoginRequest { IdentityId = "i1", IamRequestBody = "Action=GetCallerIdentity", IamRequestHeaders = "{}" });

    Assert.Equal("POST", h.Last.Json.GetProperty("iamHttpRequestMethod").GetString());
    Assert.Equal("Action=GetCallerIdentity", h.Last.Json.GetProperty("iamRequestBody").GetString());
  }

  [Fact]
  public async Task Token_renew_replaces_the_access_token()
  {
    using var h = new Harness();
    await h.LoginAsync("old");
    h.On("POST", "/api/v1/auth/token/renew", "{\"accessToken\":\"new\",\"expiresIn\":1,\"accessTokenMaxTTL\":1,\"tokenType\":\"Bearer\"}");
    h.On("GET", "/api/v1/projects", "{\"projects\":[]}");

    await h.Client.Auth().RenewTokenAsync("old");
    await h.Client.Projects().ListAsync();

    Assert.Equal("Bearer new", h.Last.Authorization);
  }
}
