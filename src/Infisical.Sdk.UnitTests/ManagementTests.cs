using System.Text.Json;
using Xunit;

namespace Infisical.Sdk.Tests;

public class ManagementTests
{
  private static readonly DynamicSecretLocation Location = new DynamicSecretLocation { ProjectSlug = "proj", EnvironmentSlug = "dev", Path = "/db" };

  [Fact]
  public async Task Dynamic_secret_lease_returns_lease_and_generated_credentials()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/dynamic-secrets/leases", "{\"lease\":{\"id\":\"l1\"},\"dynamicSecret\":{\"id\":\"d1\",\"name\":\"db\"},\"data\":{\"DB_USERNAME\":\"u\",\"DB_PASSWORD\":\"p\"}}");

    var result = await h.Client.DynamicSecrets().CreateLeaseAsync(new CreateDynamicSecretLeaseOptions { ProjectSlug = "proj", EnvironmentSlug = "dev", DynamicSecretName = "db", Ttl = "1h" });

    Assert.Equal("l1", result.Lease.Id);
    Assert.Equal("db", result.DynamicSecret.Name);
    Assert.Equal("p", result.Data["DB_PASSWORD"].GetString());
    Assert.Equal("1h", h.Last.Json.GetProperty("ttl").GetString());
  }

  [Fact]
  public async Task Dynamic_secret_get_passes_the_location_as_query()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/dynamic-secrets/db", "{\"dynamicSecret\":{\"id\":\"d1\",\"name\":\"db\"}}");

    await h.Client.DynamicSecrets().GetAsync("db", Location);

    Assert.Equal("proj", h.Last.Query["projectSlug"]);
    Assert.Equal("dev", h.Last.Query["environmentSlug"]);
    Assert.Equal("/db", h.Last.Query["path"]);
  }

  [Fact]
  public async Task Dynamic_secret_update_nests_changes_in_data()
  {
    using var h = new Harness();
    h.On("PATCH", "/api/v1/dynamic-secrets/db", "{\"dynamicSecret\":{\"id\":\"d1\"}}");

    await h.Client.DynamicSecrets().UpdateAsync(new UpdateDynamicSecretOptions
    {
      Name = "db",
      ProjectSlug = "proj",
      EnvironmentSlug = "dev",
      Data = new UpdateDynamicSecretData { DefaultTTL = "2h" }
    });

    Assert.Equal("2h", h.Last.Json.GetProperty("data").GetProperty("defaultTTL").GetString());
  }

  [Fact]
  public async Task Groups_support_users_and_machine_identities()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/groups/g1/users/alice", "{}");
    h.On("DELETE", "/api/v1/groups/g1/machine-identities/i1", "{}");

    await h.Client.Groups().AddUserAsync("g1", "alice");
    await h.Client.Groups().RemoveMachineIdentityAsync("g1", "i1");

    Assert.Equal(2, h.Requests.Count);
  }

  [Fact]
  public async Task Groups_create_returns_the_group()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/groups", "{\"id\":\"g1\",\"name\":\"devs\",\"slug\":\"devs\"}");

    var group = await h.Client.Groups().CreateAsync(new CreateGroupOptions { Name = "devs", Role = "member" });

    Assert.Equal("g1", group.Id);
  }

  [Fact]
  public async Task Kms_encrypt_and_decrypt_round_trip_the_payload_fields()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/kms/keys/k1/encrypt", "{\"ciphertext\":\"CIPHER\"}");
    h.On("POST", "/api/v1/kms/keys/k1/decrypt", "{\"plaintext\":\"PLAIN\"}");

    Assert.Equal("CIPHER", await h.Client.Kms().EncryptAsync("k1", "AAAA"));
    Assert.Equal("AAAA", h.Last.Json.GetProperty("plaintext").GetString());
    Assert.Equal("PLAIN", await h.Client.Kms().DecryptAsync("k1", "CIPHER"));
  }

  [Fact]
  public async Task Kms_list_returns_keys_and_total_count()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/kms/keys", "{\"keys\":[{\"id\":\"k1\",\"name\":\"a\"}],\"totalCount\":9}");

    var list = await h.Client.Kms().ListKeysAsync(new ListKmsKeysOptions { ProjectId = "p1", Limit = 5 });

    Assert.Single(list.Keys);
    Assert.Equal(9, list.TotalCount);
    Assert.Equal("5", h.Last.Query["limit"]);
  }

  [Fact]
  public async Task Kms_verify_reports_the_validity()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/kms/keys/k1/verify", "{\"signatureValid\":true,\"keyId\":\"k1\"}");

    var result = await h.Client.Kms().VerifyAsync("k1", "data", "sig", "RSASSA_PKCS1_V1_5_SHA_256");

    Assert.True(result.SignatureValid);
  }

  [Fact]
  public async Task Shared_secrets_create_returns_the_id()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/shared-secrets", "{\"id\":\"ss1\"}");

    var id = await h.Client.SharedSecrets().CreateAsync(new CreateSharedSecretOptions { SecretValue = "hunter2", ExpiresIn = "1d", MaxViews = 1 });

    Assert.Equal("ss1", id);
    Assert.Equal(1, h.Last.Json.GetProperty("maxViews").GetInt32());
  }

  [Fact]
  public async Task Project_roles_and_memberships_use_the_expected_paths()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/projects/p1/roles", "{\"role\":{\"id\":\"r1\",\"slug\":\"ops\",\"name\":\"Ops\"}}");
    h.On("POST", "/api/v1/projects/p1/memberships/identities/i1", "{}");
    h.On("PATCH", "/api/v1/projects/p1/memberships/groups/g1", "{}");

    var role = await h.Client.ProjectAccess().CreateRoleAsync(new ProjectRoleOptions { ProjectId = "p1", Slug = "ops", Name = "Ops", Permissions = new object[] { new { subject = "secrets", action = new[] { "read" } } } });
    await h.Client.ProjectAccess().AddIdentityAsync("p1", "i1", new ProjectRoleAssignment { Role = "member" });
    await h.Client.ProjectAccess().UpdateGroupRolesAsync("p1", "g1", new ProjectRoleAssignment { Role = "admin" });

    Assert.Equal("ops", role.Slug);
    Assert.Equal("member", h.Requests[1].Json.GetProperty("roles")[0].GetProperty("role").GetString());
  }

  [Fact]
  public async Task Identity_privileges_support_temporary_validity()
  {
    using var h = new Harness();
    h.On("POST", "/api/v2/identity-project-additional-privilege", "{\"privilege\":{\"id\":\"pv1\",\"slug\":\"tmp\",\"isTemporary\":true}}");

    var privilege = await h.Client.IdentityPrivileges().CreateAsync(new CreateIdentityPrivilegeOptions
    {
      IdentityId = "i1",
      ProjectId = "p1",
      Permissions = new object[] { new { subject = "secrets", action = new[] { "read" } } },
      Type = PrivilegeValidity.Temporary("1h", new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc))
    });

    Assert.True(privilege.IsTemporary);
    var type = h.Last.Json.GetProperty("type");
    Assert.True(type.GetProperty("isTemporary").GetBoolean());
    Assert.Equal("relative", type.GetProperty("temporaryMode").GetString());
    Assert.Equal("2030-01-01T12:00:00.000Z", type.GetProperty("temporaryAccessStartTime").GetString());
  }

  [Fact]
  public async Task Secret_validation_rules_send_constraints()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/secret-validation-rules/static-secrets", "{\"secretValidationRule\":{\"id\":\"vr1\",\"name\":\"keys\"}}");

    var rule = await h.Client.SecretValidationRules().CreateAsync(new SecretValidationRuleOptions
    {
      Type = SecretValidationRuleTypes.StaticSecrets,
      Name = "keys",
      ProjectId = "p1",
      SecretPath = "/",
      KeyConstraints = new SecretConstraints { RegexPattern = "^[A-Z_]+$", MaxLength = 64 },
      ValueConstraints = new SecretConstraints { MinLength = 16, UniqueAcrossLastVersions = 5 }
    });

    Assert.Equal("vr1", rule.Id);
    Assert.Equal(64, h.Last.Json.GetProperty("keyConstraints").GetProperty("maxLength").GetInt32());
    Assert.Equal(5, h.Last.Json.GetProperty("valueConstraints").GetProperty("uniqueAcrossLastVersions").GetInt32());
  }

  [Fact]
  public async Task Project_templates_send_environments_and_roles()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/project-templates", "{\"projectTemplate\":{\"id\":\"t1\",\"name\":\"tpl\"}}");

    await h.Client.ProjectTemplates().CreateAsync(new ProjectTemplateOptions
    {
      Name = "tpl",
      Type = "secret-manager",
      Environments = new[] { new ProjectTemplateEnvironment { Name = "Dev", Slug = "dev", Position = 1 } },
      Roles = new[] { new ProjectTemplateRole { Slug = "r", Name = "R", Permissions = new object[] { new { action = new[] { "read" }, subject = "secrets" } } } }
    });

    Assert.Equal("dev", h.Last.Json.GetProperty("environments")[0].GetProperty("slug").GetString());
  }
}
