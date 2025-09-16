namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateCheckResult
{
    private string? _autoUri;
    private string? _hash;
    private bool? _isUpdateAvailable;
    private string? _userUri;
    private string? _version;

    public MoneroDaemonUpdateCheckResult()
    {
        // nothing to construct
    }

    public MoneroDaemonUpdateCheckResult(MoneroDaemonUpdateCheckResult checkResult)
    {
        _isUpdateAvailable = checkResult._isUpdateAvailable;
        _version = checkResult._version;
        _hash = checkResult._hash;
        _autoUri = checkResult._autoUri;
        _userUri = checkResult._userUri;
    }

    public MoneroDaemonUpdateCheckResult Clone()
    {
        return new MoneroDaemonUpdateCheckResult(this);
    }

    public bool? IsUpdateAvailable()
    {
        return _isUpdateAvailable;
    }

    public void SetIsUpdateAvailable(bool? isUpdateAvailable)
    {
        this._isUpdateAvailable = isUpdateAvailable;
    }

    public string? GetVersion()
    {
        return _version;
    }

    public void SetVersion(string? version)
    {
        this._version = version;
    }

    public string? GetHash()
    {
        return _hash;
    }

    public void SetHash(string? hash)
    {
        this._hash = hash;
    }

    public string? GetAutoUri()
    {
        return _autoUri;
    }

    public void SetAutoUri(string? autoUri)
    {
        this._autoUri = autoUri;
    }

    public string? GetUserUri()
    {
        return _userUri;
    }

    public void SetUserUri(string? userUri)
    {
        this._userUri = userUri;
    }
}