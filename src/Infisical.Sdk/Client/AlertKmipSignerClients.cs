using System.Text.Json;
using Infisical.Sdk.Api;
using Infisical.Sdk.Util;

namespace Infisical.Sdk.Client;

/// <summary>Alerts that notify about events of resources (e.g. expiring certificates).</summary>
public class AlertsClient
{
  private const string BasePath = "/api/v1/alerts";
  private readonly ApiClient _apiClient;

  public AlertsClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string alertId)
  {
    if (string.IsNullOrEmpty(alertId)) throw new InfisicalException("AlertId is required");
    return $"{BasePath}/{Uri.EscapeDataString(alertId)}";
  }

  public async Task<Alert> CreateAsync(CreateAlertOptions options)
  {
    try
    {
      options.Validate();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<Alert>(response, "alert");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create alert", e);
    }
  }

  public async Task<Alert[]> ListAsync(ListAlertsOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.ResourceType)) throw new InfisicalException("ResourceType is required");
      var query = new QueryBuilder()
        .Add("resourceType", options.ResourceType)
        .Add("resourceId", options.ResourceId!)
        .Add("projectId", options.ProjectId!)
        .Add("enabled", options.Enabled!)
        .Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<Alert>(response, "alerts");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list alerts", e);
    }
  }

  public async Task<Alert> GetAsync(string alertId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(alertId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<Alert>(response, "alert");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get alert", e);
    }
  }

  public async Task<Alert> UpdateAsync(UpdateAlertOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.AlertId), options).ConfigureAwait(false);
      return ResponseReader.Entity<Alert>(response, "alert");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update alert", e);
    }
  }

  public async Task<Alert> DeleteAsync(string alertId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(alertId)).ConfigureAwait(false);
      return ResponseReader.Entity<Alert>(response, "alert");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete alert", e);
    }
  }

  /// <summary>Sends a test notification through a channel.</summary>
  public async Task<AlertChannelTestResult> TestChannelAsync(TestAlertChannelOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.ResourceType)) throw new InfisicalException("ResourceType is required");
      if (string.IsNullOrEmpty(options.ChannelType)) throw new InfisicalException("ChannelType is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath + "/channels/test", options).ConfigureAwait(false);
      return ResponseReader.Deserialize<AlertChannelTestResult>(response) ?? new AlertChannelTestResult();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to test alert channel", e);
    }
  }
}

/// <summary>KMIP servers (certificate based access of KMIP clients).</summary>
public class KmipServersClient
{
  private const string BasePath = "/api/v1/kmip/servers";
  private readonly ApiClient _apiClient;

  public KmipServersClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string kmipServerId)
  {
    if (string.IsNullOrEmpty(kmipServerId)) throw new InfisicalException("KmipServerId is required");
    return $"{BasePath}/{Uri.EscapeDataString(kmipServerId)}";
  }

  public async Task<KmipServer> CreateAsync(CreateKmipServerOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      if (string.IsNullOrEmpty(options.HostnamesOrIps)) throw new InfisicalException("HostnamesOrIps is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<KmipServer>(response, "kmipServer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create KMIP server", e);
    }
  }

  public async Task<KmipServer[]> ListAsync()
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, null, true).ConfigureAwait(false);
      return ResponseReader.List<KmipServer>(response, "kmipServers");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list KMIP servers", e);
    }
  }

  public async Task<KmipServer> GetAsync(string kmipServerId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(kmipServerId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<KmipServer>(response, "kmipServer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get KMIP server", e);
    }
  }

  public async Task<KmipServer> UpdateAsync(UpdateKmipServerOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.KmipServerId), options).ConfigureAwait(false);
      return ResponseReader.Entity<KmipServer>(response, "kmipServer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update KMIP server", e);
    }
  }

  public async Task<KmipServer> DeleteAsync(string kmipServerId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(kmipServerId)).ConfigureAwait(false);
      return ResponseReader.Entity<KmipServer>(response, "kmipServer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete KMIP server", e);
    }
  }

  public async Task<JsonElement?> RevokeAsync(string kmipServerId)
  {
    try
    {
      return await _apiClient.SendAsync<JsonElement?>(HttpMethod.Post, Path(kmipServerId) + "/revoke").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to revoke KMIP server", e);
    }
  }

  /// <summary>Generates an enrollment token for a KMIP server that uses the token auth method.</summary>
  public async Task<GatewayEnrollmentToken> GenerateEnrollmentTokenAsync(string kmipServerId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(kmipServerId) + "/token-auth/generate-enrollment-token").ConfigureAwait(false);
      var token = response.GetProperty("token").GetString() ?? string.Empty;
      var expiresAt = response.TryGetProperty("expiresAt", out var e) && e.ValueKind == JsonValueKind.String ? e.GetString() : null;
      return new GatewayEnrollmentToken(token, expiresAt);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to generate enrollment token", e);
    }
  }
}

