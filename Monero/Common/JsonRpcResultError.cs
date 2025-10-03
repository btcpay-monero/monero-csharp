using System.Text.Json.Serialization;

namespace Monero.Common;

public class JsonRpcResultError(int code, string message, dynamic data)
{
    [JsonPropertyName("code")]
    public int Code { get; set; } = code;

    [JsonPropertyName("message")]
    public string Message { get; set; } = message;

    [JsonPropertyName("data")]
    public dynamic Data { get; set; } = data;
}