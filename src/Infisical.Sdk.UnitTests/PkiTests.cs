using Xunit;

namespace Infisical.Sdk.Tests;

public class PkiTests
{
  [Fact]
  public async Task Certificate_issue_understands_the_nested_bundle()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/cert-manager/certificates", "{\"certificate\":{\"certificate\":\"PEM\",\"privateKey\":\"KEY\",\"serialNumber\":\"01\"},\"certificateRequestId\":\"r1\"}");

    var result = await h.Client.Certificates().IssueAsync(new IssueCertificateRequest
    {
      ProfileId = "pr1",
      Attributes = new CertificateAttributes { CommonName = "a.example.com", Ttl = "30d", AltNames = new[] { new SubjectAlternativeName { Type = "dns_name", Value = "a.example.com" } } }
    });

    Assert.Equal("KEY", result.Material!.PrivateKey);
    Assert.Equal("r1", result.CertificateRequestId);
    Assert.Equal("30d", h.Last.Json.GetProperty("attributes").GetProperty("ttl").GetString());
  }

  [Fact]
  public async Task Certificate_issue_without_material_means_asynchronous_issuance()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/cert-manager/certificates", "{\"certificateRequestId\":\"r1\",\"status\":\"pending\"}");

    var result = await h.Client.Certificates().IssueAsync(new IssueCertificateRequest { ProfileId = "pr1" });

    Assert.Null(result.Material);
    Assert.Equal("pending", result.Status);
  }

  [Fact]
  public async Task Certificate_revoke_sends_the_reason()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/cert-manager/certificates/c1/revoke", "{\"message\":\"ok\"}");

    await h.Client.Certificates().RevokeAsync("c1", "keyCompromise");

    Assert.Equal("keyCompromise", h.Last.Json.GetProperty("revocationReason").GetString());
  }

  [Fact]
  public async Task Certificate_search_posts_to_the_project_and_reads_the_total()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/projects/p1/certificates/search", "{\"certificates\":[{\"id\":\"c1\",\"commonName\":\"a\"}],\"totalCount\":12}");

    var result = await h.Client.Certificates().SearchAsync(new SearchCertificatesOptions { ProjectId = "p1", Status = "active", Limit = 10 });

    Assert.Single(result.Certificates);
    Assert.Equal(12, result.TotalCount);
    Assert.False(h.Last.Json.TryGetProperty("projectId", out _));
  }

  [Fact]
  public async Task Certificate_profiles_and_policies_roundtrip()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/cert-manager/certificate-profiles", "{\"certificateProfile\":{\"id\":\"pr1\",\"slug\":\"web\"}}");
    h.On("POST", "/api/v1/cert-manager/certificate-policies", "{\"certificatePolicy\":{\"id\":\"po1\",\"name\":\"default\"}}");

    var profile = await h.Client.CertificateProfiles().CreateAsync(new CertificateProfileOptions { Slug = "web", CertificatePolicyId = "po1", CaId = "ca1", EnrollmentType = "api", ApiConfig = new Dictionary<string, object?> { ["autoRenew"] = true } });
    var policy = await h.Client.CertificatePolicies().CreateAsync(new CertificatePolicyOptions { Name = "default" });

    Assert.Equal("web", profile.Slug);
    Assert.Equal("default", policy.Name);
    Assert.True(h.Requests[0].Json.GetProperty("apiConfig").GetProperty("autoRenew").GetBoolean());
  }

  [Fact]
  public async Task Certificate_authority_create_sends_the_configuration()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/cert-manager/ca/internal", "{\"id\":\"ca1\",\"name\":\"root\",\"status\":\"active\"}");

    var ca = await h.Client.CertificateAuthorities().CreateAsync(new CreateInternalCaOptions
    {
      ProjectId = "p1",
      Name = "root",
      Configuration = new InternalCaConfiguration { CommonName = "Root CA", KeyAlgorithm = "RSA_4096", MaxPathLength = 2 }
    });

    Assert.Equal("ca1", ca.Id);
    var config = h.Last.Json.GetProperty("configuration");
    Assert.Equal("root", config.GetProperty("type").GetString());
    Assert.Equal("RSA_4096", config.GetProperty("keyAlgorithm").GetString());
  }

  [Fact]
  public async Task Certificate_authority_csr_is_returned_as_string()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/cert-manager/ca/internal/ca1/csr", "{\"csr\":\"-----BEGIN CERTIFICATE REQUEST-----\"}");

    Assert.StartsWith("-----BEGIN", await h.Client.CertificateAuthorities().GetCsrAsync("ca1"));
  }

  [Fact]
  public async Task Inventory_views_are_split_into_system_shared_and_custom()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/cert-manager/certificate-inventory-views", "{\"systemViews\":[{\"id\":\"s\"}],\"sharedViews\":[{\"id\":\"a\"},{\"id\":\"b\"}],\"customViews\":[]}");

    var views = await h.Client.CertificateInventory().ListViewsAsync("p1");

    Assert.Single(views.SystemViews);
    Assert.Equal(2, views.SharedViews.Length);
    Assert.Empty(views.CustomViews);
  }

  [Fact]
  public async Task Inventory_filters_accept_a_single_value_or_a_list()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/cert-manager/certificate-inventory-views", "{\"view\":{\"id\":\"v1\"}}");

    await h.Client.CertificateInventory().CreateViewAsync(new CertificateInventoryViewOptions
    {
      Name = "x",
      Filters = new CertificateInventoryFilters { KeyAlgorithm = new[] { "RSA_2048", "RSA_4096" }, Source = "discovery" },
      Columns = new[] { CertificateInventoryColumns.SanCn }
    });

    var filters = h.Last.Json.GetProperty("filters");
    Assert.Equal(2, filters.GetProperty("keyAlgorithm").GetArrayLength());
    Assert.Equal("discovery", filters.GetProperty("source").GetString());
  }

  [Fact]
  public async Task Cleanup_config_is_null_when_not_configured()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/cert-manager/certificate-cleanup", "{\"config\":null}");

    Assert.Null(await h.Client.CertificateCleanup().GetConfigAsync("p1"));
  }

  [Fact]
  public async Task Cleanup_config_update_uses_put()
  {
    using var h = new Harness();
    h.On("PUT", "/api/v1/cert-manager/certificate-cleanup", "{\"config\":{\"isEnabled\":true,\"postExpiryRetentionDays\":14}}");

    var config = await h.Client.CertificateCleanup().UpdateConfigAsync(new UpdateCertificateCleanupOptions { IsEnabled = true, PostExpiryRetentionDays = 14 });

    Assert.Equal(14, config.PostExpiryRetentionDays);
  }

  [Fact]
  public async Task Legacy_certificate_templates_use_the_pki_path()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/pki/certificate-templates", "{\"certificateTemplate\":{\"id\":\"ct1\",\"name\":\"web\"}}");

    var template = await h.Client.CertificateTemplates().CreateAsync(new CertificateTemplateOptions { CaId = "ca1", Name = "web", CommonName = ".*", SubjectAlternativeName = ".*", Ttl = "90d" });

    Assert.Equal("ct1", template.Id);
  }

  [Fact]
  public async Task Discovery_jobs_send_the_network_target()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/cert-manager/discovery-jobs", "{\"id\":\"dj1\",\"name\":\"net\"}");

    await h.Client.PkiDiscovery().CreateJobAsync(new PkiDiscoveryJobOptions
    {
      Name = "net",
      TargetConfig = new PkiDiscoveryTargetConfig { IpRanges = new[] { "10.0.0.0/24" }, Ports = "443,8443" },
      GatewayPoolId = "gp1",
      ScanIntervalDays = 7
    });

    var target = h.Last.Json.GetProperty("targetConfig");
    Assert.Equal("10.0.0.0/24", target.GetProperty("ipRanges")[0].GetString());
    Assert.Equal("gp1", h.Last.Json.GetProperty("gatewayPoolId").GetString());
  }

  [Fact]
  public async Task Discovery_job_without_target_is_rejected()
  {
    using var h = new Harness();

    await Assert.ThrowsAsync<InfisicalException>(() => h.Client.PkiDiscovery().CreateJobAsync(new PkiDiscoveryJobOptions { Name = "x" }));

    Assert.Empty(h.Requests);
  }

  [Fact]
  public async Task Signers_can_be_disabled_and_used_for_signing()
  {
    using var h = new Harness();
    h.On("PATCH", "/api/v1/cert-manager/signers/sg1/status", "{\"id\":\"sg1\",\"status\":\"disabled\"}");
    h.On("POST", "/api/v1/cert-manager/signers/sg1/sign", "{\"signature\":\"SIG\"}");

    var signer = await h.Client.Signers().DisableAsync("sg1");
    var signed = await h.Client.Signers().SignAsync(new SignDataOptions { SignerId = "sg1", Data = "AAAA", SigningAlgorithm = "RSASSA_PKCS1_V1_5_SHA_256", IsDigest = true });

    Assert.Equal("disabled", signer.Status);
    Assert.Equal("SIG", signed.GetProperty("signature").GetString());
    Assert.Equal("disabled", h.Requests[0].Json.GetProperty("status").GetString());
  }
}
