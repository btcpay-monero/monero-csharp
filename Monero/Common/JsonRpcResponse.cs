using System.Text.Json.Serialization;

namespace Monero.Common;

public class JsonRpcResponse<T>(T result, JsonRpcResponseError error, string id)
{
    [JsonPropertyName("result")]
    public T Result { get; set; } = result;

    [JsonPropertyName("error")]
    public JsonRpcResponseError Error { get; set; } = error;

    [JsonPropertyName("id")]
    public string Id { get; set; } = id;
}