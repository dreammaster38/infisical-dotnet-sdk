using Infisical.Sdk;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------------------------
// Projects
// ---------------------------------------------------------------------------------------------

public class ProjectEnvironment : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string Slug { get; set; } = string.Empty;

  [JsonPropertyName("position")]
  public int? Position { get; set; }
}

public class Project : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string Slug { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("type")]
  public string? Type { get; set; }

  [JsonPropertyName("orgId")]
  public string? OrgId { get; set; }

  [JsonPropertyName("autoCapitalization")]
  public bool? AutoCapitalization { get; set; }

  [JsonPropertyName("environments")]
  public ProjectEnvironment[]? Environments { get; set; }
}

public class CreateProjectOptions
{
  [JsonPropertyName("projectName")]
  public string ProjectName { get; init; } = string.Empty;

  [JsonPropertyName("projectDescription")]
  public string? ProjectDescription { get; init; }

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("kmsKeyId")]
  public string? KmsKeyId { get; init; }

  [JsonPropertyName("template")]
  public string? Template { get; init; }

  /// <summary>Project type, e.g. "secret-manager", "cert-manager", "kms", "ssh".</summary>
  [JsonPropertyName("type")]
  public string? Type { get; init; }

  [JsonPropertyName("shouldCreateDefaultEnvs")]
  public bool? ShouldCreateDefaultEnvs { get; init; }

  [JsonPropertyName("hasDeleteProtection")]
  public bool? HasDeleteProtection { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectName))
    {
      throw new InfisicalException("ProjectName is required");
    }
  }
}

public class UpdateProjectOptions
{
  [JsonIgnore]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("autoCapitalization")]
  public bool? AutoCapitalization { get; init; }

  [JsonPropertyName("hasDeleteProtection")]
  public bool? HasDeleteProtection { get; init; }

  [JsonPropertyName("secretSharing")]
  public bool? SecretSharing { get; init; }

  [JsonPropertyName("pitVersionLimit")]
  public int? PitVersionLimit { get; init; }

  [JsonPropertyName("auditLogsRetentionDays")]
  public int? AuditLogsRetentionDays { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId))
    {
      throw new InfisicalException("ProjectId is required");
    }
  }
}

public class ListProjectsOptions
{
  public bool IncludeRoles { get; init; } = false;

  /// <summary>Optional project type filter, e.g. "secret-manager".</summary>
  public string? Type { get; init; }
}

// ---------------------------------------------------------------------------------------------
// Environments
// ---------------------------------------------------------------------------------------------

public class CreateEnvironmentOptions
{
  [JsonIgnore]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("slug")]
  public string Slug { get; init; } = string.Empty;

  [JsonPropertyName("position")]
  public int? Position { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
    if (string.IsNullOrEmpty(Slug)) throw new InfisicalException("Slug is required");
  }
}

public class UpdateEnvironmentOptions
{
  [JsonIgnore]
  public string ProjectId { get; init; } = string.Empty;

  /// <summary>ID of the environment to update.</summary>
  [JsonIgnore]
  public string EnvironmentId { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string? Name { get; init; }

  [JsonPropertyName("slug")]
  public string? Slug { get; init; }

  [JsonPropertyName("position")]
  public int? Position { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentId)) throw new InfisicalException("EnvironmentId is required");
  }
}

// ---------------------------------------------------------------------------------------------
// Folders
// ---------------------------------------------------------------------------------------------

public class SecretFolder : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("path")]
  public string? Path { get; set; }

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("envId")]
  public string? EnvironmentId { get; set; }

  [JsonPropertyName("parentId")]
  public string? ParentId { get; set; }
}

public class CreateFolderOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("path")]
  public string Path { get; init; } = "/";

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
  }
}

public class UpdateFolderOptions
{
  [JsonIgnore]
  public string FolderId { get; init; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  /// <summary>The (new) name of the folder.</summary>
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("path")]
  public string Path { get; init; } = "/";

  [JsonPropertyName("description")]
  public string? Description { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(FolderId)) throw new InfisicalException("FolderId is required");
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (string.IsNullOrEmpty(Name)) throw new InfisicalException("Name is required");
  }
}

