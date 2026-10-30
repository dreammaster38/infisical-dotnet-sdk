<h1 align="center">
  <img width="300" src="/img/logoname-white.svg#gh-dark-mode-only" alt="infisical">
</h1>
<p align="center">
  <p align="center"><b>Infisical .NET SDK</b></p>
<h4 align="center">
|
  <a href="https://infisical.com/docs/sdks/languages/dotnet">Documentation</a> |
  <a href="https://www.infisical.com">Website</a> |
  <a href="https://infisical.com/slack">Slack</a> |
</h4>

<h4 align="center">
  <a href="https://github.com/Infisical/infisical-dotnet-sdk/blob/main/LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="Infisical SDK's are released under the MIT license." />
  </a>
  <a href="https://infisical.com/slack">
    <img src="https://img.shields.io/badge/chat-on%20Slack-blueviolet" alt="Slack community channel" />
  </a>
  <a href="https://twitter.com/infisical">
    <img src="https://img.shields.io/twitter/follow/infisical?label=Follow" alt="Infisical Twitter" />
  </a>
</h4>

## Introduction

**[Infisical](https://infisical.com)** is the open source secret management platform that teams use to centralize their secrets like API keys, database credentials, and configurations.

If you’re working with .NET, the official Infisical .NET SDK package is the easiest way to fetch and work with secrets for your application. You can read the documentation [here](https://infisical.com/docs/sdks/languages/dotnet).

## Documentation
You can find the documentation for the .NET SDK on our [SDK documentation page](https://infisical.com/docs/sdks/languages/dotnet).

## Supported APIs

Besides secrets and PKI subscribers, the SDK covers the following areas of the Infisical API. All clients are reachable from `InfisicalClient`:

| Accessor | Covers |
| --- | --- |
| `Auth()` | Logins: Universal, LDAP, AWS, Azure, GCP, Kubernetes, OIDC, JWT, OCI, Alibaba Cloud, TLS certificate; token renew / revoke |
| `Secrets()` | list / get / create / update / delete, **batch create / update / delete, move, duplicate** |
| `Projects()` | create, list, get (by id / slug), update, delete |
| `Environments()` | create, get by slug, update, delete, restore |
| `Folders()` | create, list, get, update, delete |
| `SecretImports()` | create, list, get, update, delete, replication resync |
| `SecretTags()` | create, list, get (by id / slug), update, delete |
| `Identities()` | machine identities (CRUD, search) and `Identities().Auth()` to attach / update / remove every auth method, manage Universal Auth client secrets and Token Auth tokens |
| `DynamicSecrets()` | dynamic secrets (CRUD) and leases (create, get, list, renew, revoke) |
| `Groups()` | groups (CRUD), users and machine identities of a group |
| `Kms()` | KMS keys (CRUD, rotate), encrypt / decrypt, sign / verify, public key |
| `SharedSecrets()` | create, list, get, access, delete |
| `ProjectAccess()` | project roles, project memberships of users, machine identities and groups |
| `SecretSyncs()` | secret syncs for all destinations (`SecretSyncDestinations.*`): list, get, create, update, delete, sync / import / remove secrets, check destination |
| `Certificates()` | issue, get, bundle / private key, PKCS#12, search, renew, revoke, import, certificate requests |
| `CertificateProfiles()` / `CertificatePolicies()` | CRUD for certificate profiles and policies |
| `CertificateAuthorities()` | internal CAs: CRUD, CSR, renew, sign intermediate, CRLs, auto-renewal, signing config |
| `Pki()` | PKI subscribers |
| `Raw()` | any other endpoint (app connections, secret syncs, rotations, audit logs, ...) |

```csharp
var client = new InfisicalClient(new InfisicalSdkSettingsBuilder().WithHostUri("https://app.infisical.com").Build());
await client.Auth().UniversalAuth().LoginAsync(clientId, clientSecret);

var project = await client.Projects().CreateAsync(new CreateProjectOptions { ProjectName = "My project" });
await client.Folders().CreateAsync(new CreateFolderOptions { ProjectId = project.Id, EnvironmentSlug = "dev", Name = "backend" });

await client.Secrets().BatchCreateAsync(new BatchCreateSecretsOptions
{
  ProjectId = project.Id,
  EnvironmentSlug = "dev",
  SecretPath = "/backend",
  Secrets = new[]
  {
    new BatchSecretInput { SecretKey = "DB_URL", SecretValue = "postgres://..." },
    new BatchSecretInput { SecretKey = "API_KEY", SecretValue = "abc" },
  }
});

// sync the secrets of a folder to AWS Parameter Store
await client.SecretSyncs().CreateAsync(new CreateSecretSyncOptions
{
  Destination = SecretSyncDestinations.AWSParameterStore,
  Name = "my-app-sync",
  ProjectId = project.Id,
  ConnectionId = awsConnectionId,
  EnvironmentSlug = "dev",
  SecretPath = "/backend",
  SyncOptions = new SecretSyncOptions { InitialSyncBehavior = SecretSyncInitialSyncBehaviors.OverwriteDestination },
  DestinationConfig = new() { ["region"] = "eu-central-1", ["path"] = "/my-app/" }
});

// request a certificate from a certificate profile
var cert = await client.Certificates().IssueAsync(new IssueCertificateRequest
{
  ProfileId = profileId,
  Attributes = new CertificateAttributes { CommonName = "service.example.com", Ttl = "90d" }
});
Console.WriteLine(cert.Material?.Certificate);

// endpoints without a typed client
var auditLogs = await client.Raw().GetAsync("/api/v1/organization/audit-logs", new Dictionary<string, string> { ["limit"] = "20" });
```

Response models expose the most important fields as properties; everything else the API returns is available through `AdditionalData`.

## Security

Please do not file GitHub issues or post on our public forum for security vulnerabilities, as they are public!

Infisical takes security issues very seriously. If you have any concerns about Infisical or believe you have uncovered a vulnerability, please get in touch via the e-mail address security@infisical.com. In the message, try to provide a description of the issue and ideally a way of reproducing it. The security team will get back to you as soon as possible.

Note that this security address should be used only for undisclosed vulnerabilities. Please report any security problems to us before disclosing it publicly.