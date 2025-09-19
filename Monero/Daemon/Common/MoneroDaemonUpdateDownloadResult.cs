namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateDownloadResult : MoneroDaemonUpdateCheckResult
{
    private string? _downloadPath;

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