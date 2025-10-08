using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateCheckResponse : MoneroRpcResponse
{
    [JsonPropertyName("auto_uri")]
    public string? AutoUri { get; set; }

    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    [JsonPropertyName("update")]
    public bool? IsUpdateAvailable { get; set; }

    [JsonPropertyName("user_uri")]
    public string? UserUri { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}