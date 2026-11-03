# Changelog

All notable changes to the Infisical .NET SDK are documented in this file. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows [Semantic Versioning](https://semver.org/).

The version of a release is taken from the git tag (`<major>.<minor>.<patch>`) by the release workflow; the `Version`
in `Infisical.Sdk.csproj` is only the fallback for local builds.

## [4.0.0] - 2026-11-02

Typed clients for most of the documented Infisical API. Existing calls keep working, but the release is a major
version because of the breaking change below.

### Breaking changes

- **New public types in the global namespace.** Like the existing model classes (`Secret`, `SecretType`, ...), the model
  and option classes of the new clients (`Project`, `Group`, `Identity`, `Organization`, `Alert`, `Folder`, `Signer`,
  `Gateway`, ...) are declared in the global namespace. A type in the global namespace takes precedence over a type
  that a file imports with a `using` directive at the top of the file, so code that has its own type with one of these
  names can silently bind to the SDK's type instead and fail to compile (e.g. `CS1061`), or behave differently if the
  members happen to match. Ways to resolve it, per file:
  - put the `using` directive inside the `namespace` declaration (`namespace MyApp.Services { using MyApp.Models; ... }`),
  - use the fully qualified name (`new MyApp.Models.Project()`), or
  - add an alias inside the namespace (`using Project = MyApp.Models.Project;`).

  Code that does not define types with these names is not affected.

### Added

- **Authentication:** logins for AWS, Azure, GCP, Kubernetes, OIDC, JWT, OCI, Alibaba Cloud and TLS certificate auth,
  a user login (`Auth().UserAuth()`, email and password, no MFA), `Auth().SetAccessToken(...)`, and token renew / revoke.
- **Secrets:** batch create / update / delete, move and duplicate (v4 API).
- **Platform:** `Projects()`, `Environments()`, `Folders()`, `SecretImports()`, `SecretTags()`, `ProjectTemplates()`.
- **Identities and access:** `Identities()` (CRUD, search, and attach / update / remove for all 11 auth methods, Universal
  Auth client secrets, Token Auth tokens), `Groups()`, `ProjectAccess()` (project roles and memberships),
  `IdentityPrivileges()`.
- **Integrations:** `AppConnections()`, `SecretSyncs()` and `SecretRotations()`, each generic over all destinations / types
  with constants (`AppConnectionTypes`, `SecretSyncDestinations`, `SecretRotationTypes`).
- **Secret tooling:** `DynamicSecrets()` (incl. leases), `Kms()`, `SharedSecrets()`, `SecretValidationRules()`,
  `SecretScanning()`, `AgentVault()`, `ProxiedServices()`.
- **Certificate Manager:** `Certificates()`, `CertificateProfiles()`, `CertificatePolicies()`, `CertificateAuthorities()`,
  `CertificateInventory()`, `CertificateCleanup()`, `CertificateTemplates()` (legacy), `PkiDiscovery()`, `Signers()` (code
  signing), `Alerts()`; `InfisicalClient.SetCertManagerProject(...)` to select the project for calls that address a
  resource by ID.
- **Infrastructure:** `Gateways()`, `GatewayPools()`, `KmipServers()`.
- **Organization:** `Organizations()` (organizations, memberships, `Roles()`, `SubOrganizations()`, `Sso()`, `Scim()`,
  audit logs), `OrganizationAdmin()`, `Instance()` (status and bootstrap).
- `Raw()` to call any endpoint that has no typed client, with the client's authentication.
- Constants for enum-like values (`ProjectTypes`, `CertificateKeyUsages`, `CertificateExtendedKeyUsages`,
  `CertificateRevocationReasons`, `SigningAlgorithms`, ...).
- Response models keep everything the API returns that they do not model in `AdditionalData`.
- Unit tests (`src/Infisical.Sdk.UnitTests`) that run the clients against an in-process fake server, and live smoke
  tests that run against a real instance when `INFISICAL_LIVE_*` is set.

### Changed

- The SDK sends a `User-Agent: infisical-dotnet-sdk` header.
- `QueryBuilder.Add` now writes booleans as `true` / `false` instead of `True` / `False`.
- The new clients retry only transient failures (network errors, 5xx, 408), so a 4xx answer is reported immediately.
  The existing secret and PKI subscriber calls keep their previous retry behavior.
- The new clients read JSON leniently: a number or boolean is accepted for a text field and a numeric string for a
  number field.

- The package now ships IntelliSense documentation (XML comments), Source Link information and embedded symbols, and
  CI builds are deterministic.
- Documentation: a guide for authentication and error handling (`docs/authentication-and-errors.md`), more examples
  in the README, and the sample project `src/Infisical.Sdk.Test` uses the new clients.

### Notes

- Endpoints that need an Enterprise license (e.g. groups, custom roles, project templates, gateway pools, secret
  rotations, secret scanning, SCIM tokens) were verified against the backend sources but not run against a licensed
  instance.
- A few endpoints only accept a user login and reject machine identity tokens (e.g. listing organizations, SCIM tokens,
  creating certificate inventory views); use `Auth().UserAuth()` for those.

## [3.0.4]

Last release before this changelog was introduced: secrets (list, get, create, update, delete), PKI subscribers,
Universal Auth and LDAP auth.
