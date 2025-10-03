using System.Text.Json.Serialization;

namespace Monero.Common;

public class JsonRpcCommand<T>(string method, T parameters)
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("method")]
    public string Method { get; set; } = method;

    [JsonPropertyName("params")]
    public T Parameters { get; set; } = parameters;
}