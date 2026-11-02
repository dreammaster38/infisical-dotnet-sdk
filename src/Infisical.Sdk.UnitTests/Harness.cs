using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Web;
using Infisical.Sdk.Model;

namespace Infisical.Sdk.Tests;

internal sealed record Recorded(string Method, string Path, string RawQuery, string Body, string? Authorization)
{
  public NameValueCollection Query => HttpUtility.ParseQueryString(RawQuery);

  /// <summary>The request body parsed as JSON.</summary>
  public JsonElement Json => JsonDocument.Parse(Body).RootElement;
}

/// <summary>
/// A tiny in-process HTTP server that records the requests of the SDK and answers with canned responses, so the
/// clients can be tested without an Infisical instance.
/// </summary>
internal sealed class Harness : IDisposable
{
  private readonly HttpListener _listener = new HttpListener();
  private readonly Dictionary<string, (int Status, string Body)> _responses = new Dictionary<string, (int, string)>();
  private readonly object _lock = new object();

  public List<Recorded> Requests { get; } = new List<Recorded>();
  public InfisicalClient Client { get; }
  public string BaseUrl { get; }

  public Harness()
  {
    int port;
    var probe = new TcpListener(IPAddress.Loopback, 0);
    probe.Start();
    port = ((IPEndPoint)probe.LocalEndpoint).Port;
    probe.Stop();

    BaseUrl = $"http://localhost:{port}";
    _listener.Prefixes.Add(BaseUrl + "/");
    _listener.Start();
    _ = Task.Run(Loop);

    Client = new InfisicalClient(new InfisicalSdkSettingsBuilder().WithHostUri(BaseUrl).Build());
  }

  /// <summary>Registers the response for a method + path (the query string is ignored).</summary>
  public Harness On(string method, string path, string body = "{}", int status = 200)
  {
    lock (_lock)
    {
      _responses[$"{method.ToUpperInvariant()} {path}"] = (status, body);
    }
    return this;
  }

  public Recorded Last
  {
    get
    {
      lock (_lock)
      {
        return Requests[^1];
      }
    }
  }

  /// <summary>Logs in with universal auth so that following requests carry a bearer token.</summary>
  public async Task LoginAsync(string token = "test-token")
  {
    On("POST", "/api/v1/auth/universal-auth/login", $"{{\"accessToken\":\"{token}\",\"expiresIn\":60,\"accessTokenMaxTTL\":120,\"tokenType\":\"Bearer\"}}");
    await Client.Auth().UniversalAuth().LoginAsync("id", "secret");
  }

  private async Task Loop()
  {
    while (_listener.IsListening)
    {
      HttpListenerContext ctx;
      try
      {
        ctx = await _listener.GetContextAsync();
      }
      catch
      {
        return;
      }

      string body;
      using (var reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8))
      {
        body = await reader.ReadToEndAsync();
      }

      var url = ctx.Request.Url!;
      var recorded = new Recorded(ctx.Request.HttpMethod, url.AbsolutePath, url.Query, body, ctx.Request.Headers["Authorization"]);

      (int Status, string Body) response;
      lock (_lock)
      {
        Requests.Add(recorded);
        if (!_responses.TryGetValue($"{recorded.Method} {recorded.Path}", out response))
        {
          response = (404, "{\"message\":\"no canned response\"}");
        }
      }

      var bytes = Encoding.UTF8.GetBytes(response.Body);
      ctx.Response.StatusCode = response.Status;
      ctx.Response.ContentType = "application/json";
      ctx.Response.ContentLength64 = bytes.Length;
      await ctx.Response.OutputStream.WriteAsync(bytes);
      ctx.Response.Close();
    }
  }

  public void Dispose()
  {
    try { _listener.Stop(); _listener.Close(); } catch { /* shutting down */ }
  }
}
