
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Polly;
using Polly.Extensions.Http;

namespace Infisical.Sdk.Api
{
  public class InfisicalException : Exception
  {
    public InfisicalException(string message) : base(message) { }
    public InfisicalException(string message, Exception innerException) : base(message, innerException) { }
  }

  public class ApiClient : IDisposable
  {
    private readonly HttpClient _httpClient;
    private string? _accessToken;
    private string _baseUrl;

    private static readonly IAsyncPolicy<HttpResponseMessage> RetryPolicy =
    HttpPolicyExtensions
        .HandleTransientHttpError() // HttpRequestException and 5XX/408 responses
        .OrResult(msg => !msg.IsSuccessStatusCode)
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    // Used by the generic request method: only transient failures (network errors, 5xx, 408) are retried, so a 4xx
    // answer (e.g. 404 or 403) is reported immediately.
    private static readonly IAsyncPolicy<HttpResponseMessage> TransientRetryPolicy =
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    public ApiClient(string baseUrl, string? accessToken = null)
    {
      _httpClient = new HttpClient();
      _baseUrl = baseUrl;
      _accessToken = accessToken;

      FormatBaseUrl();
    }

    public void SetAccessToken(string accessToken)
    {
      _accessToken = accessToken;
    }

    private void FormatBaseUrl()
    {
      // Remove trailing slash if present
      if (_baseUrl.EndsWith("/"))
      {
        _baseUrl = _baseUrl.Substring(0, _baseUrl.Length - 1);
      }

      // Check if URL starts with protocol
      if (!System.Text.RegularExpressions.Regex.IsMatch(_baseUrl, @"^[a-zA-Z]+://.*"))
      {
        _baseUrl = "https://" + _baseUrl;
      }

      // Remove /api if present at the end
      if (_baseUrl.EndsWith("/api"))
      {
        _baseUrl = _baseUrl.Substring(0, _baseUrl.Length - 4);
      }
    }

    public string GetBaseUrl()
    {
      return _baseUrl;
    }

    public HttpClient GetClient()
    {
      return _httpClient;
    }

    private async Task<string> FormatErrorMessageAsync(HttpResponseMessage response)
    {
      var message = $"Unexpected response: {response.StatusCode} {response.ReasonPhrase}";

      try
      {
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!string.IsNullOrEmpty(content))
        {
          message += $" - {content}";
        }
      }
      catch
      {
        // If we can't read the error content, just use the status message
      }

