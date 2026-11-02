using Xunit;

namespace Infisical.Sdk.Tests;

public class IntegrationTests
{
  [Fact]
  public async Task Secret_sync_create_posts_to_the_destination_path()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/secret-syncs/aws-parameter-store", "{\"secretSync\":{\"id\":\"sy1\",\"name\":\"n\",\"destination\":\"aws-parameter-store\",\"destinationConfig\":{\"region\":\"eu-central-1\"}}}");

    var sync = await h.Client.SecretSyncs().CreateAsync(new CreateSecretSyncOptions
    {
      Destination = SecretSyncDestinations.AWSParameterStore,
      Name = "n",
      ProjectId = "p1",
      ConnectionId = "cn1",
      EnvironmentSlug = "dev",
      SyncOptions = new SecretSyncOptions
      {
        InitialSyncBehavior = SecretSyncInitialSyncBehaviors.OverwriteDestination,
        Extra = new Dictionary<string, object?> { ["syncSecretMetadataAsTags"] = true }
      },
      DestinationConfig = new Dictionary<string, object?> { ["region"] = "eu-central-1", ["path"] = "/app/" }
    });

    Assert.Equal("sy1", sync.Id);
    Assert.Equal("eu-central-1", sync.DestinationConfig!.Value.GetProperty("region").GetString());
    var body = h.Last.Json;
    Assert.Equal("dev", body.GetProperty("environment").GetString());
    Assert.True(body.GetProperty("syncOptions").GetProperty("syncSecretMetadataAsTags").GetBoolean());
    Assert.Equal("/app/", body.GetProperty("destinationConfig").GetProperty("path").GetString());
  }

  [Fact]
  public async Task Secret_sync_actions_hit_their_endpoints()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/secret-syncs/github/sy1/sync-secrets", "{\"secretSync\":{\"id\":\"sy1\",\"syncStatus\":\"pending\"}}");
    h.On("POST", "/api/v1/secret-syncs/github/sy1/import-secrets", "{\"secretSync\":{\"id\":\"sy1\"}}");
    h.On("DELETE", "/api/v1/secret-syncs/github/sy1", "{\"secretSync\":{\"id\":\"sy1\"}}");

    Assert.Equal("pending", (await h.Client.SecretSyncs().SyncSecretsAsync("github", "sy1")).SyncStatus);
    await h.Client.SecretSyncs().ImportSecretsAsync("github", "sy1", SecretSyncImportBehaviors.PrioritizeSource);
    Assert.Equal("prioritize-source", h.Last.Query["importBehavior"]);
    await h.Client.SecretSyncs().DeleteAsync("github", "sy1", removeSecrets: true);
    Assert.Equal("true", h.Last.Query["removeSecrets"]);
  }

  [Fact]
  public async Task Secret_syncs_list_for_a_project()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/secret-syncs", "{\"secretSyncs\":[{\"id\":\"a\",\"destination\":\"github\"},{\"id\":\"b\",\"destination\":\"vercel\"}]}");

    var syncs = await h.Client.SecretSyncs().ListAsync("p1");

    Assert.Equal(new[] { "github", "vercel" }, syncs.Select(s => s.Destination));
    Assert.Equal("p1", h.Last.Query["projectId"]);
  }

  [Fact]
  public void All_secret_sync_destinations_are_defined()
  {
    Assert.Equal("aws-parameter-store", SecretSyncDestinations.AWSParameterStore);
    Assert.Equal("github", SecretSyncDestinations.GitHub);
    Assert.True(typeof(SecretSyncDestinations).GetFields().Length >= 50);
  }

  [Fact]
  public async Task App_connection_create_sends_method_and_credentials()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/app-connections/aws", "{\"appConnection\":{\"id\":\"ac1\",\"name\":\"aws1\",\"app\":\"aws\",\"method\":\"access-key\",\"credentials\":{\"accessKeyId\":\"AK\"}}}");

    var connection = await h.Client.AppConnections().CreateAsync(new CreateAppConnectionOptions
    {
      App = AppConnectionTypes.AWS,
      Name = "aws1",
      Method = "access-key",
      ProjectId = "p1",
      Credentials = new Dictionary<string, object?> { ["accessKeyId"] = "AK", ["secretAccessKey"] = "SK" }
    });

    Assert.Equal("AK", connection.Credentials!.Value.GetProperty("accessKeyId").GetString());
    Assert.Equal("SK", h.Last.Json.GetProperty("credentials").GetProperty("secretAccessKey").GetString());
    Assert.False(h.Last.Json.TryGetProperty("app", out _)); // the app is part of the path
  }

  [Fact]
  public async Task App_connections_list_available_for_a_project()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/app-connections/github/available", "{\"appConnections\":[{\"id\":\"c1\",\"name\":\"gh\"}]}");

    var available = await h.Client.AppConnections().ListAvailableAsync(AppConnectionTypes.GitHub, "p1");

    Assert.Equal("gh", available[0].Name);
    Assert.Equal("p1", h.Last.Query["projectId"]);
  }

  [Fact]
  public async Task Secret_rotation_create_includes_schedule_parameters_and_mapping()
  {
    using var h = new Harness();
    h.On("POST", "/api/v2/secret-rotations/postgres-credentials", "{\"secretRotation\":{\"id\":\"ro1\",\"name\":\"pg\",\"rotationInterval\":30}}");

    var rotation = await h.Client.SecretRotations().CreateAsync(new CreateSecretRotationOptions
    {
      Type = SecretRotationTypes.PostgresCredentials,
      Name = "pg",
      ProjectId = "p1",
      ConnectionId = "cn1",
      EnvironmentSlug = "dev",
      RotationInterval = 30,
      RotateAtUtc = new RotateAtUtc { Hours = 2, Minutes = 30 },
      Parameters = new Dictionary<string, object?> { ["username1"] = "a", ["username2"] = "b" },
      SecretsMapping = new Dictionary<string, object?> { ["username"] = "DB_USER", ["password"] = "DB_PASS" }
    });

    Assert.Equal(30, rotation.RotationInterval);
    var body = h.Last.Json;
    Assert.Equal(30, body.GetProperty("rotateAtUtc").GetProperty("minutes").GetInt32());
    Assert.Equal("DB_USER", body.GetProperty("secretsMapping").GetProperty("username").GetString());
  }

  [Fact]
  public async Task Secret_rotation_generated_credentials_expose_the_active_index()
  {
    using var h = new Harness();
    h.On("GET", "/api/v2/secret-rotations/postgres-credentials/ro1/generated-credentials", "{\"generatedCredentials\":[{\"username\":\"a\"},{\"username\":\"b\"}],\"activeIndex\":1,\"rotationId\":\"ro1\"}");

    var credentials = await h.Client.SecretRotations().GetGeneratedCredentialsAsync("postgres-credentials", "ro1");

    Assert.Equal(2, credentials.GeneratedCredentials.Length);
    Assert.Equal(1, credentials.ActiveIndex);
  }

  [Fact]
  public async Task Secret_rotation_delete_passes_flags_as_query()
  {
    using var h = new Harness();
    h.On("DELETE", "/api/v2/secret-rotations/aws-iam-user-secret/ro1", "{\"secretRotation\":{\"id\":\"ro1\"}}");

    await h.Client.SecretRotations().DeleteAsync(new DeleteSecretRotationOptions { Type = "aws-iam-user-secret", RotationId = "ro1", DeleteSecrets = true });

    Assert.Equal("true", h.Last.Query["deleteSecrets"]);
    Assert.Equal("false", h.Last.Query["revokeGeneratedCredentials"]);
  }

  [Fact]
  public async Task Proxied_service_credentials_default_to_header_rewrite()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/proxied-services", "{\"service\":{\"id\":\"px1\",\"name\":\"api\"}}");

    await h.Client.ProxiedServices().CreateAsync(new CreateProxiedServiceOptions
    {
      ProjectId = "p1",
      EnvironmentSlug = "dev",
      Name = "api",
      HostPattern = "api.example.com",
      Credentials = new[] { new ProxiedServiceCredential { SecretKey = "API_KEY", HeaderName = "Authorization", HeaderPrefix = "Bearer " } }
    });

    Assert.Equal("header-rewrite", h.Last.Json.GetProperty("credentials")[0].GetProperty("role").GetString());
  }
}
