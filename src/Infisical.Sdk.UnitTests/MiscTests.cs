using Xunit;

namespace Infisical.Sdk.Tests;

public class MiscTests
{
  [Fact]
  public async Task Gateway_create_accepts_a_flat_response_and_generates_enrollment_tokens()
  {
    using var h = new Harness();
    h.On("POST", "/api/v3/gateways", "{\"id\":\"g1\",\"name\":\"gw\",\"authMethod\":{\"method\":\"token\"}}");
    h.On("POST", "/api/v3/gateways/g1/token-auth/generate-enrollment-token", "{\"token\":\"T\",\"expiresAt\":\"2030-01-01T00:00:00Z\"}");

    var gateway = await h.Client.Gateways().CreateAsync(new CreateGatewayOptions { Name = "gw", AuthMethod = GatewayAuthMethod.Token() });
    var token = await h.Client.Gateways().GenerateEnrollmentTokenAsync("g1");

    Assert.Equal("g1", gateway.Id);
    Assert.Equal("token", h.Requests[0].Json.GetProperty("authMethod").GetProperty("method").GetString());
    Assert.Equal("T", token.Token);
  }

  [Fact]
  public async Task Gateway_pools_accept_array_responses()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/gateway-pools", "[{\"id\":\"gp1\",\"name\":\"pool\",\"memberCount\":2,\"healthyMemberCount\":1}]");
    h.On("POST", "/api/v1/gateway-pools/gp1/memberships", "{}");

    var pools = await h.Client.GatewayPools().ListAsync();
    await h.Client.GatewayPools().AddGatewayAsync("gp1", "g1");

    Assert.Equal(1, pools[0].HealthyMemberCount);
    Assert.Equal("g1", h.Last.Json.GetProperty("gatewayId").GetString());
  }

  [Fact]
  public async Task Kmip_servers_accept_array_and_flat_responses()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/kmip/servers", "{\"id\":\"k1\",\"name\":\"kmip\"}");
    h.On("GET", "/api/v1/kmip/servers", "[{\"id\":\"k1\"},{\"id\":\"k2\"}]");

    var server = await h.Client.KmipServers().CreateAsync(new CreateKmipServerOptions { Name = "kmip", HostnamesOrIps = "kmip.local" });
    var servers = await h.Client.KmipServers().ListAsync();

    Assert.Equal("k1", server.Id);
    Assert.Equal(2, servers.Length);
  }

  [Fact]
  public async Task Alerts_require_at_least_one_channel()
  {
    using var h = new Harness();

    await Assert.ThrowsAsync<InfisicalException>(() => h.Client.Alerts().CreateAsync(new CreateAlertOptions { Name = "a", ResourceType = "certificate", EventType = "expiration" }));

    Assert.Empty(h.Requests);
  }

  [Fact]
  public async Task Alerts_create_sends_channels_with_recipients()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/alerts", "{\"alert\":{\"id\":\"a1\",\"name\":\"exp\"}}");

    await h.Client.Alerts().CreateAsync(new CreateAlertOptions
    {
      Name = "exp",
      ResourceType = "certificate",
      EventType = "expiration",
      Condition = new { daysBeforeExpiry = 30 },
      Channels = new[] { new AlertChannel { Name = "mail", ChannelType = "email", Recipients = new[] { new AlertRecipient { PrincipalType = "user", PrincipalId = "u1" } } } }
    });

    var body = h.Last.Json;
    Assert.Equal(30, body.GetProperty("condition").GetProperty("daysBeforeExpiry").GetInt32());
    Assert.Equal("u1", body.GetProperty("channels")[0].GetProperty("recipients")[0].GetProperty("principalId").GetString());
  }

  [Fact]
  public async Task Secret_scanning_findings_return_the_total_count()
  {
    using var h = new Harness();
    h.On("GET", "/api/v2/secret-scanning/findings", "{\"findings\":[{\"id\":\"f1\",\"status\":\"unresolved\"}],\"totalCount\":7}");
    h.On("GET", "/api/v2/secret-scanning/unresolved-findings-count", "{\"unresolvedFindings\":4}");

    var findings = await h.Client.SecretScanning().ListFindingsAsync(new ListSecretScanningFindingsOptions { ProjectId = "p1", Status = SecretScanningFindingStatuses.Unresolved });

    Assert.Equal(7, findings.TotalCount);
    Assert.Equal("unresolved", h.Last.Query["status"]);
    Assert.Equal(4, await h.Client.SecretScanning().GetUnresolvedFindingsCountAsync("p1"));
  }

  [Fact]
  public async Task Secret_scanning_config_reset_sends_an_explicit_null()
  {
    using var h = new Harness();
    h.On("PATCH", "/api/v2/secret-scanning/configs", "{\"config\":{}}");

    await h.Client.SecretScanning().UpdateConfigAsync(null, "p1");

    Assert.Equal("{\"content\":null}", h.Last.Body);
  }

  [Fact]
  public async Task Secret_scanning_data_source_type_is_part_of_the_path()
  {
    using var h = new Harness();
    h.On("POST", "/api/v2/secret-scanning/data-sources/github", "{\"dataSource\":{\"id\":\"ds1\",\"name\":\"gh\",\"type\":\"github\"}}");

    var source = await h.Client.SecretScanning().CreateDataSourceAsync(new CreateSecretScanningDataSourceOptions { Type = SecretScanningDataSourceTypes.GitHub, Name = "gh", ConnectionId = "cn1" });

    Assert.Equal("github", source.Type);
    Assert.False(h.Last.Json.TryGetProperty("type", out _));
  }

  [Fact]
  public async Task Agent_vault_grants_send_all_three_id_lists()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/agent-vault/access-bundles/ab1/members", "{}");

    await h.Client.AgentVault().AccessBundles.GrantAsync("ab1", new AgentVaultMemberIds { UserIds = new[] { "u1" } });

    var body = h.Last.Json;
    Assert.Equal(1, body.GetProperty("userIds").GetArrayLength());
    Assert.Equal(0, body.GetProperty("groupIds").GetArrayLength());
  }

  [Fact]
  public async Task Agent_vault_skills_are_uploaded_as_base64_files()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/agent-vault/skills", "{\"skill\":{\"id\":\"sk1\",\"name\":\"skill\"}}");

    var skill = await h.Client.AgentVault().Skills.CreateAsync(new AgentVaultSkillOptions { Files = new[] { AgentVaultSkillFile.FromText("SKILL.md", "file", "# skill") } });

    Assert.Equal("sk1", skill.Id);
    Assert.Equal("IyBza2lsbA==", h.Last.Json.GetProperty("files")[0].GetProperty("contentBase64").GetString());
  }

  [Fact]
  public async Task Agent_vault_sessions_are_created_for_bundles()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/agent-vault/sessions", "{\"session\":{\"id\":\"se1\"},\"token\":\"T\"}");

    var response = await h.Client.AgentVault().Sessions.CreateAsync(new CreateAgentVaultSessionOptions { AccessBundles = new[] { "bundle" }, Ttl = "8h" });

    Assert.Equal("T", response.GetProperty("token").GetString());
    await Assert.ThrowsAsync<InfisicalException>(() => h.Client.AgentVault().Sessions.CreateAsync(new CreateAgentVaultSessionOptions()));
  }
}
