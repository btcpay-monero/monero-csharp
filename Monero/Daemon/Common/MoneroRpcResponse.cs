using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroRpcResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("error")]
    public MoneroRpcResponseError? Error { get; set; }

    [JsonPropertyName("untrusted")]
    public bool? Untrusted { get; set; }
}