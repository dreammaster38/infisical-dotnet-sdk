using System.Text;
using Infisical.Sdk.Model;
using Xunit;

namespace Infisical.Sdk.Tests;

/// <summary>
/// Marks a test that needs a running Infisical instance. It is skipped unless these environment variables are set:
/// INFISICAL_LIVE_URL (e.g. http://localhost:8080), INFISICAL_LIVE_TOKEN (an access token of a machine identity that is
/// admin of the organization, e.g. the one returned by the instance bootstrap) and INFISICAL_LIVE_ORG_ID.
/// </summary>
public sealed class LiveFactAttribute : FactAttribute
{
  public LiveFactAttribute()
  {
    if (string.IsNullOrEmpty(Live.Url) || string.IsNullOrEmpty(Live.Token) || string.IsNullOrEmpty(Live.OrgId))
    {
      Skip = "Set INFISICAL_LIVE_URL, INFISICAL_LIVE_TOKEN and INFISICAL_LIVE_ORG_ID to run the live tests.";
    }
  }
}

/// <summary>Like <see cref="LiveFactAttribute"/>, but also needs INFISICAL_LIVE_USER_EMAIL and INFISICAL_LIVE_USER_PASSWORD.</summary>
public sealed class LiveUserFactAttribute : FactAttribute
{
  public LiveUserFactAttribute()
  {
    if (string.IsNullOrEmpty(Live.Url) || string.IsNullOrEmpty(Live.OrgId) || string.IsNullOrEmpty(Live.UserEmail) || string.IsNullOrEmpty(Live.UserPassword))
    {
      Skip = "Set INFISICAL_LIVE_URL, INFISICAL_LIVE_ORG_ID, INFISICAL_LIVE_USER_EMAIL and INFISICAL_LIVE_USER_PASSWORD to run this test.";
    }
  }
}

internal static class Live
{
  public static string? UserEmail => Environment.GetEnvironmentVariable("INFISICAL_LIVE_USER_EMAIL");
  public static string? UserPassword => Environment.GetEnvironmentVariable("INFISICAL_LIVE_USER_PASSWORD");

  public static string? Url => Environment.GetEnvironmentVariable("INFISICAL_LIVE_URL");
  public static string? Token => Environment.GetEnvironmentVariable("INFISICAL_LIVE_TOKEN");
  public static string? OrgId => Environment.GetEnvironmentVariable("INFISICAL_LIVE_ORG_ID");

  public static InfisicalClient NewClient(bool authenticated = true)
  {
    var client = new InfisicalClient(new InfisicalSdkSettingsBuilder().WithHostUri(Url ?? "http://localhost:8080").Build());
    if (authenticated) client.Auth().SetAccessToken(Token ?? "");
    return client;
  }

  public static string Suffix() => Guid.NewGuid().ToString("N").Substring(0, 6);
}

/// <summary>
/// Smoke tests against a real instance. Features that need an Enterprise license (groups, custom roles, project
/// templates, gateway pools, secret rotations, secret scanning) and endpoints that only accept user logins
/// (organization details, SCIM tokens) are not covered here.
/// </summary>
[Trait("Category", "Live")]
public class LiveTests
{
  private readonly InfisicalClient _client = Live.NewClient();
  private readonly string _suffix = Live.Suffix();

  private async Task<Project> NewProjectAsync(string type = "secret-manager")
    => await _client.Projects().CreateAsync(new CreateProjectOptions { ProjectName = $"sdk-{type}-{_suffix}", Type = type });

  [LiveFact]
  public async Task Instance_status_is_ok()
  {
    var status = await Live.NewClient(false).Instance().GetStatusAsync();
    Assert.Equal("Ok", status.GetProperty("message").GetString());
  }

