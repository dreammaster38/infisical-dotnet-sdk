using Infisical.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ScimToken : ApiModel
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("ttlDays")]
  public int? TtlDays { get; set; }

  [JsonPropertyName("orgId")]
  public string? OrgId { get; set; }

  [JsonPropertyName("createdAt")]
  public string? CreatedAt { get; set; }
}

public class OrganizationAdminProjects
{
  public OrganizationAdminProjects(Project[] projects, int? count)
  {
    Projects = projects;
    Count = count;
  }

  public Project[] Projects { get; }
  public int? Count { get; }
}
