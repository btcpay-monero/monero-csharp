
using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Common;

public class MoneroJsonRpcRequest
{
    [JsonPropertyName("method")]
    public string Method { get; set; }
    [JsonPropertyName("jsonrpc")]
    public string Version { get; set; } = "2.0";

    [JsonPropertyName("params")]
    public object? Params { get; set; }

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
    [JsonPropertyName("error")]
    public MoneroRpcResponseError? Error { get; set; }

    [JsonPropertyName("result")]
    public T? Result { get; set; }
}