  [LiveFact]
  public async Task Project_environment_folder_and_secret_lifecycle()
  {
    var project = await NewProjectAsync();
    try
    {
      Assert.Equal(project.Id, (await _client.Projects().GetAsync(project.Id)).Id);
      Assert.Equal(project.Id, (await _client.Projects().GetBySlugAsync(project.Slug)).Id);
      Assert.Contains(await _client.Projects().ListAsync(), p => p.Id == project.Id);

      var env = await _client.Environments().CreateAsync(new CreateEnvironmentOptions { ProjectId = project.Id, Name = "QA", Slug = "qa" });
      Assert.Equal("qa", (await _client.Environments().GetBySlugAsync(project.Id, "qa")).Slug);
      await _client.Environments().UpdateAsync(new UpdateEnvironmentOptions { ProjectId = project.Id, EnvironmentId = env.Id, Name = "QA 2" });

      var folder = await _client.Folders().CreateAsync(new CreateFolderOptions { ProjectId = project.Id, EnvironmentSlug = "dev", Name = "backend" });
      Assert.Contains(await _client.Folders().ListAsync(new ListFoldersOptions { ProjectId = project.Id, EnvironmentSlug = "dev" }), f => f.Id == folder.Id);

      await _client.Secrets().CreateAsync(new CreateSecretOptions { ProjectId = project.Id, EnvironmentSlug = "dev", SecretName = "FOO", SecretValue = "bar" });
      Assert.Equal("bar", (await _client.Secrets().GetAsync(new GetSecretOptions { ProjectId = project.Id, EnvironmentSlug = "dev", SecretName = "FOO" })).SecretValue);
      await _client.Secrets().UpdateAsync(new UpdateSecretOptions { ProjectId = project.Id, EnvironmentSlug = "dev", SecretName = "FOO", NewSecretValue = "baz" });
      Assert.Equal("baz", (await _client.Secrets().GetAsync(new GetSecretOptions { ProjectId = project.Id, EnvironmentSlug = "dev", SecretName = "FOO" })).SecretValue);
      await _client.Secrets().DeleteAsync(new DeleteSecretOptions { ProjectId = project.Id, EnvironmentSlug = "dev", SecretName = "FOO" });
    }
    finally
    {
      await _client.Projects().DeleteAsync(project.Id);
    }
  }

  [LiveFact]
  public async Task Batch_secret_operations()
  {
    var project = await NewProjectAsync();
    try
    {
      var created = await _client.Secrets().BatchCreateAsync(new BatchCreateSecretsOptions
      {
        ProjectId = project.Id,
        EnvironmentSlug = "dev",
        Secrets = new[] { new BatchSecretInput { SecretKey = "A", SecretValue = "1" }, new BatchSecretInput { SecretKey = "B", SecretValue = "2" } }
      });
      Assert.Equal(2, created.Length);

      await _client.Secrets().BatchUpdateAsync(new BatchUpdateSecretsOptions { ProjectId = project.Id, EnvironmentSlug = "dev", Secrets = new[] { new BatchSecretInput { SecretKey = "A", SecretValue = "11" } } });
      var listed = await _client.Secrets().ListAsync(new ListSecretsOptions { ProjectId = project.Id, EnvironmentSlug = "dev" });
      Assert.Equal("11", listed.Single(s => s.SecretKey == "A").SecretValue);

      await _client.Secrets().BatchDeleteAsync(new BatchDeleteSecretsOptions { ProjectId = project.Id, EnvironmentSlug = "dev", Secrets = new[] { new BatchDeleteSecretInput { SecretKey = "A" }, new BatchDeleteSecretInput { SecretKey = "B" } } });
      Assert.Empty(await _client.Secrets().ListAsync(new ListSecretsOptions { ProjectId = project.Id, EnvironmentSlug = "dev" }));
    }
    finally
    {
      await _client.Projects().DeleteAsync(project.Id);
    }
  }

  [LiveFact]
  public async Task Tags_and_secret_imports()
  {
    var project = await NewProjectAsync();
    try
    {
      var tag = await _client.SecretTags().CreateAsync(new SecretTagOptions { ProjectId = project.Id, Slug = "prod-tag", Color = "#ff0000" });
      Assert.Contains(await _client.SecretTags().ListAsync(project.Id), t => t.Slug == "prod-tag");
      await _client.SecretTags().UpdateAsync(new SecretTagOptions { ProjectId = project.Id, TagId = tag.Id, Slug = "prod-tag", Color = "#00ff00" });
      await _client.SecretTags().DeleteAsync(project.Id, tag.Id);

      var import = await _client.SecretImports().CreateAsync(new CreateSecretImportOptions { ProjectId = project.Id, EnvironmentSlug = "dev", Import = new SecretImportSource { EnvironmentSlug = "staging", Path = "/" } });
      var location = new SecretImportLocation { ProjectId = project.Id, EnvironmentSlug = "dev" };
      Assert.NotEmpty(await _client.SecretImports().ListAsync(location));
      await _client.SecretImports().DeleteAsync(import.Id, location);
    }
    finally
    {
      await _client.Projects().DeleteAsync(project.Id);
    }
  }

