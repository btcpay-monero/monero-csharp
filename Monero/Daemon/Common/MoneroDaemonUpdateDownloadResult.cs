using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateDownloadResult : MoneroDaemonUpdateCheckResult
{
    [JsonPropertyName("name")]
    public string? DownloadPath { get; set; }
}