public class ListFoldersOptions
{
  public string ProjectId { get; init; } = string.Empty;
  public string EnvironmentSlug { get; init; } = string.Empty;
  public string Path { get; init; } = "/";
  public bool Recursive { get; init; } = false;

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
  }
}

public class DeleteFolderOptions
{
  /// <summary>ID or name of the folder.</summary>
  [JsonIgnore]
  public string FolderIdOrName { get; init; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("path")]
  public string Path { get; init; } = "/";

  [JsonPropertyName("forceDelete")]
  public bool? ForceDelete { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(FolderIdOrName)) throw new InfisicalException("FolderIdOrName is required");
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
  }
}

// ---------------------------------------------------------------------------------------------
// Secret imports
// ---------------------------------------------------------------------------------------------

public class SecretImportInfo : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("importPath")]
  public string ImportPath { get; set; } = string.Empty;

  [JsonPropertyName("position")]
  public int? Position { get; set; }

  [JsonPropertyName("isReplication")]
  public bool? IsReplication { get; set; }
}

public class SecretImportSource
{
  [JsonPropertyName("sourceProjectId")]
  public string? SourceProjectId { get; init; }

  [JsonPropertyName("environment")]
  public string? EnvironmentSlug { get; init; }

  [JsonPropertyName("path")]
  public string? Path { get; init; }

  [JsonPropertyName("position")]
  public int? Position { get; init; }
}

public class CreateSecretImportOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("path")]
  public string Path { get; init; } = "/";

  /// <summary>The secrets location that is imported.</summary>
  [JsonPropertyName("import")]
  public SecretImportSource Import { get; init; } = new SecretImportSource();

  [JsonPropertyName("isReplication")]
  public bool? IsReplication { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (string.IsNullOrEmpty(Import?.EnvironmentSlug)) throw new InfisicalException("Import.EnvironmentSlug is required");
    if (string.IsNullOrEmpty(Import?.Path)) throw new InfisicalException("Import.Path is required");
  }
}

public class UpdateSecretImportOptions
{
  [JsonIgnore]
  public string SecretImportId { get; init; } = string.Empty;

  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("path")]
  public string Path { get; init; } = "/";

  [JsonPropertyName("import")]
  public SecretImportSource Import { get; init; } = new SecretImportSource();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(SecretImportId)) throw new InfisicalException("SecretImportId is required");
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
  }
}

/// <summary>Identifies the folder a secret import lives in (used for list / delete).</summary>
public class SecretImportLocation
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("path")]
  public string Path { get; init; } = "/";

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
  }
}

// ---------------------------------------------------------------------------------------------
// Secret tags
// ---------------------------------------------------------------------------------------------

public class SecretTag : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("slug")]
  public string Slug { get; set; } = string.Empty;

  [JsonPropertyName("color")]
  public string? Color { get; set; }
}

public class SecretTagOptions
{
  [JsonIgnore]
  public string ProjectId { get; init; } = string.Empty;

  /// <summary>Only used when updating.</summary>
  [JsonIgnore]
  public string? TagId { get; init; }

  [JsonPropertyName("slug")]
  public string Slug { get; init; } = string.Empty;

  [JsonPropertyName("color")]
  public string Color { get; init; } = string.Empty;
}

// ---------------------------------------------------------------------------------------------
// Batch secret operations (v4 API)
// ---------------------------------------------------------------------------------------------

public class BatchSecretInput
{
  [JsonPropertyName("secretKey")]
  public string SecretKey { get; init; } = string.Empty;

  [JsonPropertyName("secretValue")]
  public string? SecretValue { get; init; }

  [JsonPropertyName("secretComment")]
  public string? SecretComment { get; init; }

  [JsonPropertyName("skipMultilineEncoding")]
  public bool? SkipMultilineEncoding { get; init; }

  [JsonPropertyName("secretMetadata")]
  public SecretMetadata[]? Metadata { get; init; }

  [JsonPropertyName("tagIds")]
  public string[]? TagIds { get; init; }

