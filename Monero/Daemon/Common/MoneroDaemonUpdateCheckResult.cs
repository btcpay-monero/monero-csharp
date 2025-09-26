using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroDaemonUpdateCheckResult : MoneroRpcResponse
{
    [JsonPropertyName("auto_uri")]
    [JsonInclude]
    private string? _autoUri { get; set; }
    [JsonPropertyName("hash")]
    [JsonInclude]
    private string? _hash { get; set; }
    [JsonPropertyName("update")]
    [JsonInclude]
    private bool? _isUpdateAvailable { get; set; }
    [JsonPropertyName("user_uri")]
    [JsonInclude]
    private string? _userUri { get; set; }
    [JsonInclude]
    [JsonPropertyName("version")]
    private string? _version { get; set; }

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
        _isUpdateAvailable = isUpdateAvailable;
    }

    public string? GetVersion()
    {
        return _version;
    }

    public void SetVersion(string? version)
    {
        _version = version;
    }

    public string? GetHash()
    {
        return _hash;
    }

    public void SetHash(string? hash)
    {
        _hash = hash;
    }

    public string? GetAutoUri()
    {
        return _autoUri;
    }

    public void SetAutoUri(string? autoUri)
    {
        _autoUri = autoUri;
    }

    public string? GetUserUri()
    {
        return _userUri;
    }

    public void SetUserUri(string? userUri)
    {
        _userUri = userUri;
    }
}