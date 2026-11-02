using Xunit;

namespace Infisical.Sdk.Tests;

public class CoreTests
{
  [Fact]
  public async Task Requests_carry_the_access_token_after_login()
  {
    using var h = new Harness();
    await h.LoginAsync("abc");
    h.On("GET", "/api/v1/projects", "{\"projects\":[]}");

    await h.Client.Projects().ListAsync();

    Assert.Equal("Bearer abc", h.Last.Authorization);
  }

  [Fact]
  public async Task An_existing_access_token_can_be_used_without_login()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/projects", "{\"projects\":[]}");

    h.Client.Auth().SetAccessToken("static-token");
    await h.Client.Projects().ListAsync();

    Assert.Equal("Bearer static-token", h.Last.Authorization);
    Assert.Throws<InfisicalException>(() => h.Client.Auth().SetAccessToken(""));
  }

  [Fact]
  public async Task Requests_before_login_are_anonymous()
  {
    using var h = new Harness();
    h.On("GET", "/api/status", "{\"ok\":true}");

    await h.Client.Instance().GetStatusAsync();

    Assert.Null(h.Last.Authorization);
  }

  [Fact]
  public async Task Server_errors_become_InfisicalException_with_the_server_message()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/projects/p1", "{\"message\":\"Forbidden\"}", 403);

    var ex = await Assert.ThrowsAsync<InfisicalException>(() => h.Client.Projects().GetAsync("p1"));

    Assert.Contains("Forbidden", ex.InnerException?.Message);
  }

  [Fact]
  public async Task Client_errors_are_not_retried()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/projects/p1", "{}", 404);

    await Assert.ThrowsAsync<InfisicalException>(() => h.Client.Projects().GetAsync("p1"));

    Assert.Single(h.Requests);
  }

  [Fact]
  public async Task Missing_required_options_are_rejected_before_any_request()
  {
    using var h = new Harness();

    await Assert.ThrowsAsync<InfisicalException>(() => h.Client.Projects().CreateAsync(new CreateProjectOptions()));

    Assert.Empty(h.Requests);
  }

  [Fact]
  public async Task Path_segments_are_url_encoded()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/projects/slug/my%20slug", "{\"project\":{\"id\":\"p\"}}");

    await h.Client.Projects().GetBySlugAsync("my slug");

    Assert.Equal("/api/v1/projects/slug/my%20slug", h.Last.Path);
  }

  [Fact]
  public async Task Empty_responses_are_tolerated_where_no_body_is_expected()
  {
    using var h = new Harness();
    h.On("DELETE", "/api/v1/shared-secrets/s1", "");

    await h.Client.SharedSecrets().DeleteAsync("s1");

    Assert.Equal("DELETE", h.Last.Method);
  }

  [Fact]
  public async Task Raw_client_sends_arbitrary_requests()
  {
    using var h = new Harness();
    h.On("POST", "/api/v1/anything", "{\"ok\":true}");

    var result = await h.Client.Raw().PostAsync("/api/v1/anything", new { a = 1, b = (string?)null });

    Assert.True(result!.Value.GetProperty("ok").GetBoolean());
    Assert.Equal(1, h.Last.Json.GetProperty("a").GetInt32());
    Assert.False(h.Last.Json.TryGetProperty("b", out _)); // null values are omitted
  }

  [Fact]
  public async Task Unknown_response_properties_are_kept_in_AdditionalData()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/projects/p1", "{\"project\":{\"id\":\"p1\",\"name\":\"X\",\"somethingNew\":42}}");

    var project = await h.Client.Projects().GetAsync("p1");

    Assert.Equal(42, project.AdditionalData!["somethingNew"].GetInt32());
  }

  [Fact]
  public async Task Flat_and_wrapped_responses_are_both_understood()
  {
    using var h = new Harness();
    h.On("GET", "/api/v1/projects/wrapped", "{\"project\":{\"id\":\"w\"}}");
    h.On("GET", "/api/v1/projects/flat", "{\"id\":\"f\"}");

    Assert.Equal("w", (await h.Client.Projects().GetAsync("wrapped")).Id);
    Assert.Equal("f", (await h.Client.Projects().GetAsync("flat")).Id);
  }

  [Fact]
  public async Task User_login_selects_the_organization_and_uses_the_org_token()
  {
    using var h = new Harness();
    h.On("POST", "/api/v3/auth/login", "{\"accessToken\":\"user-token\"}");
    h.On("POST", "/api/v3/auth/select-organization", "{\"token\":\"org-token\",\"isMfaEnabled\":false}");
    h.On("GET", "/api/v1/organization", "{\"organizations\":[{\"id\":\"o1\",\"name\":\"Org\"}]}");

    var result = await h.Client.Auth().UserAuth().LoginAsync("a@b.c", "pw", "o1");
    var orgs = await h.Client.Organizations().ListAsync();

    Assert.Equal("org-token", result.AccessToken);
    Assert.Equal("o1", result.OrganizationId);
    Assert.Equal("a@b.c", h.Requests[0].Json.GetProperty("email").GetString());
    Assert.Equal("Bearer user-token", h.Requests[1].Authorization);       // select-organization is called with the login token
    Assert.Equal("Bearer org-token", h.Requests[2].Authorization);
    Assert.Single(orgs);
  }

  [Fact]
  public async Task User_login_without_organization_keeps_the_login_token()
  {
    using var h = new Harness();
    h.On("POST", "/api/v3/auth/login", "{\"accessToken\":\"user-token\"}");

    var result = await h.Client.Auth().UserAuth().LoginAsync("a@b.c", "pw");

    Assert.Null(result.OrganizationId);
    Assert.Single(h.Requests);
  }

  [Fact]
  public async Task User_login_reports_missing_mfa_support()
  {
    using var h = new Harness();
    h.On("POST", "/api/v3/auth/login", "{\"accessToken\":\"user-token\"}");
    h.On("POST", "/api/v3/auth/select-organization", "{\"token\":\"mfa-token\",\"isMfaEnabled\":true,\"mfaMethod\":\"totp\"}");

    var ex = await Assert.ThrowsAsync<InfisicalException>(() => h.Client.Auth().UserAuth().LoginAsync("a@b.c", "pw", "o1"));

    var messages = new List<string>();
    for (Exception? e = ex; e != null; e = e.InnerException) messages.Add(e.Message);
    Assert.Contains(messages, m => m.Contains("MFA"));
  }

  [Fact]
  public async Task Requests_send_a_user_agent()
  {
    using var h = new Harness();
    h.On("GET", "/api/status", "{}");

    await h.Client.Instance().GetStatusAsync();

    Assert.Contains("infisical-dotnet-sdk", h.Last.UserAgent);
  }

  [Fact]
  public async Task Responses_with_a_different_primitive_type_than_modelled_are_still_read()
  {
    using var h = new Harness();
    // Name is modelled as text but sent as a number, ScanIntervalDays is modelled as int but sent as a string,
    // Description is sent as an object
    h.On("GET", "/api/v1/cert-manager/discovery-jobs/d1", "{\"id\":\"d1\",\"name\":12345,\"scanIntervalDays\":\"7\",\"isActive\":true,\"description\":{\"a\":1}}");

    var job = await h.Client.PkiDiscovery().GetJobAsync("d1");

    Assert.Equal("12345", job.Name);
    Assert.Equal(7, job.ScanIntervalDays);
    Assert.Equal("{\"a\":1}", job.Description);
  }
}
