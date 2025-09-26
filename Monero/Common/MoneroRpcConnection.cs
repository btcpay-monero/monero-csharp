using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Monero.Daemon.Common;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Monero.Common;

public class MoneroRpcConnection : MoneroConnection, IEquatable<MoneroRpcConnection>
{
    private readonly JsonSerializerOptions defaultSerializationOptions = new()
    { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private HttpClient? _httpClient;
    private bool? _isAuthenticated;
    private string? _password;
    private bool _printStackTrace;
    private string? _username;
    private string? _zmqUri;

    public MoneroRpcConnection()
    {
        SetCredentials(null, null);
    }

    public MoneroRpcConnection(string? uri) : base(uri)
    {
        SetCredentials(null, null);
    }

    public MoneroRpcConnection(string? uri, string? username, string? password) : base(uri)
    {
        SetCredentials(username, password);
    }

    public MoneroRpcConnection(string? uri, string? username, string? password, string? zmqUri) : base(uri)
    {
        _zmqUri = zmqUri;
        SetCredentials(username, password);
    }

    public MoneroRpcConnection(string? uri, string? username, string? password,
        string? zmqUri, int priority) : base(uri, null, priority)
    {
        _zmqUri = zmqUri;
        SetCredentials(username, password);
    }

    public MoneroRpcConnection(MoneroRpcConnection other) : base(other)
    {
        _username = other._username;
        _password = other._password;
        _zmqUri = other._zmqUri;
        _isAuthenticated = other._isAuthenticated;
        _printStackTrace = other._printStackTrace;
        SetCredentials(_username, _password);
    }

    #region Private Methods

    private static void ValidateHttpResponse(HttpResponseMessage resp)
    {
        int code = (int)resp.StatusCode;
        if (code < 200 || code > 299)
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
                message = $"Received error response from RPC request with method '{method}' to {_uri}";
            }

            throw new MoneroRpcError(message, code, method, parameters);
        }
    }

    private void ValidateRpcResponse<T>(MoneroJsonRpcResponse<T>? rpcResponse, string method, object? parameters)
    {
        if (rpcResponse == null)
        {
            throw new MoneroRpcInvalidResponseError(method, _uri, parameters);
        }

        HandleRpcError(rpcResponse.Error, method, parameters);
    }

    private void ValidateRpcResponse<T>(T? rpcResponse, string method, object? parameters)
    {
        if (rpcResponse == null)
        {
            throw new MoneroRpcInvalidResponseError(method, _uri, parameters);
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

    private async Task<T?> SendRequest<T>(string path, object? parameters, ulong? timeoutMs)
    {
        if (_httpClient == null)
        {
            throw new MoneroError("Http client is null");
        }

        try
        {
            string jsonBody = "";

            if (parameters != null)
            {
                jsonBody = JsonSerializer.Serialize(parameters, defaultSerializationOptions);
            }

            if (_printStackTrace)
            {
                try
                {
                    throw new Exception($"Debug stack trace for JSON request with method '{path}' and body {jsonBody}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

            using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(timeoutMs ?? _timeoutMs));

            HttpResponseMessage httpResponse = _httpClient.PostAsync(new Uri($"{_uri}/{path}"), content, cts.Token)
                .GetAwaiter()
                .GetResult();

            ValidateHttpResponse(httpResponse);

            string respStr = await httpResponse.Content.ReadAsStringAsync();
            CancellationToken cts2 = new();
            using Stream ms = await ByteArrayToMemoryStream(httpResponse).ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<T>(ms, defaultSerializationOptions, cts2);
        }
        catch (Exception e2)
        {
            throw new MoneroError(e2);
        }
    }

    private static async Task<Stream> ByteArrayToMemoryStream(HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        return new MemoryStream(responseBody);
    }

    #endregion

    #region Public Methods

    public bool IsOnion()
    {
        try
        {
            Uri uriObj = new(_uri ?? "");
            return uriObj.Host.EndsWith(".onion", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public bool IsI2P()
    {
        try
        {
            Uri uriObj = new(_uri ?? "");
            return uriObj.Host.EndsWith(".b32.i2p", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public bool IsClearnet()
    {
        if (string.IsNullOrEmpty(_uri))
        {
            return false;
        }

        return !IsOnion() && !IsI2P();
    }

    public override bool Equals(object? other)
    {
        return Equals(other as MoneroRpcConnection);
    }

    public bool Equals(MoneroRpcConnection? other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _username == other._username &&
               _password == other._password &&
               _uri == other._uri &&
               _proxyUri == other._proxyUri &&
               _zmqUri == other._zmqUri;
    }

    public override MoneroRpcConnection Clone()
    {
        return new MoneroRpcConnection(this);
    }

    public override bool? IsConnected()
    {
        if (_isAuthenticated != null)
        {
            return _isOnline == true && _isAuthenticated == true;
        }

        return _isOnline;
    }

    public override bool? IsAuthenticated()
    {
        return _isAuthenticated;
    }

    public async Task<bool> CheckConnection()
    {
        return await CheckConnection(_timeoutMs);
    }

    public override async Task<bool> CheckConnection(ulong timeoutMs)
    {
        await _semaphore.WaitAsync();

        bool? isOnlineBefore = _isOnline;
        bool? isAuthenticatedBefore = _isAuthenticated;
        ulong startTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        try
        {
            MoneroJsonRpcRequest request = new("get_version");
            MoneroJsonRpcResponse<Dictionary<string, object>>? response = await
                SendJsonRequest<Dictionary<string, object>>(request, timeoutMs);
            if (response == null)
            {
                throw new Exception("Invalid response");
            }

            _isOnline = true;
            _isAuthenticated = true;
        }
        catch (Exception e)
        {
            _isOnline = false;
            _isAuthenticated = null;
            _responseTime = null;

            if (e is MoneroRpcError error)
            {
                if (error.GetCode() == 401)
                {
                    _isOnline = true;
                    _isAuthenticated = false;
                }
                else if (error.GetCode() == 404)
                {
                    // fallback to latency check
                    _isOnline = true;
                    _isAuthenticated = true;
                }
            }
        }

        ulong now = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (_isOnline == true)
        {
            _responseTime = now - startTime;
        }

        _semaphore.Release();

        return isOnlineBefore != _isOnline || isAuthenticatedBefore != _isAuthenticated;
    }

    public override MoneroRpcConnection SetAttribute(string key, string value)
    {
        _attributes[key] = value;
        return this;
    }

    public override MoneroRpcConnection SetUri(string? uri)
    {
        _uri = uri;
        SetCredentials(_username, _password);
        return this;
    }

    public override MoneroRpcConnection SetProxyUri(string? uri)
    {
        _proxyUri = uri;
        return this;
    }

    public override MoneroRpcConnection SetPriority(int priority)
    {
        _priority = priority;
        return this;
    }

    public string? GetUsername()
    {
        return _username;
    }

    public string? GetPassword()
    {
        return _password;
    }

    public MoneroRpcConnection SetCredentials(string? username, string? password)
    {
        try
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
        catch (Exception e)
        {
            throw new MoneroError(e);
        }

        if (string.IsNullOrEmpty(username))
        {
            username = null;
        }

        if (string.IsNullOrEmpty(password))
        {
            password = null;
        }

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

            HttpClientHandler handler = new() { Credentials = new NetworkCredential(username, password) };

            if (!string.IsNullOrEmpty(_proxyUri))
            {
                handler.Proxy = new WebProxy(_proxyUri, true);
                handler.UseProxy = true;
            }

            _httpClient = new HttpClient(handler);
        }
        else
        {
            HttpClientHandler handler = new();
            if (!string.IsNullOrEmpty(_proxyUri))
            {
                handler.Proxy = new WebProxy(_proxyUri, true);
                handler.UseProxy = true;
            }

            _httpClient = new HttpClient(handler);
        }

        if (_username != username || _password != password)
        {
            _isOnline = null;
            _isAuthenticated = null;
        }

        _username = username;
        _password = password;
        return this;
    }

    public string? GetZmqUri()
    {
        return _zmqUri;
    }

    public MoneroRpcConnection SetZmqUri(string? zmqUri)
    {
        _zmqUri = zmqUri;
        return this;
    }

    public void SetPrintStackTrace(bool printStackTrace)
    {
        _printStackTrace = printStackTrace;
    }

    #region Json Request

    public async Task<MoneroJsonRpcResponse<T>> SendJsonRequest<T>(string method,
        Dictionary<string, object?>? parameters, ulong timeoutMs)
    {
        return await SendJsonRequest<T>(new MoneroJsonRpcRequest(method, parameters),
            timeoutMs);
    }

    public async Task<MoneroJsonRpcResponse<T>> SendJsonRequest<T>(string method,
        List<string> parameters)
    {
        return await SendJsonRequest<T>(method, parameters, 20000);
    }

    public async Task<MoneroJsonRpcResponse<T>> SendJsonRequest<T>(string method,
        List<string> parameters,
        ulong timeoutMs)
    {
        return await SendJsonRequest<T>(new MoneroJsonRpcRequest(method, parameters),
            timeoutMs);
    }

    public async Task<MoneroJsonRpcResponse<string>> SendJsonRequest(string method, List<ulong> parameters)
    {
        return await SendJsonRequest(method, parameters, 20000);
    }

    public async Task<MoneroJsonRpcResponse<string>> SendJsonRequest(string method, List<ulong> parameters,
        ulong timeoutMs)
    {
        return await SendJsonRequest<string>(new MoneroJsonRpcRequest(method, parameters), timeoutMs);
    }

    public async Task<MoneroJsonRpcResponse<T>> SendJsonRequest<T>(string method)
    {
        return await SendJsonRequest<T>(new MoneroJsonRpcRequest(method, null));
    }

    public async Task<MoneroJsonRpcResponse<T>> SendJsonRequest<T>(string method,
        Dictionary<string, object?>? parameters)
    {
        return await SendJsonRequest<T>(new MoneroJsonRpcRequest(method, parameters));
    }

    public async Task<MoneroJsonRpcResponse<T>> SendJsonRequest<T>(MoneroJsonRpcRequest rpcRequest)
    {
        return await SendJsonRequest<T>(rpcRequest, 20000);
    }

    public async Task<MoneroJsonRpcResponse<T>> SendJsonRequest<T>(MoneroJsonRpcRequest rpcRequest, ulong timeoutMs)
    {
        if (_httpClient == null)
        {
            throw new MoneroError("Http client is null");
        }

        try
        {
            MoneroJsonRpcResponse<T>? rpcResponse =
                await SendRequest<MoneroJsonRpcResponse<T>>("json_rpc", rpcRequest, timeoutMs);

            ValidateRpcResponse(rpcResponse, rpcRequest.Method, rpcRequest.Params);

            return rpcResponse!;
        }
        catch (MoneroRpcError)
        {
            throw;
        }
        catch (Exception e2)
        {
            throw new MoneroError(e2);
        }
    }

    #endregion

    #region Path Request

    public async Task<T> SendPathRequest<T>(string path)
    {
        return await SendPathRequest<T>(path, null);
    }

    public async Task<T> SendPathRequest<T>(string path, Dictionary<string, object?>? parameters)
    {
        return await SendPathRequest<T>(path, parameters, null);
    }

    public async Task<T> SendPathRequest<T>(string path, Dictionary<string, object?>? parameters,
        ulong? timeoutMs)
    {
        if (_httpClient == null)
        {
            throw new MoneroError("Http client is null");
        }

        try
        {
            T? respMap =
                await SendRequest<T>(path, parameters, timeoutMs);

            ValidateRpcResponse(respMap, path, parameters);
            return respMap!;
        }
        catch (Exception ex)
        {
            throw new MoneroError(ex);
        }
    }

    #endregion

    #region Binary Request

    public async Task<byte[]> SendBinaryRequest(string method)
    {
        return await SendBinaryRequest(method, null);
    }

    public async Task<byte[]> SendBinaryRequest(string method, Dictionary<string, object?>? parameters)
    {
        return await SendBinaryRequest(method, parameters, 20000);
    }

    public Task<byte[]> SendBinaryRequest(string method, Dictionary<string, object?>? parameters, ulong timeoutMs)
    {
        throw new NotImplementedException("MoneroRpcConnection.SendBinaryRequest(): not implemented");
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    #endregion

    #endregion
}