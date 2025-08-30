
using Newtonsoft.Json;
using System.Net;

namespace Monero.Common
{
    public class MoneroRpcConnection : MoneroConnection
    {
        private string? _username;
        private string? _password;
        private string? _zmqUri;
        private bool? _isAuthenticated;
        
        private bool _printStackTrace = false;
        
        public MoneroRpcConnection(string? uri = null, string? username = null, string? password = null, string? zmqUri = null, int priority = 0) : base(uri, null, priority)
        {
            _username = username;
            _password = password;
            _zmqUri = zmqUri;
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

        public bool IsI2p()
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
            return !IsOnion() && !IsI2p();
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

        public MoneroRpcConnection(MoneroRpcConnection other) : base(other)
        {
            _username = other._username;
            _password = other._password;
            _zmqUri = other._zmqUri;
            _isAuthenticated = other._isAuthenticated;
            _printStackTrace = other._printStackTrace;
        }

        private static void ValidateHttpResponse(HttpWebResponse resp)
        {
            int code = (int)resp.StatusCode;
            if (code < 200 || code > 299)
            {
                string? content = null;
                try
                {
                    using (var reader = new StreamReader(resp.GetResponseStream()!))
                    {
                        content = reader.ReadToEnd();
                    }
                }
                catch
                {
                    // impossibile leggere il contenuto, ignoriamo
                }

                string message = $"{code} {resp.StatusDescription}";
                if (!string.IsNullOrEmpty(content))
                {
                    message += $": {content}";
                }

                throw new MoneroRpcError(message, code, null, null);
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

        private void ValidateRpcResponse(Dictionary<string, object>? rpcResponse, string method, object? parameters)
        {
            if (rpcResponse == null)
            {
                throw new MoneroRpcError($"Received null RPC response from RPC request with method '{method}' to {_uri}", -1, method, parameters);
            }

            var error = (Dictionary<string, object>?)rpcResponse.GetValueOrDefault("error");

            if (error != null)
            {
                var message = error.ContainsKey("message") ? error["message"].ToString() : "";
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

                    if (e as MoneroRpcError != null)
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
                if (_isOnline == true) _responseTime = now - (ulong)startTime;
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

        public MoneroJsonRpcResponse SendJsonRequest(MoneroJsonRpcRequest rpcRequest, ulong timeoutMs = 2000)
        {
            HttpWebResponse? resp = null;

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

                var request = (HttpWebRequest)WebRequest.Create(new Uri(_uri + "/json_rpc"));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = (int)(timeoutMs);

                if (!string.IsNullOrEmpty(_proxyUri))
                {
                    request.Proxy = new WebProxy(_proxyUri, true);
                }

                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(jsonBody);
                }

                resp = (HttpWebResponse)request.GetResponse();
                
                ValidateHttpResponse(resp);

                string respStr;
                using (var reader = new StreamReader(resp.GetResponseStream()!))
                {
                    respStr = reader.ReadToEnd();
                }

                var rpcResponse = JsonConvert.DeserializeObject<MoneroJsonRpcResponse>(respStr);

                if (MoneroUtils.GetLogLevel() >= 3)
                {
                    string shortResp = respStr.Length > 10000 ? respStr.Substring(0, 10000) : respStr;
                    MoneroUtils.Log(3, $"Received JSON response from method='{method}', response={shortResp}, uri={_uri}");
                }

                ValidateRpcResponse(rpcResponse, method, rpcRequest.Params);

                return rpcResponse;
            }
            catch (MoneroRpcError)
            {
                throw;
            }
            catch (Exception e2)
            {
                throw new MoneroError(e2);
            }
            finally
            {
                resp?.Close();
            }
        }

        public MoneroJsonRpcResponse SendJsonRequest(string method, Dictionary<string, object>? parameters = null, ulong timeoutMs = 2000)
        {
            return SendJsonRequest(new MoneroJsonRpcRequest(method, parameters), timeoutMs);
        }

        public MoneroJsonRpcResponse SendJsonRequest(string method, List<string> parameters, ulong timeoutMs = 2000)
        {
            return SendJsonRequest(new MoneroJsonRpcRequest(method, parameters), timeoutMs);
        }

        public MoneroJsonRpcStringResponse SendJsonStringRequest(string method, List<ulong> parameters, ulong timeoutMs = 2000)
        {
            HttpWebResponse? resp = null;

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

                var request = (HttpWebRequest)WebRequest.Create(new Uri(_uri + "/json_rpc"));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = (int)(timeoutMs);

                if (!string.IsNullOrEmpty(_proxyUri))
                {
                    request.Proxy = new WebProxy(_proxyUri, true);
                }

                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(jsonBody);
                }

                resp = (HttpWebResponse)request.GetResponse();

                ValidateHttpResponse(resp);

                string respStr;
                using (var reader = new StreamReader(resp.GetResponseStream()!))
                {
                    respStr = reader.ReadToEnd();
                }

                var rpcResponse = JsonConvert.DeserializeObject<MoneroJsonRpcStringResponse>(respStr);

                if (MoneroUtils.GetLogLevel() >= 3)
                {
                    string shortResp = respStr.Length > 10000 ? respStr.Substring(0, 10000) : respStr;
                    MoneroUtils.Log(3, $"Received JSON response from method='{method}', response={shortResp}, uri={_uri}");
                }

                //ValidateRpcResponse(rpcResponse, method, rpcRequest.Params);

                return rpcResponse;
            }
            catch (MoneroRpcError)
            {
                throw;
            }
            catch (Exception e2)
            {
                throw new MoneroError(e2);
            }
            finally
            {
                resp?.Close();
            }
        }

        public Dictionary<string, object> SendPathRequest(string path, Dictionary<string, object>? parameters = null, long? timeoutMs = null)
        {
            HttpWebResponse? resp = null;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create($"{_uri}/{path}");
                request.Method = "POST";
                request.ContentType = "application/json";

                if (!string.IsNullOrEmpty(_proxyUri))
                {
                    request.Proxy = new WebProxy(_proxyUri, true);
                }

                if (timeoutMs != null)
                {
                    request.Timeout = (int)timeoutMs.Value;
                }
                else
                {
                    request.Timeout = (int)_timeoutMs;
                }

                if (parameters != null)
                {
                    string jsonBody = JsonConvert.SerializeObject(parameters);
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(jsonBody);
                    }
                }

                if (MoneroUtils.GetLogLevel() >= 2)
                {
                    MoneroUtils.Log(2, $"Sending path request with path='{path}', params={JsonConvert.SerializeObject(parameters)}, uri={_uri}");
                }

                long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                resp = (HttpWebResponse)request.GetResponse();

                ValidateHttpResponse(resp);

                string responseText;
                using (var reader = new StreamReader(resp.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                }
                var respMap = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseText);

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
            finally
            {
                resp?.Close();
            }
        }

        public byte[] SendBinaryRequest(string method, Dictionary<string, object>? parameters = null, ulong timeoutMs = 2000)
        {
            throw new NotImplementedException("SendBinaryRequest(): not implemented");
        }
    }
}
