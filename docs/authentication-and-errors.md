# Authentication and error handling

## Creating a client

```csharp
var settings = new InfisicalSdkSettingsBuilder()
  .WithHostUri("https://app.infisical.com") // or your self-hosted instance
  .Build();
var client = new InfisicalClient(settings);
```

One client holds one access token. Create a separate client per identity.

## Machine identities

Log in once; the SDK stores the access token and sends it with every following call.

```csharp
await client.Auth().UniversalAuth().LoginAsync(clientId, clientSecret);
await client.Auth().LdapAuth().LoginAsync(identityId, username, password);
```

The other methods (AWS, Azure, GCP, Kubernetes, OIDC, JWT, OCI, Alibaba Cloud, TLS certificate) are available
through `client.Auth()` in the same way. Each login returns the credential with `AccessToken` and `ExpiresIn`.

Access tokens expire. The SDK does not refresh them on its own: log in again, or renew the token before it expires:

```csharp
await client.Auth().RenewTokenAsync(accessToken);
```

## An existing token

If you already have a token (a Token Auth token, or the one returned by `Instance().BootstrapAsync(...)`):

```csharp
client.Auth().SetAccessToken(token);
```

## User login

A few endpoints only accept a user login and answer machine identity tokens with `403`: listing or reading
organizations, SCIM tokens and creating certificate inventory views.

```csharp
await client.Auth().UserAuth().LoginAsync(email, password, organizationId);
```

Users with MFA are not supported. User tokens are short lived and are not refreshed by the SDK.

## Certificate Manager project

Calls that address a Certificate Manager resource by ID (certificates, CAs, profiles, signers, ...) need to know the
project. If your organization has several Certificate Manager projects, select one:

```csharp
client.SetCertManagerProject(projectId);
```

Without it, the server falls back to its default Certificate Manager project.

## Errors

Failed calls throw `InfisicalException`. The message starts with the operation (`Error during POST request: ...`) and
contains the HTTP status and the message sent by the API. The original exception (usually a `HttpRequestException`)
is available as `InnerException`.

```csharp
try
{
  await client.Projects().GetAsync(projectId);
}
catch (InfisicalException ex)
{
  Console.WriteLine(ex.Message);
  Console.WriteLine(ex.InnerException?.Message);
}
```

Typical causes:

| Status | Meaning |
| --- | --- |
| 401 | the token is missing, expired or revoked: log in again |
| 403 | the identity lacks the permission, the endpoint is user-only (see above), or the feature needs an Enterprise license |
| 404 | the resource does not exist, or it belongs to another Certificate Manager project |
| 422 | the request is invalid (e.g. an unknown enum value); use the provided constants such as `SecretSyncDestinations.*` |

## Retries

The typed clients retry only transient failures (network errors, 5xx and 408), so client errors like 404 are reported
immediately. The original secret and PKI subscriber calls keep their previous retry behavior.
