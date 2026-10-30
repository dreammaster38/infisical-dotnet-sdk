using System.Threading.Tasks;
using Infisical.Sdk.Api;
using Infisical.Sdk.Client;
using Infisical.Sdk.Model;

namespace Infisical.Sdk
{
  public class InfisicalClient
  {
    internal ApiClient _apiClient;
    private AuthClient _authClient;
    private SecretsClient _secretsClient;
    private PkiClient _pkiClient;
    private ProjectsClient _projects;
    private EnvironmentsClient _environments;
    private FoldersClient _folders;
    private SecretImportsClient _secretImports;
    private SecretTagsClient _secretTags;
    private IdentitiesClient _identities;
    private DynamicSecretsClient _dynamicSecrets;
    private GroupsClient _groups;
    private KmsClient _kms;
    private SharedSecretsClient _sharedSecrets;
    private ProjectAccessClient _projectAccess;
    private SecretSyncsClient _secretSyncs;
    private CertificatesClient _certificates;
    private CertificateProfilesClient _certificateProfiles;
    private CertificatePoliciesClient _certificatePolicies;
    private CertificateAuthoritiesClient _certificateAuthorities;
    private RawClient _raw;
    public InfisicalClient(InfisicalSdkSettings settings)
    {
      _apiClient = new ApiClient(settings.HostUri);
      _secretsClient = new SecretsClient(_apiClient);
      _authClient = new AuthClient(_apiClient, (accessToken) => _apiClient.SetAccessToken(accessToken));
      _pkiClient = new PkiClient(_apiClient);
      _projects = new ProjectsClient(_apiClient);
      _environments = new EnvironmentsClient(_apiClient);
      _folders = new FoldersClient(_apiClient);
      _secretImports = new SecretImportsClient(_apiClient);
      _secretTags = new SecretTagsClient(_apiClient);
      _identities = new IdentitiesClient(_apiClient);
      _dynamicSecrets = new DynamicSecretsClient(_apiClient);
      _groups = new GroupsClient(_apiClient);
      _kms = new KmsClient(_apiClient);
      _sharedSecrets = new SharedSecretsClient(_apiClient);
      _projectAccess = new ProjectAccessClient(_apiClient);
      _secretSyncs = new SecretSyncsClient(_apiClient);
      _certificates = new CertificatesClient(_apiClient);
      _certificateProfiles = new CertificateProfilesClient(_apiClient);
      _certificatePolicies = new CertificatePoliciesClient(_apiClient);
      _certificateAuthorities = new CertificateAuthoritiesClient(_apiClient);
      _raw = new RawClient(_apiClient);
    }

    public AuthClient Auth()
    {
      return _authClient;
    }

    public SecretsClient Secrets()
    {
      return _secretsClient;
    }

    public PkiClient Pki()
    {
      return _pkiClient;
    }

    /// <summary>Create, read, update and delete projects.</summary>
    public ProjectsClient Projects()
    {
      return _projects;
    }

    /// <summary>Manage the environments of a project.</summary>
    public EnvironmentsClient Environments()
    {
      return _environments;
    }

    /// <summary>Manage secret folders.</summary>
    public FoldersClient Folders()
    {
      return _folders;
    }

    /// <summary>Manage secret imports between folders.</summary>
    public SecretImportsClient SecretImports()
    {
      return _secretImports;
    }

    /// <summary>Manage the secret tags of a project.</summary>
    public SecretTagsClient SecretTags()
    {
      return _secretTags;
    }

    /// <summary>Manage machine identities and their auth methods.</summary>
    public IdentitiesClient Identities()
    {
      return _identities;
    }

    /// <summary>Manage dynamic secrets and their leases.</summary>
    public DynamicSecretsClient DynamicSecrets()
    {
      return _dynamicSecrets;
    }

    /// <summary>Manage groups and their members.</summary>
    public GroupsClient Groups()
    {
      return _groups;
    }

    /// <summary>Cryptographic keys of the Infisical KMS.</summary>
    public KmsClient Kms()
    {
      return _kms;
    }

    /// <summary>Share secrets via a one-time link.</summary>
    public SharedSecretsClient SharedSecrets()
    {
      return _sharedSecrets;
    }

    /// <summary>Project roles and memberships of identities, groups and users.</summary>
    public ProjectAccessClient ProjectAccess()
    {
      return _projectAccess;
    }

    /// <summary>Call any endpoint of the Infisical API that has no typed client yet.</summary>
    public RawClient Raw()
    {
      return _raw;
    }

    /// <summary>Sync secrets to external destinations (AWS, GitHub, Vercel, ...).</summary>
    public SecretSyncsClient SecretSyncs()
    {
      return _secretSyncs;
    }

    /// <summary>Issue, renew, revoke and export certificates.</summary>
    public CertificatesClient Certificates()
    {
      return _certificates;
    }

    /// <summary>Certificate profiles (enrollment configuration).</summary>
    public CertificateProfilesClient CertificateProfiles()
    {
      return _certificateProfiles;
    }

    /// <summary>Certificate policies.</summary>
    public CertificatePoliciesClient CertificatePolicies()
    {
      return _certificatePolicies;
    }

    /// <summary>Internal certificate authorities.</summary>
    public CertificateAuthoritiesClient CertificateAuthorities()
    {
      return _certificateAuthorities;
    }
  }
}