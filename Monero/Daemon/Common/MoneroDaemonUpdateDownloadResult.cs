
namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateDownloadResult : MoneroDaemonUpdateCheckResult
{
    private string? downloadPath;

    public MoneroDaemonUpdateDownloadResult(MoneroDaemonUpdateCheckResult checkResult) : base(checkResult)
    {
    }

    public MoneroDaemonUpdateDownloadResult(MoneroDaemonUpdateDownloadResult checkResult) : base(checkResult)
    {
        downloadPath = checkResult.downloadPath;
    }

    public string? GetDownloadPath()
    {
        return downloadPath;
    }

    public void SetDownloadPath(string? downloadPath)
    {
        this.downloadPath = downloadPath;
    }
}