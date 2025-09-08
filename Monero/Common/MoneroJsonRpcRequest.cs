using Newtonsoft.Json;

namespace Monero.Common
{
    public class MoneroJsonRpcRequest : MoneroHttpRequest
    {
        [JsonProperty("jsonrpc", Order = 0)]
        public readonly string Version = "2.0";

        [JsonProperty("method", Order = 1)]
        public readonly string Method;

        [JsonProperty("params", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        public object? Params;

        public MoneroJsonRpcRequest(string method, object? parameters = null)
        {
            Method = method;
            Params = parameters;
        }
    }

    public class MoneroJsonRpcResponse
    {
        [JsonProperty("result")]
        public Dictionary<string, object>? Result;

        [JsonProperty("error")]
        public Dictionary<string, object>? Error;
    }

    public class MoneroJsonRpcStringResponse
    {
        [JsonProperty("result")]
        public string? Result;

        [JsonProperty("error")]
        public Dictionary<string, object>? Error;
    }
}
