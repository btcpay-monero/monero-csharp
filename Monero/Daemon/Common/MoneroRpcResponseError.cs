using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroRpcResponseError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}