      return message;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest requestBody, bool omitNullValues = false)
    {
      try
      {
        var jsonContent = omitNullValues ? JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }) : JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_baseUrl), url))
        {
          Content = content
        };

        request.Headers.Add("Accept", "application/json");

        if (!string.IsNullOrEmpty(_accessToken))
        {
          request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        }

        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
          var errorMessage = await FormatErrorMessageAsync(response).ConfigureAwait(false);
          throw new HttpRequestException(errorMessage);
        }

        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseContent))
        {
          throw new HttpRequestException("Response body is null or empty");
        }

        var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
          throw new InfisicalException("Failed to deserialize response content");
        }

        return result;
      }
      catch (Exception ex) when (!(ex is InfisicalException))
      {
        throw new InfisicalException($"Error during POST request: {ex.Message}", ex);
      }
    }

    public async Task<TResponse> GetAsync<TResponse>(string url, Dictionary<string, string>? queryParams = null)
    {
      try
      {
        var uriBuilder = new UriBuilder(new Uri(new Uri(_baseUrl), url));

        if (queryParams != null && queryParams.Count > 0)
        {
          var query = HttpUtility.ParseQueryString(string.Empty);
          foreach (var param in queryParams)
          {
            query[param.Key] = param.Value;
          }
          uriBuilder.Query = query.ToString();
        }


        var response = await RetryPolicy.ExecuteAsync(async () =>
        {
          using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
          request.Headers.Add("Accept", "application/json");

          if (!string.IsNullOrEmpty(_accessToken))
          {
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
          }
          return await _httpClient.SendAsync(request).ConfigureAwait(false);
        }).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
          var errorMessage = await FormatErrorMessageAsync(response).ConfigureAwait(false);
          throw new HttpRequestException(errorMessage);
        }

        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseContent))
        {
          throw new HttpRequestException("Response body is null or empty");
        }

        var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
          throw new InfisicalException("Failed to deserialize response content");
        }

        return result;
      }
      catch (Exception ex) when (!(ex is InfisicalException))
      {
        throw new InfisicalException($"Error during GET request: {ex.Message}", ex);
      }
    }

    public async Task<TResponse> PatchAsync<TRequest, TResponse>(string url, TRequest requestBody, bool omitNullValues = false)
    {
      try
      {
        var jsonContent = omitNullValues ? JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }) : JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(new Uri(_baseUrl), url))
        {
          Content = content
        };

        request.Headers.Add("Accept", "application/json");

        if (!string.IsNullOrEmpty(_accessToken))
        {
          request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        }

        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
          var errorMessage = await FormatErrorMessageAsync(response).ConfigureAwait(false);
          throw new HttpRequestException(errorMessage);
        }

        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseContent))
        {
          throw new HttpRequestException("Response body is null or empty");
        }

        var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
          throw new InfisicalException("Failed to deserialize response content");
        }

        return result;
      }
      catch (Exception ex) when (!(ex is InfisicalException))
      {
        throw new InfisicalException($"Error during PATCH request: {ex.Message}", ex);
      }
    }

    public async Task<TResponse> DeleteAsync<TRequest, TResponse>(string url, TRequest requestBody, bool omitNullValues = false)
    {
      try
      {
        var jsonContent = omitNullValues ? JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }) : JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(new Uri(_baseUrl), url))
        {
          Content = content
        };

        request.Headers.Add("Accept", "application/json");

        if (!string.IsNullOrEmpty(_accessToken))
        {
          request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        }

        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
          var errorMessage = await FormatErrorMessageAsync(response).ConfigureAwait(false);
          throw new HttpRequestException(errorMessage);
        }

        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseContent))
        {
          throw new HttpRequestException("Response body is null or empty");
        }

        var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
          throw new InfisicalException("Failed to deserialize response content");
        }

        return result;
      }
      catch (Exception ex) when (!(ex is InfisicalException))
      {
        throw new InfisicalException($"Error during DELETE request: {ex.Message}", ex);
      }
    }

    private static readonly JsonSerializerOptions ReadOptions = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions
    {
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Generic request used by the typed clients. Null properties are omitted from the request body.
    /// An empty response body is tolerated and results in <c>default</c>.
    /// </summary>
    public async Task<TResponse?> SendAsync<TResponse>(HttpMethod method, string url, object? body = null, Dictionary<string, string>? queryParams = null, bool retry = false)
    {
      try
      {
        var uriBuilder = new UriBuilder(new Uri(new Uri(_baseUrl), url));

        if (queryParams != null && queryParams.Count > 0)
        {
          var query = HttpUtility.ParseQueryString(string.Empty);
          foreach (var param in queryParams)
          {
            query[param.Key] = param.Value;
          }
          uriBuilder.Query = query.ToString();
        }

        string? jsonContent = body == null ? null : JsonSerializer.Serialize(body, body.GetType(), WriteOptions);

        Func<Task<HttpResponseMessage>> send = async () =>
        {
          using var request = new HttpRequestMessage(method, uriBuilder.Uri);
          request.Headers.Add("Accept", "application/json");

          if (!string.IsNullOrEmpty(_accessToken))
          {
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
          }

          if (jsonContent != null)
          {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
          }

          return await _httpClient.SendAsync(request).ConfigureAwait(false);
        };

        // only idempotent reads are retried
        var response = retry
          ? await TransientRetryPolicy.ExecuteAsync(send).ConfigureAwait(false)
          : await send().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
          var errorMessage = await FormatErrorMessageAsync(response).ConfigureAwait(false);
          throw new HttpRequestException(errorMessage);
        }

        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(responseContent))
        {
          return default;
        }

        return JsonSerializer.Deserialize<TResponse>(responseContent, ReadOptions);
      }
      catch (Exception ex) when (!(ex is InfisicalException))
      {
        throw new InfisicalException($"Error during {method.Method} request: {ex.Message}", ex);
      }
    }

    /// <summary>Like <see cref="SendAsync{TResponse}"/>, but throws if the server returned no content.</summary>
    public async Task<TResponse> SendRequiredAsync<TResponse>(HttpMethod method, string url, object? body = null, Dictionary<string, string>? queryParams = null, bool retry = false)
    {
      var result = await SendAsync<TResponse>(method, url, body, queryParams, retry).ConfigureAwait(false);
      if (result == null)
      {
        throw new InfisicalException("Response body is null or empty");
      }
      return result;
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
    }
  }

  public class QueryBuilder
  {
    private readonly Dictionary<string, string> _params = new Dictionary<string, string>();

    public QueryBuilder Add(string key, object value)
    {
      if (value != null)
      {
#pragma warning disable CS8601 // Possible null reference assignment.
        _params[key] = value is bool boolean ? (boolean ? "true" : "false") : value.ToString();
#pragma warning restore CS8601 // Possible null reference assignment.
      }
      return this;
    }

    public Dictionary<string, string> Build()
    {
      return new Dictionary<string, string>(_params);
    }
  }
}
