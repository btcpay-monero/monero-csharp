using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace Monero.Common;

public class MoneroRpcConnection : MoneroConnection
{
    private HttpClient? _httpClient;
    private bool? _isAuthenticated;
    private string? _password;

    private bool _printStackTrace;
    private string? _username;
    private string? _zmqUri;

    public MoneroRpcConnection(string? uri = null, string? username = null, string? password = null,
        string? zmqUri = null, int priority = 0) : base(uri, null, priority)
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

    private void HandleRpcError(Dictionary<string, object>? error, string method, object? parameters)
    {
        if (error != null)
        {
            string? message = error.ContainsKey("message") ? error["message"].ToString() : "";
            int code = error.ContainsKey("code") ? Convert.ToInt32(error["code"]) : -1;

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

    private void ValidateRpcResponse(Dictionary<string, object>? rpcResponse, string method, object? parameters)
    {
        if (rpcResponse == null)
        {
            throw new MoneroRpcInvalidResponseError(method, _uri, parameters);
        }

        Dictionary<string, object>? error = (Dictionary<string, object>?)rpcResponse.GetValueOrDefault("error");

        HandleRpcError(error, method, parameters);
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

    public bool CheckConnection()
    {
        return CheckConnection(_timeoutMs);
    }

    public override bool CheckConnection(ulong timeoutMs)
    {
        lock (this)
        {
            bool? isOnlineBefore = _isOnline;
            bool? isAuthenticatedBefore = _isAuthenticated;
            ulong startTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            try
            {
                MoneroJsonRpcRequest request = new("get_version");
                MoneroJsonRpcResponse<Dictionary<string, object>>? response =
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

            return isOnlineBefore != _isOnline || isAuthenticatedBefore != _isAuthenticated;
        }
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

    public MoneroJsonRpcResponse<Dictionary<string, object>> SendJsonRequest(string method,
        Dictionary<string, object?>? parameters = null, ulong timeoutMs = 20000)
    {
        return SendJsonRequest<Dictionary<string, object>>(new MoneroJsonRpcRequest(method, parameters), timeoutMs);
    }

    public MoneroJsonRpcResponse<Dictionary<string, object>> SendJsonRequest(string method, List<string> parameters,
        ulong timeoutMs = 20000)
    {
        return SendJsonRequest<Dictionary<string, object>>(new MoneroJsonRpcRequest(method, parameters), timeoutMs);
    }

    public MoneroJsonRpcResponse<string> SendJsonRequest(string method, List<ulong> parameters, ulong timeoutMs = 20000)
    {
        return SendJsonRequest<string>(new MoneroJsonRpcRequest(method, parameters), timeoutMs);
    }

    public MoneroJsonRpcResponse<T> SendJsonRequest<T>(MoneroJsonRpcRequest rpcRequest, ulong timeoutMs = 20000)
    {
        if (_httpClient == null)
        {
            throw new MoneroError("Http client is null");
        }

        try
        {
            MoneroJsonRpcResponse<T>? rpcResponse =
                SendRequest<MoneroJsonRpcResponse<T>>("json_rpc", rpcRequest, timeoutMs);

            ValidateRpcResponse(rpcResponse, rpcRequest.Method, rpcRequest.Params);

            return rpcResponse!;
        }
        catch (Exception e2)
        {
            throw new MoneroError(e2);
        }
    }

    public Dictionary<string, object> SendPathRequest(string path, Dictionary<string, object?>? parameters = null,
        ulong? timeoutMs = null)
    {
        if (_httpClient == null)
        {
            throw new MoneroError("Http client is null");
        }

        try
        {
            Dictionary<string, object>? respMap = SendRequest<Dictionary<string, object>>(path, parameters, timeoutMs);

            ValidateRpcResponse(respMap, path, parameters);
            return respMap!;
        }
        catch (Exception ex)
        {
            throw new MoneroError(ex);
        }
    }

    public byte[] SendBinaryRequest(string method, Dictionary<string, object?>? parameters = null,
        ulong timeoutMs = 20000)
    {
        throw new NotImplementedException("MoneroRpcConnection.SendBinaryRequest(): not implemented");
    }

    private T? SendRequest<T>(string path, object? parameters, ulong? timeoutMs)
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
                jsonBody = JsonConvert.SerializeObject(parameters);
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

            string respStr = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return JsonConvert.DeserializeObject<T>(respStr);
        }
        catch (Exception e2)
        {
            throw new MoneroError(e2);
        }
    }
}