  /// <summary>Only used by batch update: renames the secret.</summary>
  [JsonPropertyName("newSecretName")]
  public string? NewSecretName { get; init; }
}

public class BatchCreateSecretsOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("secretPath")]
  public string SecretPath { get; init; } = "/";

  [JsonPropertyName("secrets")]
  public BatchSecretInput[] Secrets { get; init; } = Array.Empty<BatchSecretInput>();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (Secrets == null || Secrets.Length == 0) throw new InfisicalException("At least one secret is required");
  }
}

public class BatchUpdateSecretsOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("secretPath")]
  public string SecretPath { get; init; } = "/";

  /// <summary>"failOnNotFound" (default) or "ignoreNotFound".</summary>
  [JsonPropertyName("mode")]
  public string? Mode { get; init; }

  [JsonPropertyName("secrets")]
  public BatchSecretInput[] Secrets { get; init; } = Array.Empty<BatchSecretInput>();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (Secrets == null || Secrets.Length == 0) throw new InfisicalException("At least one secret is required");
  }
}

public class BatchDeleteSecretInput
{
  [JsonPropertyName("secretKey")]
  public string SecretKey { get; init; } = string.Empty;

  [JsonPropertyName("type")]
  public SecretType? Type { get; init; }
}

public class BatchDeleteSecretsOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("environment")]
  public string EnvironmentSlug { get; init; } = string.Empty;

  [JsonPropertyName("secretPath")]
  public string SecretPath { get; init; } = "/";

  [JsonPropertyName("secrets")]
  public BatchDeleteSecretInput[] Secrets { get; init; } = Array.Empty<BatchDeleteSecretInput>();

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(EnvironmentSlug)) throw new InfisicalException("EnvironmentSlug is required");
    if (Secrets == null || Secrets.Length == 0) throw new InfisicalException("At least one secret is required");
  }
}

public class MoveSecretsOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("sourceEnvironment")]
  public string SourceEnvironment { get; init; } = string.Empty;

  [JsonPropertyName("sourceSecretPath")]
  public string SourceSecretPath { get; init; } = "/";

  [JsonPropertyName("destinationEnvironment")]
  public string DestinationEnvironment { get; init; } = string.Empty;

  [JsonPropertyName("destinationSecretPath")]
  public string DestinationSecretPath { get; init; } = "/";

  [JsonPropertyName("secretIds")]
  public string[] SecretIds { get; init; } = Array.Empty<string>();

  [JsonPropertyName("shouldOverwrite")]
  public bool? ShouldOverwrite { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(SourceEnvironment)) throw new InfisicalException("SourceEnvironment is required");
    if (string.IsNullOrEmpty(DestinationEnvironment)) throw new InfisicalException("DestinationEnvironment is required");
    if (SecretIds == null || SecretIds.Length == 0) throw new InfisicalException("At least one secret id is required");
  }
}

public class DuplicateSecretsOptions
{
  [JsonPropertyName("projectId")]
  public string ProjectId { get; init; } = string.Empty;

  [JsonPropertyName("sourceEnvironment")]
  public string SourceEnvironment { get; init; } = string.Empty;

  [JsonPropertyName("sourceSecretPath")]
  public string SourceSecretPath { get; init; } = "/";

  [JsonPropertyName("destinationEnvironment")]
  public string DestinationEnvironment { get; init; } = string.Empty;

  [JsonPropertyName("destinationSecretPath")]
  public string DestinationSecretPath { get; init; } = "/";

  [JsonPropertyName("secretIds")]
  public string[] SecretIds { get; init; } = Array.Empty<string>();

  [JsonPropertyName("shouldOverwrite")]
  public bool? ShouldOverwrite { get; init; }

  internal void Validate()
  {
    if (string.IsNullOrEmpty(ProjectId)) throw new InfisicalException("ProjectId is required");
    if (string.IsNullOrEmpty(SourceEnvironment)) throw new InfisicalException("SourceEnvironment is required");
    if (string.IsNullOrEmpty(DestinationEnvironment)) throw new InfisicalException("DestinationEnvironment is required");
    if (SecretIds == null || SecretIds.Length == 0) throw new InfisicalException("At least one secret id is required");
  }
}
