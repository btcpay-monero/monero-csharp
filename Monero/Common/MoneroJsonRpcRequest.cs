using Newtonsoft.Json;

namespace Monero.Common;

public class MoneroJsonRpcRequest : MoneroHttpRequest
{
    [JsonProperty("method", Order = 1)] public readonly string Method;
    [JsonProperty("jsonrpc", Order = 0)] public readonly string Version = "2.0";

    [JsonProperty("params", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
    public object? Params;

    public MoneroJsonRpcRequest(string method)
    {
        Method = method;
    }

    public MoneroJsonRpcRequest(string method, object? parameters)
    {
        Method = method;
        Params = parameters;
    }
}

public class MoneroJsonRpcResponse<T>
{
    [JsonProperty("error")] public Dictionary<string, object>? Error;

    [JsonProperty("result")] public T? Result;
}