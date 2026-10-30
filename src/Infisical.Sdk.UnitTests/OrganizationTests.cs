using Xunit;

namespace Infisical.Sdk.Tests;

public class OrganizationTests
{
  [Fact]
  public async Task Org_memberships_are_listed_from_the_users_envelope()
  {
    using var h = new Harness();
    h.On("GET", "/api/v2/organizations/o1/memberships", "{\"users\":[{\"id\":\"m1\",\"role\":\"admin\"}]}");

    var memberships = await h.Client.Organizations().ListMembershipsAsync("o1");

    Assert.Equal("admin", memberships[0].Role);
  }

  [Fact]
  public async Task Org_membership_update_sends_role_only()
  {
    using var h = new Harness();
    h.On("PATCH", "/api/v2/organizations/o1/memberships/m1", "{\"membership\":{\"id\":\"m1\",\"role\":\"member\"}}");

    await h.Client.Organizations().UpdateMembershipAsync(new UpdateOrganizationMembershipOptions { OrganizationId = "o1", MembershipId = "m1", Role = "member" });

    Assert.Equal("{\"role\":\"member\"}", h.Last.Body);
  }

  [Fact]
  public async Task Org_roles_and_sub_organizations_roundtrip()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/organization/roles", "{\"role\":{\"id\":\"r1\",\"slug\":\"ops\"}}");
    h.On("POST", "/api/v1/sub-organizations", "{\"organization\":{\"id\":\"so1\",\"name\":\"Team\"}}");
    h.On("GET", "/api/v1/sub-organizations", "{\"organizations\":[{\"id\":\"so1\"}]}");

    var role = await h.Client.Organizations().Roles().CreateAsync(new OrganizationRoleOptions { Slug = "ops", Name = "Ops", Permissions = new object[] { new { subject = "workspace", action = new[] { "create" } } } });
    var sub = await h.Client.Organizations().SubOrganizations().CreateAsync("Team");
    var subs = await h.Client.Organizations().SubOrganizations().ListAsync();

    Assert.Equal("ops", role.Slug);
    Assert.Equal("so1", sub.Id);
    Assert.Single(subs);
  }

  [Fact]
  public async Task Audit_logs_join_event_types_with_commas()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/organization/audit-logs", "{\"auditLogs\":[]}");

    await h.Client.Organizations().ListAuditLogsAsync(new AuditLogsQuery { EventTypes = new[] { "create-secret", "get-secrets" }, Limit = 10, ProjectId = "p1" });

    Assert.Equal("create-secret,get-secrets", h.Last.Query["eventType"]);
    Assert.Equal("10", h.Last.Query["limit"]);
  }

  [Fact]
  public async Task Sso_configs_use_their_own_endpoints()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/sso/oidc/config", "{}").On("PATCH", "/api/v1/sso/config", "{}").On("GET", "/api/v1/ldap/config", "{}");

    await h.Client.Organizations().Sso().CreateOidcConfigAsync(new OidcConfigOptions { OrganizationId = "o1", ConfigurationType = "discoveryURL", DiscoveryUrl = "https://idp/.well-known", ClientId = "c", ClientSecret = "s", IsActive = true });
    await h.Client.Organizations().Sso().UpdateSamlConfigAsync(new SamlConfigOptions { OrganizationId = "o1", IsActive = false });
    await h.Client.Organizations().Sso().GetLdapConfigAsync("o1");

    Assert.Equal("discoveryURL", h.Requests[0].Json.GetProperty("configurationType").GetString());
    Assert.Equal("o1", h.Requests[2].Query["organizationId"]);
  }

  [Fact]
  public async Task Scim_token_creation_returns_the_plain_token()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/scim/scim-tokens", "{\"scimToken\":\"st_secret\"}");
    h.On("GET", "/api/v1/scim/scim-tokens", "{\"scimTokens\":[{\"id\":\"t1\",\"ttlDays\":30}]}");

    Assert.Equal("st_secret", await h.Client.Organizations().Scim().CreateTokenAsync("o1", "okta", 30));
    Assert.Equal(30, h.Last.Json.GetProperty("ttlDays").GetInt32());
    Assert.Equal(30, (await h.Client.Organizations().Scim().ListTokensAsync("o1"))[0].TtlDays);
  }

  [Fact]
  public async Task Org_admin_lists_projects_with_count()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/organization-admin/projects", "{\"projects\":[{\"id\":\"p1\"}],\"count\":7}");

    var result = await h.Client.OrganizationAdmin().ListProjectsAsync("x", 0, 10);

    Assert.Single(result.Projects);
    Assert.Equal(7, result.Count);
  }

  [Fact]
  public async Task Instance_bootstrap_posts_the_admin_details()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/admin/bootstrap", "{\"message\":\"ok\"}");

    await h.Client.Instance().BootstrapAsync("a@b.c", "pw", "org");

    Assert.Equal("org", h.Last.Json.GetProperty("organization").GetString());
  }
}
