using System.Text.Json.Serialization;

namespace Monero.Common;

public class JsonRpcResult<T>(T result, JsonRpcResultError error, string id)
{
    [JsonPropertyName("result")]
    public T Result { get; set; } = result;

    [JsonPropertyName("error")]
    public JsonRpcResultError Error { get; set; } = error;

    [JsonPropertyName("id")]
    public string Id { get; set; } = id;
}