  [LiveFact]
  public async Task Identity_with_universal_auth_can_log_in()
  {
    var identity = await _client.Identities().CreateAsync(new CreateIdentityOptions { Name = $"sdk-id-{_suffix}", OrganizationId = Live.OrgId!, Role = "member" });
    try
    {
      Assert.Equal(identity.Id, (await _client.Identities().GetAsync(identity.Id)).Id);
      await _client.Identities().UpdateAsync(new UpdateIdentityOptions { IdentityId = identity.Id, Name = $"sdk-id-renamed-{_suffix}" });

      var auth = _client.Identities().Auth().UniversalAuth();
      var attached = await auth.AttachAsync(identity.Id, new UniversalAuthConfig { AccessTokenTTL = 600, AccessTokenMaxTTL = 600 });
      var clientId = (await auth.GetAsync(identity.Id)).AdditionalData!["clientId"].GetString()!;

      var secret = await auth.CreateClientSecretAsync(new CreateClientSecretOptions { IdentityId = identity.Id, Description = "sdk" });
      Assert.NotEmpty(await auth.ListClientSecretsAsync(identity.Id));

      var other = Live.NewClient(false);
      var credential = await other.Auth().UniversalAuth().LoginAsync(clientId, secret.ClientSecret);
      Assert.False(string.IsNullOrEmpty(credential.AccessToken));

      await auth.RevokeClientSecretAsync(identity.Id, secret.Data.Id);
      Assert.NotNull(attached);
    }
    finally
    {
      await _client.Identities().DeleteAsync(identity.Id);
    }
  }

  [LiveFact]
  public async Task Shared_secret_roundtrip()
  {
    var id = await _client.SharedSecrets().CreateAsync(new CreateSharedSecretOptions { SecretValue = "hunter2", ExpiresIn = "1d", MaxViews = 3 });
    Assert.False(string.IsNullOrEmpty(id));
    await _client.SharedSecrets().ListAsync();
    await _client.SharedSecrets().DeleteAsync(id);
  }

  [LiveFact]
  public async Task Kms_key_encrypts_and_decrypts()
  {
    var project = await NewProjectAsync("kms");
    try
    {
      var key = await _client.Kms().CreateKeyAsync(new CreateKmsKeyOptions { ProjectId = project.Id, Name = "sdk-key", EncryptionAlgorithm = "aes-256-gcm" });
      var cipher = await _client.Kms().EncryptAsync(key.Id, Convert.ToBase64String(Encoding.UTF8.GetBytes("hello")));
      var plain = await _client.Kms().DecryptAsync(key.Id, cipher);
      Assert.Equal("hello", Encoding.UTF8.GetString(Convert.FromBase64String(plain)));
      Assert.Contains((await _client.Kms().ListKeysAsync(new ListKmsKeysOptions { ProjectId = project.Id })).Keys, k => k.Id == key.Id);
    }
    finally
    {
      await _client.Projects().DeleteAsync(project.Id);
    }
  }

