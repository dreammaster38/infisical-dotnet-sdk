using Xunit;

namespace Infisical.Sdk.Tests;

public class PlatformTests
{
  [Fact]
  public async Task Projects_create_sends_the_body_and_unwraps_the_project()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/projects", "{\"project\":{\"id\":\"p1\",\"name\":\"X\",\"slug\":\"x\"}}");

    var project = await h.Client.Projects().CreateAsync(new CreateProjectOptions { ProjectName = "X", Type = "secret-manager" });

    Assert.Equal("p1", project.Id);
    Assert.Equal("X", h.Last.Json.GetProperty("projectName").GetString());
    Assert.Equal("secret-manager", h.Last.Json.GetProperty("type").GetString());
  }

  [Fact]
  public async Task Projects_list_sends_lowercase_booleans()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/projects", "{\"projects\":[{\"id\":\"p1\"},{\"id\":\"p2\"}]}");

    var projects = await h.Client.Projects().ListAsync(new ListProjectsOptions { IncludeRoles = true });

    Assert.Equal(2, projects.Length);
    Assert.Equal("true", h.Last.Query["includeRoles"]);
  }

  [Fact]
  public async Task Environments_update_uses_patch_and_leaves_ids_out_of_the_body()
  {
    using var h = new Harness();
    h.On("PATCH", "/api/v1/projects/p1/environments/e1", "{\"environment\":{\"id\":\"e1\",\"slug\":\"prod\"}}");

    var env = await h.Client.Environments().UpdateAsync(new UpdateEnvironmentOptions { ProjectId = "p1", EnvironmentId = "e1", Slug = "prod" });

    Assert.Equal("prod", env.Slug);
    Assert.False(h.Last.Json.TryGetProperty("ProjectId", out _));
    Assert.False(h.Last.Json.TryGetProperty("environmentId", out _));
  }

  [Fact]
  public async Task Environments_delete_passes_hardDelete_as_query()
  {
    using var h = new Harness();
    h.On("DELETE", "/api/v1/projects/p1/environments/e1", "{\"environment\":{\"id\":\"e1\"}}");

    await h.Client.Environments().DeleteAsync("p1", "e1", hardDelete: true);

    Assert.Equal("true", h.Last.Query["hardDelete"]);
  }

  [Fact]
  public async Task Folders_list_sends_all_query_parameters()
  {
    using var h = new Harness();
    h.On("GET", "/api/v2/folders", "{\"folders\":[{\"id\":\"f1\",\"name\":\"a\"}]}");

    var folders = await h.Client.Folders().ListAsync(new ListFoldersOptions { ProjectId = "p1", EnvironmentSlug = "dev", Path = "/app", Recursive = true });

    Assert.Single(folders);
    Assert.Equal("p1", h.Last.Query["projectId"]);
    Assert.Equal("dev", h.Last.Query["environment"]);
    Assert.Equal("/app", h.Last.Query["path"]);
    Assert.Equal("true", h.Last.Query["recursive"]);
  }

  [Fact]
  public async Task Folders_delete_sends_a_body()
  {
    using var h = new Harness();
    h.On("DELETE", "/api/v2/folders/f1", "{\"folder\":{\"id\":\"f1\"}}");

    await h.Client.Folders().DeleteAsync(new DeleteFolderOptions { FolderIdOrName = "f1", ProjectId = "p1", EnvironmentSlug = "dev", ForceDelete = true });

    Assert.True(h.Last.Json.GetProperty("forceDelete").GetBoolean());
  }

  [Fact]
  public async Task SecretImports_create_nests_the_source()
  {
    using var h = new Harness();
    h.On("POST", "/api/v2/secret-imports", "{\"secretImport\":{\"id\":\"i1\",\"importPath\":\"/shared\"}}");

    var import = await h.Client.SecretImports().CreateAsync(new CreateSecretImportOptions
    {
      ProjectId = "p1",
      EnvironmentSlug = "dev",
      Import = new SecretImportSource { EnvironmentSlug = "prod", Path = "/shared" }
    });

    Assert.Equal("/shared", import.ImportPath);
    Assert.Equal("prod", h.Last.Json.GetProperty("import").GetProperty("environment").GetString());
  }

  [Fact]
  public async Task SecretTags_update_requires_the_tag_id()
  {
    using var h = new Harness();

    await Assert.ThrowsAsync<InfisicalException>(() => h.Client.SecretTags().UpdateAsync(new SecretTagOptions { ProjectId = "p1", Slug = "x", Color = "#fff" }));

    Assert.Empty(h.Requests);
  }

  [Fact]
  public async Task Secrets_batch_create_posts_to_the_v4_endpoint()
  {
    using var h = new Harness();
    h.On("POST", "/api/v4/secrets/batch", "{\"secrets\":[{\"id\":\"s1\",\"secretKey\":\"A\"},{\"id\":\"s2\",\"secretKey\":\"B\"}]}");

    var secrets = await h.Client.Secrets().BatchCreateAsync(new BatchCreateSecretsOptions
    {
      ProjectId = "p1",
      EnvironmentSlug = "dev",
      Secrets = new[] { new BatchSecretInput { SecretKey = "A", SecretValue = "1" }, new BatchSecretInput { SecretKey = "B", SecretValue = "2" } }
    });

    Assert.Equal(new[] { "A", "B" }, secrets.Select(s => s.SecretKey));
    Assert.Equal(2, h.Last.Json.GetProperty("secrets").GetArrayLength());
    Assert.Equal("/", h.Last.Json.GetProperty("secretPath").GetString());
  }

  [Fact]
  public async Task Secrets_batch_delete_and_move_use_the_expected_verbs()
  {
    using var h = new Harness();
    h.On("DELETE", "/api/v4/secrets/batch", "{\"secrets\":[]}");
    h.On("PATCH", "/api/v4/secrets/batch", "{\"secrets\":[]}");
    h.On("POST", "/api/v4/secrets/move", "{\"ok\":true}");

    await h.Client.Secrets().BatchDeleteAsync(new BatchDeleteSecretsOptions { ProjectId = "p", EnvironmentSlug = "e", Secrets = new[] { new BatchDeleteSecretInput { SecretKey = "A" } } });
    await h.Client.Secrets().BatchUpdateAsync(new BatchUpdateSecretsOptions { ProjectId = "p", EnvironmentSlug = "e", Mode = "ignoreNotFound", Secrets = new[] { new BatchSecretInput { SecretKey = "A", SecretValue = "x" } } });
    await h.Client.Secrets().MoveAsync(new MoveSecretsOptions { ProjectId = "p", SourceEnvironment = "dev", DestinationEnvironment = "prod", SecretIds = new[] { "s1" } });

    Assert.Equal(new[] { "DELETE", "PATCH", "POST" }, h.Requests.Select(r => r.Method));
  }
}
