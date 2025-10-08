using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateDownloadResponse : MoneroDaemonUpdateCheckResponse
{
    [JsonPropertyName("name")]
    public string? DownloadPath { get; set; }
}