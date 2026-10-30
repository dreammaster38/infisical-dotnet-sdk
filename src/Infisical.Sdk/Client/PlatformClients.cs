using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

public class ProjectsClient
{
  private readonly ApiClient _apiClient;

  public ProjectsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  public async Task<Project> CreateAsync(CreateProjectOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v1/projects", options).ConfigureAwait(false);
      return ResponseReader.Entity<Project>(response, "project", "workspace");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create project", e);
    }
  }

  public async Task<Project[]> ListAsync(ListProjectsOptions? options = null)
  {
    try
    {
      var query = new QueryBuilder()
        .Add("includeRoles", options?.IncludeRoles ?? false)
        .Add("type", options?.Type!)
        .Build();

      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v1/projects", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<Project>(response, "projects", "workspaces");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list projects", e);
    }
  }

  public async Task<Project> GetAsync(string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/projects/{Uri.EscapeDataString(projectId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<Project>(response, "project", "workspace");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get project", e);
    }
  }

  public async Task<Project> GetBySlugAsync(string slug)
  {
    try
    {
      if (string.IsNullOrEmpty(slug)) throw new InfisicalException("Slug is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/projects/slug/{Uri.EscapeDataString(slug)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<Project>(response, "project", "workspace");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get project by slug", e);
    }
  }

  public async Task<Project> UpdateAsync(UpdateProjectOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v1/projects/{Uri.EscapeDataString(options.ProjectId)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<Project>(response, "project", "workspace");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update project", e);
    }
  }

  public async Task<Project> DeleteAsync(string projectId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v1/projects/{Uri.EscapeDataString(projectId)}").ConfigureAwait(false);
      return ResponseReader.Entity<Project>(response, "project", "workspace");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete project", e);
    }
  }
}

public class EnvironmentsClient
{
  private readonly ApiClient _apiClient;

  public EnvironmentsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  public async Task<ProjectEnvironment> CreateAsync(CreateEnvironmentOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"/api/v1/projects/{Uri.EscapeDataString(options.ProjectId)}/environments", options).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectEnvironment>(response, "environment");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create environment", e);
    }
  }

  public async Task<ProjectEnvironment> GetBySlugAsync(string projectId, string environmentSlug)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      if (string.IsNullOrEmpty(environmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v1/projects/{Uri.EscapeDataString(projectId)}/environments/slug/{Uri.EscapeDataString(environmentSlug)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectEnvironment>(response, "environment");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get environment", e);
    }
  }

  public async Task<ProjectEnvironment> UpdateAsync(UpdateEnvironmentOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v1/projects/{Uri.EscapeDataString(options.ProjectId)}/environments/{Uri.EscapeDataString(options.EnvironmentId)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectEnvironment>(response, "environment");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update environment", e);
    }
  }

  public async Task<ProjectEnvironment> DeleteAsync(string projectId, string environmentId, bool hardDelete = false)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      if (string.IsNullOrEmpty(environmentId)) throw new InfisicalException("EnvironmentId is required");
      var query = new QueryBuilder().Add("hardDelete", hardDelete).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v1/projects/{Uri.EscapeDataString(projectId)}/environments/{Uri.EscapeDataString(environmentId)}", null, query).ConfigureAwait(false);
      return ResponseReader.Entity<ProjectEnvironment>(response, "environment");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete environment", e);
    }
  }

  public async Task<ProjectEnvironment> RestoreAsync(string projectId, string environmentId)
  {
    try
    {
      if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
      if (string.IsNullOrEmpty(environmentId)) throw new InfisicalException("EnvironmentId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"/api/v1/projects/{Uri.EscapeDataString(projectId)}/environments/{Uri.EscapeDataString(environmentId)}/restore").ConfigureAwait(false);
      return ResponseReader.Entity<ProjectEnvironment>(response, "environment");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to restore environment", e);
    }
  }
}

public class FoldersClient
{
  private readonly ApiClient _apiClient;

  public FoldersClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  public async Task<SecretFolder> CreateAsync(CreateFolderOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v2/folders", options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretFolder>(response, "folder");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create folder", e);
    }
  }

  public async Task<SecretFolder[]> ListAsync(ListFoldersOptions options)
  {
    try
    {
      options.Validate();
      var query = new QueryBuilder()
        .Add("projectId", options.ProjectId)
        .Add("environment", options.EnvironmentSlug)
        .Add("path", options.Path)
        .Add("recursive", options.Recursive)
        .Build();

      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v2/folders", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretFolder>(response, "folders");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list folders", e);
    }
  }

  public async Task<SecretFolder> GetByIdAsync(string folderId)
  {
    try
    {
      if (string.IsNullOrEmpty(folderId)) throw new InfisicalException("FolderId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v2/folders/{Uri.EscapeDataString(folderId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretFolder>(response, "folder");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get folder", e);
    }
  }

  public async Task<SecretFolder> UpdateAsync(UpdateFolderOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v2/folders/{Uri.EscapeDataString(options.FolderId)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretFolder>(response, "folder");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update folder", e);
    }
  }

  public async Task<SecretFolder> DeleteAsync(DeleteFolderOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v2/folders/{Uri.EscapeDataString(options.FolderIdOrName)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretFolder>(response, "folder");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete folder", e);
    }
  }
}

