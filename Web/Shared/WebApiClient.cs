using Newtonsoft.Json;
using System.Net.Http.Headers;
using Web.Shared.Exceptions;

namespace Web.Shared;

/// <summary>
/// Web Client - Get, Put, Delete and Post for WEB API
/// </summary>
public class WebApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed;

    public IList<KeyValuePair<string, string>> Headers { get; set; } = new List<KeyValuePair<string, string>>();

    public WebApiClient(HttpClient httpClient)
        => _httpClient = httpClient;

    /// <summary>
    /// Gets using the specified method.
    /// </summary>
    public async Task<TResponse> Get<TResponse>(WebApiClientInfo<object> info)
        where TResponse : class
    {
        var requestUri = $"{info.Method}/{info.Request}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        AddHeaders(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await Read<TResponse, HttpGetException>(response);
    }

    /// <summary>
    /// Gets using the specified method.
    /// </summary>
    public async Task<TResponse> Get<TRequest, TResponse>(WebApiClientInfo<TRequest> info)
        where TResponse : class
        where TRequest : class
    {
        var strRequest = JsonConvert.SerializeObject(info.Request);
        using var content = new StringContent(strRequest);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, info.Method) { Content = content };
        AddHeaders(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await Read<TResponse, HttpPostException>(response);
    }

    /// <summary>
    /// Posts using the specified method.
    /// </summary>
    public async Task<TResponse> Post<TRequest, TResponse>(WebApiClientInfo<TRequest> info)
        where TResponse : class
    {
        var strRequest = JsonConvert.SerializeObject(info.Request);
        using var content = new StringContent(strRequest);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, info.Method) { Content = content };
        AddHeaders(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await Read<TResponse, HttpPostException>(response);
    }

    /// <summary>
    /// Puts using the specified method.
    /// </summary>
    public async Task<TResponse> Put<TRequest, TResponse>(WebApiClientInfo<TRequest> info)
        where TResponse : class
    {
        var strRequest = JsonConvert.SerializeObject(info.Request);
        using var content = new StringContent(strRequest);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        using var request = new HttpRequestMessage(HttpMethod.Put, info.Method) { Content = content };
        AddHeaders(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await Read<TResponse, HttpPutException>(response);
    }

    /// <summary>
    /// Deletes using the specified method.
    /// </summary>
    public async Task<TResponse> Delete<TIdentity, TResponse>(WebApiClientInfo info, TIdentity id)
        where TResponse : class
    {
        var requestUri = $"{info.Method}/{id}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        AddHeaders(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await Read<TResponse, HttpDeleteException>(response);
    }

    /// <summary>
    /// Delete using the specified method.
    /// </summary>
    public async Task<TResponse> Delete<TResponse>(WebApiClientInfo<object> info)
        where TResponse : class
    {
        var requestUri = $"{info.Method}/{info.Request}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        AddHeaders(request);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await Read<TResponse, HttpGetException>(response);
    }

    #region Helpers

    private void AddHeaders(HttpRequestMessage request)
    {
        if (Headers == null) return;
        foreach (var kvp in Headers)
        {
            if (!request.Headers.Contains(kvp.Key))
                request.Headers.Add(kvp.Key, kvp.Value);
        }
    }

    private static async Task<TResponse> Read<TResponse, TException>(HttpResponseMessage message)
        where TException : Exception, new()
        where TResponse : class
    {
        if (message.IsSuccessStatusCode)
        {
            var s = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResponse>(s);
        }
        throw new TException();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        // No managed resources to dispose
        _disposed = true;
    }

    #endregion
}

#region WebApiClientInfo

public class WebApiClientInfo
{
    public string Method { get; set; }
}

public class WebApiClientInfo<TRequest> : WebApiClientInfo
{
    public TRequest Request { get; set; }
}

#endregion WebApiClientInfo