/// <summary>Code signing with signers backed by certificates of the Certificate Manager.</summary>
public class SignersClient
{
  private const string BasePath = "/api/v1/cert-manager/signers";
  private readonly ApiClient _apiClient;

  public SignersClient(ApiClient apiClient)
  {
    _apiClient = apiClient;
  }

  private static string Path(string signerId)
  {
    if (string.IsNullOrEmpty(signerId)) throw new InfisicalException("SignerId is required");
    return $"{BasePath}/{Uri.EscapeDataString(signerId)}";
  }

  public async Task<Signer> CreateAsync(CreateSignerOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Name)) throw new InfisicalException("Name is required");
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, BasePath, options).ConfigureAwait(false);
      return ResponseReader.Entity<Signer>(response, "signer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create signer", e);
    }
  }

  public async Task<Signer[]> ListAsync(string? projectId = null, string? search = null, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("projectId", projectId!).Add("search", search!).Add("offset", offset!).Add("limit", limit!).Build();
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, BasePath, null, query, true).ConfigureAwait(false);
      return ResponseReader.List<Signer>(response, "signers");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list signers", e);
    }
  }

  public async Task<Signer> GetAsync(string signerId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(signerId), null, null, true).ConfigureAwait(false);
      return ResponseReader.Entity<Signer>(response, "signer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get signer", e);
    }
  }

  public async Task<Signer> UpdateAsync(UpdateSignerOptions options)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(options.SignerId), options).ConfigureAwait(false);
      return ResponseReader.Entity<Signer>(response, "signer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update signer", e);
    }
  }

  public async Task<Signer> DeleteAsync(string signerId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Delete, Path(signerId)).ConfigureAwait(false);
      return ResponseReader.Entity<Signer>(response, "signer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to delete signer", e);
    }
  }

  /// <summary>Enables or disables a signer.</summary>
  public async Task<Signer> SetStatusAsync(string signerId, bool enabled)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethods.Patch, Path(signerId) + "/status", new { status = enabled ? "active" : "disabled" }).ConfigureAwait(false);
      return ResponseReader.Entity<Signer>(response, "signer");
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to set signer status", e);
    }
  }

  public Task<Signer> EnableAsync(string signerId) => SetStatusAsync(signerId, true);
  public Task<Signer> DisableAsync(string signerId) => SetStatusAsync(signerId, false);

  /// <summary>Checks whether a certificate can be issued for the signer (raw response).</summary>
  public async Task<JsonElement> CheckIssuanceAsync(string signerId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(signerId) + "/issuance/check").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to check signer issuance", e);
    }
  }

  /// <summary>Gets the permissions and memberships of the signer (raw response).</summary>
  public async Task<JsonElement> GetPermissionsAsync(string signerId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(signerId) + "/permissions", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get signer permissions", e);
    }
  }

  // --- signing ----------------------------------------------------------------------------------

  /// <summary>Signs data (or a digest) with the signer's key (raw response: signature, ...).</summary>
  public async Task<JsonElement> SignAsync(SignDataOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Data)) throw new InfisicalException("Data is required");
      if (string.IsNullOrEmpty(options.SigningAlgorithm)) throw new InfisicalException("SigningAlgorithm is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(options.SignerId) + "/sign", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to sign data", e);
    }
  }

  public async Task<string> GetPublicKeyAsync(string signerId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(signerId) + "/public-key", null, null, true).ConfigureAwait(false);
      return response.TryGetProperty("publicKey", out var key) && key.ValueKind == JsonValueKind.String ? key.GetString() ?? string.Empty : response.GetRawText();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get signer public key", e);
    }
  }

  // --- certificate ------------------------------------------------------------------------------

  public async Task<CertificateMaterial> GetCertificateAsync(string signerId)
  {
    try
    {
      var response = await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(signerId) + "/certificate", null, null, true).ConfigureAwait(false);
      return ResponseReader.Deserialize<CertificateMaterial>(response) ?? new CertificateMaterial();
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get signer certificate", e);
    }
  }

  public async Task<JsonElement> ReissueCertificateAsync(ReissueSignerCertificateOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.CaId)) throw new InfisicalException("CaId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(options.SignerId) + "/certificate/reissue", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to reissue signer certificate", e);
    }
  }

  /// <summary>Imports an externally issued certificate whose key lives in an HSM.</summary>
  /// <param name="body">Request body (caId, externalOrderId, certificate, certificateRenewBeforeDays), see the API reference.</param>
  public async Task<JsonElement> ImportCertificateAsync(string signerId, object body)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(signerId) + "/certificate/import", body).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to import signer certificate", e);
    }
  }

  // --- approval policy and requests -------------------------------------------------------------

  public async Task<JsonElement> GetApprovalPolicyAsync(string signerId)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(signerId) + "/approval-policy", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get approval policy", e);
    }
  }

  /// <summary>Replaces the approval policy (steps, approvers, limits), see the API reference for the body.</summary>
  public async Task<JsonElement> UpdateApprovalPolicyAsync(string signerId, object policy)
  {
    try
    {
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Put, Path(signerId) + "/approval-policy", policy).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to update approval policy", e);
    }
  }

  public async Task<JsonElement> ListRequestsAsync(string signerId, string[]? statuses = null, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("statuses", statuses == null ? null! : string.Join(",", statuses)).Add("offset", offset!).Add("limit", limit!).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(signerId) + "/requests", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list signing requests", e);
    }
  }

  /// <summary>Requests approval to sign.</summary>
  public async Task<JsonElement> CreateRequestAsync(SigningRequestOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Justification)) throw new InfisicalException("Justification is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(options.SignerId) + "/requests", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to create signing request", e);
    }
  }

  /// <summary>Grants a user or identity an approval in advance (set <c>GranteeUserId</c> or <c>GranteeIdentityId</c>).</summary>
  public async Task<JsonElement> PreApproveAsync(SigningRequestOptions options)
  {
    try
    {
      if (string.IsNullOrEmpty(options.Justification)) throw new InfisicalException("Justification is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, Path(options.SignerId) + "/requests/pre-approve", options).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to pre-approve signing request", e);
    }
  }

  public async Task<JsonElement> RevokeRequestAsync(string signerId, string requestId)
  {
    try
    {
      if (string.IsNullOrEmpty(requestId)) throw new InfisicalException("RequestId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Post, $"{Path(signerId)}/requests/{Uri.EscapeDataString(requestId)}/revoke").ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to revoke signing request", e);
    }
  }

  // --- operations -------------------------------------------------------------------------------

  /// <summary>Lists the signing operations (audit trail) of the signer. <paramref name="status"/> filters by status.</summary>
  public async Task<JsonElement> ListOperationsAsync(string signerId, string? status = null, int? offset = null, int? limit = null)
  {
    try
    {
      var query = new QueryBuilder().Add("status", status!).Add("offset", offset!).Add("limit", limit!).Build();
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, Path(signerId) + "/operations", null, query, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to list signing operations", e);
    }
  }

  public async Task<JsonElement> GetOperationAsync(string signerId, string operationId)
  {
    try
    {
      if (string.IsNullOrEmpty(operationId)) throw new InfisicalException("OperationId is required");
      return await _apiClient.SendRequiredAsync<JsonElement>(HttpMethod.Get, $"{Path(signerId)}/operations/{Uri.EscapeDataString(operationId)}", null, null, true).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      throw new InfisicalException("Failed to get signing operation", e);
    }
  }
}
