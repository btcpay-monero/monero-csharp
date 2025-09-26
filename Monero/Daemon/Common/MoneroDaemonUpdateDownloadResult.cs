using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateDownloadResult : MoneroDaemonUpdateCheckResult
{
    [JsonPropertyName("name")]
    [JsonInclude]
    private string? _downloadPath { get; set; }

    public MoneroDaemonUpdateDownloadResult(MoneroDaemonUpdateCheckResult checkResult) : base(checkResult)
    {
    }

    public MoneroDaemonUpdateDownloadResult(MoneroDaemonUpdateDownloadResult checkResult) : base(checkResult)
    {
        _downloadPath = checkResult._downloadPath;
    }

    public string? GetDownloadPath()
    {
        return _downloadPath;
    }

    public void SetDownloadPath(string? downloadPath)
    {
        _downloadPath = downloadPath;
    }
}