public class SecretImportsClient
{
  private readonly ApiClient _apiClient;

  public SecretImportsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  public async Task<SecretImportInfo> CreateAsync(CreateSecretImportOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, "/api/v2/secret-imports", options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretImportInfo>(response, "secretImport");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create secret import", e);
    }
  }

  public async Task<SecretImportInfo[]> ListAsync(SecretImportLocation location)
  {
    try
    {
      location.Validate();
      var query = new QueryBuilder()
        .Add("projectId", location.ProjectId)
        .Add("environment", location.EnvironmentSlug)
        .Add("path", location.Path)
        .Build();

      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, "/api/v2/secret-imports", null, query, true).ConfigureAwait(false);
      return ResponseReader.List<SecretImportInfo>(response, "secretImports");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list secret imports", e);
    }
  }

  public async Task<SecretImportInfo> GetByIdAsync(string secretImportId)
  {
    try
    {
      if (string.IsNullOrEmpty(secretImportId)) throw new InfisicalException("SecretImportId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"/api/v2/secret-imports/{Uri.EscapeDataString(secretImportId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretImportInfo>(response, "secretImport");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get secret import", e);
    }
  }

  public async Task<SecretImportInfo> UpdateAsync(UpdateSecretImportOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"/api/v2/secret-imports/{Uri.EscapeDataString(options.SecretImportId)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretImportInfo>(response, "secretImport");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update secret import", e);
    }
  }

  public async Task<SecretImportInfo> DeleteAsync(string secretImportId, SecretImportLocation location)
  {
    try
    {
      if (string.IsNullOrEmpty(secretImportId)) throw new InfisicalException("SecretImportId is required");
      location.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"/api/v2/secret-imports/{Uri.EscapeDataString(secretImportId)}", location).ConfigureAwait(false);
      return ResponseReader.Entity<SecretImportInfo>(response, "secretImport");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete secret import", e);
    }
  }

  /// <summary>Re-syncs a replicated secret import.</summary>
  public async Task ResyncReplicationAsync(string secretImportId, SecretImportLocation location)
  {
    try
    {
      if (string.IsNullOrEmpty(secretImportId)) throw new InfisicalException("SecretImportId is required");
      location.Validate();
      await _apiClient.SendAsync<JsonElement>(HttpMethod.Post, $"/api/v2/secret-imports/{Uri.EscapeDataString(secretImportId)}/replication-resync", location).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to resync secret import replication", e);
    }
  }
}

public class SecretTagsClient
{
  private readonly ApiClient _apiClient;

  public SecretTagsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Base(string projectId)
  {
    if (string.IsNullOrEmpty(projectId)) throw new InfisicalException("ProjectId is required");
    return $"/api/v1/projects/{Uri.EscapeDataString(projectId)}/tags";
  }

  public async Task<SecretTag> CreateAsync(SecretTagOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Base(options.ProjectId), options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretTag>(response, "workspaceTag", "tag");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create tag", e);
    }
  }

  public async Task<SecretTag[]> ListAsync(string projectId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Base(projectId), null, null, true).ConfigureAwait(false);
      return ResponseReader.List<SecretTag>(response, "workspaceTags", "tags");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list tags", e);
    }
  }

  public async Task<SecretTag> GetByIdAsync(string projectId, string tagId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Base(projectId)}/{Uri.EscapeDataString(tagId)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretTag>(response, "tag");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get tag", e);
    }
  }

  public async Task<SecretTag> GetBySlugAsync(string projectId, string tagSlug)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Base(projectId)}/slug/{Uri.EscapeDataString(tagSlug)}", null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<SecretTag>(response, "tag");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get tag", e);
    }
  }

  public async Task<SecretTag> UpdateAsync(SecretTagOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.TagId)) throw new InfisicalException("TagId is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, $"{Base(options.ProjectId)}/{Uri.EscapeDataString(options.TagId!)}", options).ConfigureAwait(false);
      return ResponseReader.Entity<SecretTag>(response, "workspaceTag", "tag");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update tag", e);
    }
  }

  public async Task<SecretTag> DeleteAsync(string projectId, string tagId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, $"{Base(projectId)}/{Uri.EscapeDataString(tagId)}").ConfigureAwait(false);
      return ResponseReader.Entity<SecretTag>(response, "workspaceTag", "tag");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete tag", e);
    }
  }
}
