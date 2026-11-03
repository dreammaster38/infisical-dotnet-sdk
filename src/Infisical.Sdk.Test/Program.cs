namespace Infisical.Sdk.Samples;

using System.Text.Json;
using Infisical.Sdk;
using Infisical.Sdk.Model;

/// <summary>
/// Sample that logs in with a machine identity and walks through the secret calls and a few of the newer clients.
/// Required: INFISICAL_MACHINE_IDENTITY_CLIENT_ID, INFISICAL_MACHINE_IDENTITY_CLIENT_SECRET, INFISICAL_PROJECT_ID.
/// Optional: INFISICAL_URL (default http://localhost:8080), LDAP_IDENTITY_ID / LDAP_USERNAME / LDAP_PASSWORD.
/// </summary>
internal class Program
{
  private static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true };

  private static string RandomString(int length) =>
    new(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", length)
      .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());

  private static void Print(string title, object? value) =>
    Console.WriteLine($"\n{title}:\n{JsonSerializer.Serialize(value, Pretty)}");

  private static async Task<int> Main()
  {
    var clientId = Environment.GetEnvironmentVariable("INFISICAL_MACHINE_IDENTITY_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("INFISICAL_MACHINE_IDENTITY_CLIENT_SECRET");
    var projectId = Environment.GetEnvironmentVariable("INFISICAL_PROJECT_ID");

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(projectId))
    {
      Console.WriteLine("INFISICAL_MACHINE_IDENTITY_CLIENT_ID, INFISICAL_MACHINE_IDENTITY_CLIENT_SECRET and INFISICAL_PROJECT_ID must be set");
      return 1;
    }

    var settings = new InfisicalSdkSettingsBuilder()
      .WithHostUri(Environment.GetEnvironmentVariable("INFISICAL_URL") ?? "http://localhost:8080")
      .Build();
    var client = new InfisicalClient(settings);

    try
    {
      // 1. log in with a machine identity (Universal Auth)
      var credential = await client.Auth().UniversalAuth().LoginAsync(clientId, clientSecret);
      Console.WriteLine($"Logged in, the token is valid for {credential.ExpiresIn} seconds");

      // 2. optionally switch to LDAP auth with a second client
      var ldapIdentityId = Environment.GetEnvironmentVariable("LDAP_IDENTITY_ID");
      var ldapUsername = Environment.GetEnvironmentVariable("LDAP_USERNAME");
      var ldapPassword = Environment.GetEnvironmentVariable("LDAP_PASSWORD");
      if (!string.IsNullOrEmpty(ldapIdentityId) && !string.IsNullOrEmpty(ldapUsername) && !string.IsNullOrEmpty(ldapPassword))
      {
        var ldapClient = new InfisicalClient(settings);
        await ldapClient.Auth().LdapAuth().LoginAsync(ldapIdentityId, ldapUsername, ldapPassword);
        Console.WriteLine("LDAP login succeeded, using the LDAP client from here on");
        client = ldapClient;
      }

      // 3. project, environments and folders (new clients)
      var project = await client.Projects().GetAsync(projectId);
      Console.WriteLine($"Project: {project.Name}");
      await client.Folders().CreateAsync(new CreateFolderOptions { ProjectId = projectId, EnvironmentSlug = "dev", Name = "test", Path = "/" });

      // 4. secrets: create, list, get, update, delete
      var name = $".NET-SDK-TEST-{RandomString(32)}";
      Print("Create secret", await client.Secrets().CreateAsync(new CreateSecretOptions
      {
        SecretName = name, EnvironmentSlug = "dev", SecretPath = "/test", SecretValue = RandomString(10), ProjectId = projectId,
      }));

      Print("List secrets", await client.Secrets().ListAsync(new ListSecretsOptions
      {
        EnvironmentSlug = "dev", SecretPath = "/test", Recursive = true, ProjectId = projectId,
      }));

      Print("Get secret", await client.Secrets().GetAsync(new GetSecretOptions
      {
        SecretName = name, EnvironmentSlug = "dev", SecretPath = "/test", ProjectId = projectId,
      }));

      Print("Update secret", await client.Secrets().UpdateAsync(new UpdateSecretOptions
      {
        SecretName = name, NewSecretName = $"{name}-updated", NewSecretValue = RandomString(10),
        EnvironmentSlug = "dev", SecretPath = "/test", ProjectId = projectId,
      }));

      Print("Delete secret", await client.Secrets().DeleteAsync(new DeleteSecretOptions
      {
        SecretName = $"{name}-updated", EnvironmentSlug = "dev", SecretPath = "/test", ProjectId = projectId,
      }));

      return 0;
    }
    catch (InfisicalException ex)
    {
      // every failed call is reported as InfisicalException; the cause (e.g. HttpRequestException) is the inner exception
      Console.WriteLine($"Infisical call failed: {ex.Message}");
      if (ex.InnerException != null) Console.WriteLine($"  cause: {ex.InnerException.Message}");
      return 1;
    }
  }
}
