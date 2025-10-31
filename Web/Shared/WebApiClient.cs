using Newtonsoft.Json;

using System.Net.Http.Headers;

using Web.Shared.Exceptions;

namespace Web.Shared;

/// <summary>
/// Web Client - Get, Put, Delete and Post for WEB API
/// </summary>
public class WebApiClient(HttpClient httpClient)
{
    private bool _disposed = false;

    /// <summary>
    /// Finalizes an instance of the <see cref="WebClient"/> class.
    /// </summary>
    ~WebApiClient()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets or sets the HTTP headers.
    /// </summary>
    public IList<KeyValuePair<string, string>> Headers { get; set; } = new List<KeyValuePair<string, string>>();


    /// <summary>
    /// Gets using the specified method.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="info">The call information.</param>
    /// <returns></returns>
    public async Task<TResponse> Get<TResponse>(WebApiClientInfo<object> info)
        where TResponse : class
    {
        try
        {
            HttpResponseMessage message = await httpClient.GetAsync($"{info.Method}/{info.Request}");
            message.EnsureSuccessStatusCode();

            return await Read<TResponse, HttpGetException>(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Gets using the specified method.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="info">The call information.</param>
    /// <returns></returns>
    public async Task<TResponse> Get<TRequest, TResponse>(WebApiClientInfo<TRequest> info)
        where TResponse : class
        where TRequest : class
    {
        try
        {
            string strRequest = JsonConvert.SerializeObject(info.Request);

            StringContent content = new(strRequest);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            //TODO: Figure out why client.PostAsync(info.Method, content) does not send correct URL?
            HttpResponseMessage message = await httpClient.PostAsync(info.Method, content);
            message.EnsureSuccessStatusCode();

            return await Read<TResponse, HttpPostException>(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Posts using the specified method.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="info">The call information.</param>
    /// <returns></returns>
    public async Task<TResponse> Post<TRequest, TResponse>(WebApiClientInfo<TRequest> info)
        where TResponse : class
    {
        try
        {
            string strRequest = JsonConvert.SerializeObject(info.Request);

            StringContent content = new(strRequest);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            HttpResponseMessage message = await httpClient.PostAsync(info.Method, content);
            message.EnsureSuccessStatusCode();

            return await Read<TResponse, HttpPostException>(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Puts using the specified method.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="info">The call information.</param>
    /// <returns></returns>
    public async Task<TResponse> Put<TRequest, TResponse>(WebApiClientInfo<TRequest> info)
        where TResponse : class
    {
        try
        {
            string strRequest = JsonConvert.SerializeObject(info.Request);

            StringContent content = new(strRequest);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            HttpResponseMessage message = await httpClient.PutAsync(info.Method, content);
            message.EnsureSuccessStatusCode();

            return await Read<TResponse, HttpPutException>(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Deletes using the specified method.
    /// </summary>
    /// <typeparam name="TIdentity">The type of the identity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="method">The method.</param>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public async Task<TResponse> Delete<TIdentity, TResponse>(WebApiClientInfo info, TIdentity id)
        where TResponse : class
    {
        try
        {
            HttpResponseMessage message = httpClient.DeleteAsync($"{info.Method}/{id}").Result;
            message.EnsureSuccessStatusCode();

            return await Read<TResponse, HttpDeleteException>(message);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Delete using the specified method.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="info">The call information.</param>
    /// <returns></returns>
    public async Task<TResponse> Delete<TResponse>(WebApiClientInfo<object> info)
        where TResponse : class
    {
        try
        {
            HttpResponseMessage message = await httpClient.DeleteAsync($"{info.Method}/{info.Request}");
            message.EnsureSuccessStatusCode();

            return await Read<TResponse, HttpGetException>(message);
        }
        catch (Exception)
        {
            throw;
        }
    }


    #region Helpers

    /// <summary>
    /// Connects to the WEB API
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="userId"></param>
    /// <param name="sessionKey"></param>
    /// <returns></returns>
    private HttpClient Connect(string uri)
    {
        HttpClient client = new()
        {
            BaseAddress = new Uri(uri)
        };

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (Headers != null)
            foreach (KeyValuePair<string, string> kvp in Headers)
                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);

        return client;
    }


    private async Task<TResponse> Read<TResponse, TException>(HttpResponseMessage message)
        where TException : Exception, new()
        where TResponse : class
    {
        if (message.IsSuccessStatusCode)
        {
            string s = await message.Content.ReadAsStringAsync();

            TResponse obj = JsonConvert.DeserializeObject<TResponse>(s);

            return obj;
        }

        throw new TException();
    }

    #endregion


    #region IDisposable

    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Free unmanaged here.


            // Free any other managed objects here.
        }

        _disposed = true;
    }

    #endregion
}


#region WebApiClientInfo

/// <summary>
/// The WebApiClient Information.
/// </summary>
public class WebApiClientInfo
{
    public string Method { get; set; }
}

/// <summary>
/// The WebClient Information, with request.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public class WebApiClientInfo<TRequest> : WebApiClientInfo
{
    public TRequest Request { get; set; }
}

#endregion WebApiClientInfo

