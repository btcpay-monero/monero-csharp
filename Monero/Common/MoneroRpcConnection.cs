using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Monero.Daemon.Common;

using JsonSerializer = System.Text.Json.JsonSerializer;
using MoneroJsonRpcParams = System.Collections.Generic.Dictionary<string, object?>;

namespace Monero.Common;

public class MoneroRpcConnection(Uri uri, string username, string password, HttpClient? client = null)
{
    private HttpClient _httpClient = client ?? new HttpClient();
    private string _password = password;
    private string _username = username;

    public async Task<TResponse> SendCommandAsync<TRequest, TResponse>(string method, TRequest data,
        CancellationToken cts = default)
    {
        var jsonSerializer = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(uri, "json_rpc"),
            Content = new StringContent(
                JsonSerializer.Serialize(new JsonRpcCommand<TRequest>(method, data), jsonSerializer),
                Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Accept.Clear();
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.Default.GetBytes($"{_username}:{_password}")));

        HttpResponseMessage rawResult = await _httpClient.SendAsync(httpRequest, cts);
        rawResult.EnsureSuccessStatusCode();
        var rawJson = await rawResult.Content.ReadAsStringAsync(cts);

        JsonRpcResult<TResponse>? response;
        try
        {
            response = JsonSerializer.Deserialize<JsonRpcResult<TResponse>>(rawJson, jsonSerializer);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(rawJson);
            throw;
        }

        if (response == null)
        {
            throw new JsonRpcApiException(new JsonRpcResultError(500, "Response is null", new Object()));
        }

        if (response.Error != null)
        {
            throw new JsonRpcApiException(response.Error);
        }

        return response.Result;
    }

    private async Task<T?> SendRequest<T>(string path, object? parameters, CancellationToken cts = default)
    {
        try
        {
            string jsonBody = "";

            var jsonSerializer = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            if (parameters != null)
            {
                jsonBody = JsonSerializer.Serialize(parameters, jsonSerializer);
            }

            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage httpResponse = _httpClient.PostAsync(new Uri($"{uri.AbsoluteUri}{path}"), content, cts)
                .GetAwaiter()
                .GetResult();

            ValidateHttpResponse(httpResponse);

            await httpResponse.Content.ReadAsStringAsync(cts);
            await using Stream ms = await ByteArrayToMemoryStream(httpResponse).ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<T>(ms, jsonSerializer, CancellationToken.None);
        }
        catch (Exception e)
        {
            throw new MoneroError(e);
        }
    }

    private static async Task<Stream> ByteArrayToMemoryStream(HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        return new MemoryStream(responseBody);
    }

    public string GetUri()
    {
        return uri.AbsoluteUri;
    }

    public string GetUsername()
    {
        return _username;
    }

    public string GetPassword()
    {
        return _password;
    }

    public void SetCredentials(string username, string password)
    {
        if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new MoneroError("username cannot be empty because password is not empty");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new MoneroError("password cannot be empty because username is not empty");
            }

            _httpClient = new HttpClient(new HttpClientHandler { Credentials = new NetworkCredential(username, password) });
        }
        else
        {
            _httpClient = new HttpClient(new HttpClientHandler());
        }

        _username = username;
        _password = password;
    }

    // TODO migrate to SendCommandAsync
    public async Task<MoneroJsonRpcResponse<T>> SendJsonRequest<T>(string method, MoneroJsonRpcParams parameters)
    {
        MoneroJsonRpcRequest rpcRequest = new(method, parameters);
        MoneroJsonRpcResponse<T>? rpcResponse =
            await SendRequest<MoneroJsonRpcResponse<T>>("json_rpc", rpcRequest);

        ValidateRpcResponse(rpcResponse, rpcRequest.Method, rpcRequest.Params);
        return rpcResponse!;
    }

    public async Task<T> SendPathRequest<T>(string path, MoneroJsonRpcParams parameters)
    {
        try
        {
            T? respMap = await SendRequest<T>(path, parameters);

            ValidateRpcResponse(respMap, path, parameters);
            return respMap!;
        }
        catch (Exception ex)
        {
            throw new MoneroError(ex);
        }
    }

    private static void ValidateHttpResponse(HttpResponseMessage resp)
    {
        int code = (int)resp.StatusCode;
        if (code is < 200 or > 299)
        {
            string? content = null;
            try
            {
                content = resp.Content.ReadAsStringAsync().Result;
            }
            catch
            {
                // could not get content
            }

            string message = $"{code} {resp.ReasonPhrase}";
            if (!string.IsNullOrEmpty(content))
            {
                message += $": {content}";
            }

            throw new MoneroRpcError(message, code, "");
        }
    }

    private void HandleRpcError(MoneroRpcResponseError? error, string method, object? parameters)
    {
        if (error != null)
        {
            string? message = error.Message;
            int code = error.Code;

            if (string.IsNullOrEmpty(message))
            {
                message = $"Received error response from RPC request with method '{method}' to {uri.AbsoluteUri}";
            }

            throw new MoneroRpcError(message, code, method, parameters);
        }
    }

    private void ValidateRpcResponse<T>(MoneroJsonRpcResponse<T>? rpcResponse, string method, object? parameters)
    {
        if (rpcResponse == null)
        {
            throw new MoneroRpcInvalidResponseError(method, uri, parameters);
        }

        HandleRpcError(rpcResponse.Error, method, parameters);
    }

    private void ValidateRpcResponse<T>(T? rpcResponse, string method, object? parameters)
    {
        if (rpcResponse == null)
        {
            throw new MoneroRpcInvalidResponseError(method, uri, parameters);
        }

        if (rpcResponse is MoneroRpcResponse resp)
        {
            HandleRpcError(resp.Error, method, parameters);
        }
        else
        {
            throw new MoneroError("Invalid rpc response");
        }
    }
}