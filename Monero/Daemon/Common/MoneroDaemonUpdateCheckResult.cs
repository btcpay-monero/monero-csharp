
namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateCheckResult
{
    private bool? isUpdateAvailable;
    private string? version;
    private string? hash;
    private string? autoUri;
    private string? userUri;

    public MoneroDaemonUpdateCheckResult()
    {
        // nothing to construct
    }

    public MoneroDaemonUpdateCheckResult(MoneroDaemonUpdateCheckResult checkResult)
    {
        isUpdateAvailable = checkResult.isUpdateAvailable;
        version = checkResult.version;
        hash = checkResult.hash;
        autoUri = checkResult.autoUri;
        userUri = checkResult.userUri;
    }

    public MoneroDaemonUpdateCheckResult Clone()
    {
        return new MoneroDaemonUpdateCheckResult(this);
    }

    public bool? IsUpdateAvailable()
    {
        return isUpdateAvailable;
    }

    public void SetIsUpdateAvailable(bool? isUpdateAvailable)
    {
        this.isUpdateAvailable = isUpdateAvailable;
    }

    public string? GetVersion()
    {
        return version;
    }

    public void SetVersion(string? version)
    {
        this.version = version;
    }

    public string? GetHash()
    {
        return hash;
    }

    public void SetHash(string? hash)
    {
        this.hash = hash;
    }

    public string? GetAutoUri()
    {
        return autoUri;
    }

    public void SetAutoUri(string? autoUri)
    {
        this.autoUri = autoUri;
    }

    public string? GetUserUri()
    {
        return userUri;
    }

    public void SetUserUri(string? userUri)
    {
        this.userUri = userUri;
    }
}