  [LiveFact]
  public async Task Certificate_issue_get_and_revoke_through_an_internal_ca()
  {
    var project = await NewProjectAsync("cert-manager");
    try
    {
      var ca = await _client.CertificateAuthorities().CreateAsync(new CreateInternalCaOptions
      {
        ProjectId = project.Id,
        Name = "sdk-root",
        Configuration = new InternalCaConfiguration { CommonName = "SDK Root CA", Organization = "SDK", Country = "DE", KeyAlgorithm = "RSA_2048", NotAfter = DateTime.UtcNow.AddYears(5).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'") }
      });
      var policy = await _client.CertificatePolicies().CreateAsync(new CertificatePolicyOptions { ProjectId = project.Id, Name = "sdk-policy" });
      var profile = await _client.CertificateProfiles().CreateAsync(new CertificateProfileOptions
      {
        ProjectId = project.Id,
        Slug = "sdk-profile",
        CaId = ca.Id,
        CertificatePolicyId = policy.Id,
        EnrollmentType = "api",
        ApiConfig = new Dictionary<string, object?> { ["autoRenew"] = false }
      });
      Assert.Equal(profile.Id, (await _client.CertificateProfiles().GetBySlugAsync("sdk-profile", project.Id)).Id);

      var issued = await _client.Certificates().IssueAsync(new IssueCertificateRequest
      {
        ProfileId = profile.Id,
        Attributes = new CertificateAttributes { CommonName = "svc.example.com", Ttl = "30d", AltNames = new[] { new SubjectAlternativeName { Type = "dns_name", Value = "svc.example.com" } } }
      });
      Assert.NotNull(issued.Material);
      var certificateId = issued.Material!.CertificateId!;

      Assert.Equal(certificateId, (await _client.Certificates().GetAsync(certificateId)).Id);
      Assert.False(string.IsNullOrEmpty((await _client.Certificates().GetBundleAsync(certificateId)).PrivateKey));
      Assert.StartsWith("-----BEGIN", (await _client.Certificates().GetPrivateKeyAsync(certificateId)).PrivateKey);
      Assert.NotEmpty(await _client.Certificates().ExportPkcs12Async(certificateId, "Passw0rd-123456!", "svc"));
      Assert.False(string.IsNullOrEmpty((await _client.CertificateAuthorities().GetCertificateAsync(ca.Id)).Certificate));
      Assert.NotEmpty((await _client.Certificates().SearchAsync(new SearchCertificatesOptions { ProjectId = project.Id })).Certificates);

      await _client.Certificates().RevokeAsync(certificateId, CertificateRevocationReasons.KeyCompromise);
      await _client.CertificateCleanup().UpdateConfigAsync(new UpdateCertificateCleanupOptions { ProjectId = project.Id, IsEnabled = true, PostExpiryRetentionDays = 7 });
      Assert.True((await _client.CertificateCleanup().GetConfigAsync(project.Id))!.IsEnabled);
    }
    finally
    {
      await _client.Projects().DeleteAsync(project.Id);
    }
  }

  [LiveFact]
  public async Task Gateway_and_validation_rule_lifecycle()
  {
    var gateway = await _client.Gateways().CreateAsync(new CreateGatewayOptions { Name = $"sdk-gw-{_suffix}", AuthMethod = GatewayAuthMethod.Token() });
    Assert.Equal(gateway.Id, (await _client.Gateways().GetAsync(gateway.Id)).Id);
    Assert.False(string.IsNullOrEmpty((await _client.Gateways().GenerateEnrollmentTokenAsync(gateway.Id)).Token));

    var project = await NewProjectAsync();
    try
    {
      var rule = await _client.SecretValidationRules().CreateAsync(new SecretValidationRuleOptions { Type = SecretValidationRuleTypes.StaticSecrets, Name = $"sdk-rule-{_suffix}", ProjectId = project.Id, SecretPath = "/", KeyConstraints = new SecretConstraints { MaxLength = 64 } });
      Assert.Contains(await _client.SecretValidationRules().ListAsync(project.Id), r => r.Id == rule.Id);
      await _client.SecretValidationRules().DeleteAsync(SecretValidationRuleTypes.StaticSecrets, rule.Id);
    }
    finally
    {
      await _client.Projects().DeleteAsync(project.Id);
    }
  }

  [LiveFact]
  public async Task Catalogs_and_organization_admin_endpoints_respond()
  {
    Assert.NotEmpty(await _client.AppConnections().ListOptionsAsync());
    Assert.NotEmpty(await _client.SecretSyncs().ListOptionsAsync());
    Assert.NotEmpty((await _client.OrganizationAdmin().ListProjectsAsync()).Projects);
    Assert.NotEmpty(await _client.Organizations().Roles().ListAsync());
    await _client.Organizations().ListMembershipsAsync(Live.OrgId!);
    await _client.Organizations().ListAuditLogsAsync(new AuditLogsQuery { Limit = 5 });
    await _client.KmipServers().ListAsync();
  }

  [LiveUserFact]
  public async Task User_login_unlocks_the_endpoints_that_reject_machine_identities()
  {
    var client = Live.NewClient(false);
    var login = await client.Auth().UserAuth().LoginAsync(Live.UserEmail!, Live.UserPassword!, Live.OrgId);
    Assert.Equal(Live.OrgId, login.OrganizationId);

    Assert.Contains(await client.Organizations().ListAsync(), o => o.Id == Live.OrgId);
    Assert.Equal(Live.OrgId, (await client.Organizations().GetAsync(Live.OrgId!)).Id);

    // creating SCIM tokens needs an Enterprise license, but reading them only needs a user login
    await client.Organizations().Scim().ListTokensAsync(Live.OrgId!);

    var project = await client.Projects().CreateAsync(new CreateProjectOptions { ProjectName = $"sdk-views-{_suffix}", Type = "cert-manager" });
    try
    {
      client.SetCertManagerProject(project.Id);
      var view = await client.CertificateInventory().CreateViewAsync(new CertificateInventoryViewOptions { ProjectId = project.Id, Name = "sdk-view", Filters = new CertificateInventoryFilters { Status = "active" }, Columns = new[] { CertificateInventoryColumns.SanCn } });
      var views = await client.CertificateInventory().ListViewsAsync(project.Id);
      Assert.Contains(views.CustomViews.Concat(views.SharedViews), v => v.Id == view.Id);
      await client.CertificateInventory().DeleteViewAsync(view.Id);
    }
    finally
    {
      await client.Projects().DeleteAsync(project.Id);
    }
  }
}
