
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Monero.Common
{
    public class MoneroRpcConnection : MoneroConnection
    {
        private string? _username;
        private string? _password;
        private string? _zmqUri;
        private bool? _isAuthenticated;
        
        private bool _printStackTrace;

        private HttpClient? _httpClient;
        
        public MoneroRpcConnection(string? uri = null, string? username = null, string? password = null, string? zmqUri = null, int priority = 0) : base(uri, null, priority)
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
                var uriObj = new Uri(_uri ?? "");
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
                var uriObj = new Uri(_uri ?? "");
                return uriObj.Host.EndsWith(".b32.i2p", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public bool IsClearnet() {
            if (string.IsNullOrEmpty(_uri)) return false;
            return !IsOnion() && !IsI2P();
        }

        public bool Equals(MoneroRpcConnection? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
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

                throw new MoneroRpcError(message, code,"");
            }
        }


        private void ValidateRpcResponse(MoneroJsonRpcResponse? rpcResponse, string method, object? parameters)
        {
            if (rpcResponse == null)
            {
                throw new MoneroRpcError($"Received null RPC response from RPC request with method '{method}' to {_uri}", -1, method, parameters);
            }

            if (rpcResponse.Error != null)
            {
                var message = rpcResponse.Error.ContainsKey("message") ? rpcResponse.Error["message"].ToString() : "";
                int code = rpcResponse.Error.ContainsKey("code") ? Convert.ToInt32(rpcResponse.Error["code"]) : -1;

                if (string.IsNullOrEmpty(message))
                {
                    message = $"Received error response from RPC request with method '{method}' to {_uri}";
                }

                throw new MoneroRpcError(message, code, method, parameters);
            }
        }

        private void ValidateRpcResponse(Dictionary<string, object?>? rpcResponse, string method, object? parameters)
        {
            if (rpcResponse == null)
            {
                throw new MoneroRpcError($"Received null RPC response from RPC request with method '{method}' to {_uri}", -1, method, parameters);
            }

            var error = (Dictionary<string, object?>?)rpcResponse.GetValueOrDefault("error");

            if (error != null)
            {
                var message = error.ContainsKey("message") ? error["message"]!.ToString() : "";
                int code = error.ContainsKey("code") ? Convert.ToInt32(error["code"]) : -1;

                if (string.IsNullOrEmpty(message))
                {
                    message = $"Received error response from RPC request with method '{method}' to {_uri}";
                }

                throw new MoneroRpcError(message, code, method, parameters);
            }
        }

        public override MoneroRpcConnection Clone()
        {
            return new MoneroRpcConnection(this);
        }

        public override bool? IsConnected()
        {
            if (_isAuthenticated != null) return _isOnline == true && _isAuthenticated == true;
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
                    var request = new MoneroJsonRpcRequest("get_version");
                    var response = SendJsonRequest(request, timeoutMs);
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

                    if (e is MoneroRpcError)
                    {
                        if (((MoneroRpcError)e).GetCode() == 401)
                        {
                            _isOnline = true;
                            _isAuthenticated = false;
                        }
                        else if (((MoneroRpcError)e).GetCode() == 404)
                        { // fallback to latency check
                            _isOnline = true;
                            _isAuthenticated = true;
                        }
                    }
                }
                var now = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (_isOnline == true) _responseTime = now - startTime;
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
                if (_httpClient != null) _httpClient.Dispose();
            }
            catch (Exception e)
            {
                throw new MoneroError(e);
            }

            if (string.IsNullOrEmpty(username)) username = null;
            if (string.IsNullOrEmpty(password)) password = null;

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
                
                var handler = new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(username, password)
                };
                
                if (!string.IsNullOrEmpty(_proxyUri))
                {
                    handler.Proxy = new WebProxy(_proxyUri, true);
                    handler.UseProxy = true;
                }
                _httpClient = new HttpClient(handler);
            }
            else
            {
                var handler = new HttpClientHandler();
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

        public MoneroJsonRpcResponse SendJsonRequest(MoneroJsonRpcRequest rpcRequest, ulong timeoutMs = 20000)
        {
            if (_httpClient == null) throw new MoneroError("Http client is null");

            try
            {
                string jsonBody = JsonConvert.SerializeObject(rpcRequest);
                string method = rpcRequest.Method;

                if (MoneroUtils.GetLogLevel() >= 0)
                {
                    MoneroUtils.Log(0, $"Sending JSON request with method='{method}', body={jsonBody}, uri={_uri}");
                }

                if (_printStackTrace)
                {
                    try
                    {
                        throw new Exception($"Debug stack trace for JSON request with method '{method}' and body {jsonBody}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                var httpResponse = _httpClient.PostAsync(new Uri(_uri + "/json_rpc"), content, cts.Token).GetAwaiter().GetResult();
                
                ValidateHttpResponse(httpResponse);

                string respStr = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var rpcResponse = JsonConvert.DeserializeObject<MoneroJsonRpcResponse>(respStr);

                if (MoneroUtils.GetLogLevel() >= 3)
                {
                    string shortResp = respStr.Length > 10000 ? respStr.Substring(0, 10000) : respStr;
                    MoneroUtils.Log(3, $"Received JSON response from method='{method}', response={shortResp}, uri={_uri}");
                }

                ValidateRpcResponse(rpcResponse, method, rpcRequest.Params);

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

        public MoneroJsonRpcResponse SendJsonRequest(string method, Dictionary<string, object?>? parameters = null, ulong timeoutMs = 20000)
        {
            return SendJsonRequest(new MoneroJsonRpcRequest(method, parameters), timeoutMs);
        }

        public MoneroJsonRpcResponse SendJsonRequest(string method, List<string> parameters, ulong timeoutMs = 20000)
        {
            return SendJsonRequest(new MoneroJsonRpcRequest(method, parameters), timeoutMs);
        }

        public MoneroJsonRpcStringResponse SendJsonStringRequest(string method, List<ulong> parameters, ulong timeoutMs = 20000)
        {
            if (_httpClient == null) throw new MoneroError("Http client is null");

            try
            {
                var rpcRequest = new MoneroJsonRpcRequest(method, parameters);
                string jsonBody = JsonConvert.SerializeObject(rpcRequest);

                if (MoneroUtils.GetLogLevel() >= 0)
                {
                    MoneroUtils.Log(0, $"Sending JSON request with method='{method}', body={jsonBody}, uri={_uri}");
                }

                if (_printStackTrace)
                {
                    try
                    {
                        throw new Exception($"Debug stack trace for JSON request with method '{method}' and body {jsonBody}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                var httpResponse = _httpClient.PostAsync(new Uri(_uri + "/json_rpc"), content, cts.Token).GetAwaiter().GetResult();
                
                ValidateHttpResponse(httpResponse);
                
                string respStr = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var rpcResponse = JsonConvert.DeserializeObject<MoneroJsonRpcStringResponse>(respStr);

                if (MoneroUtils.GetLogLevel() >= 3)
                {
                    string shortResp = respStr.Length > 10000 ? respStr.Substring(0, 10000) : respStr;
                    MoneroUtils.Log(3, $"Received JSON response from method='{method}', response={shortResp}, uri={_uri}");
                }
                
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

        public Dictionary<string, object> SendPathRequest(string path, Dictionary<string, object?>? parameters = null, ulong? timeoutMs = null)
        {
            if (_httpClient == null) throw new MoneroError("Http client is null");
            
            try
            {
                string jsonBody = "";
                
                if (parameters != null)
                {
                    jsonBody = JsonConvert.SerializeObject(parameters);
                }
                
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs ?? _timeoutMs));
                
                var httpResponse = _httpClient.PostAsync(new Uri($"{_uri}/{path}"), content, cts.Token).GetAwaiter().GetResult();

                if (MoneroUtils.GetLogLevel() >= 2)
                {
                    MoneroUtils.Log(2, $"Sending path request with path='{path}', params={JsonConvert.SerializeObject(parameters)}, uri={_uri}");
                }
                
                ValidateHttpResponse(httpResponse);

                var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string responseText = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var respMap = JsonConvert.DeserializeObject<Dictionary<string, object?>>(responseText);

                if (MoneroUtils.GetLogLevel() >= 3)
                {
                    string respStr = JsonConvert.SerializeObject(respMap);
                    if (respStr.Length > 10000) respStr = respStr.Substring(0, 10000);
                    MoneroUtils.Log(3, $"Received path response from path='{path}', response={respStr}, uri={_uri} ({DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime} ms)");
                }

                ValidateRpcResponse(respMap, path, parameters);
                return respMap!;
            }
            catch (MoneroRpcError)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new MoneroError(ex);
            }
        }

        public byte[] SendBinaryRequest(string method, Dictionary<string, object?>? parameters = null, ulong timeoutMs = 20000)
        {
            throw new NotImplementedException("SendBinaryRequest(): not implemented");
        }
    }
}
