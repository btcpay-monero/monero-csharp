using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroHardForkInfo : MoneroRpcPaymentInfo
{
    [JsonPropertyName("earliest_height")]
    [JsonInclude]
    private ulong? _earliestHeight { get; set; }
    [JsonPropertyName("enabled")]
    [JsonInclude]
    private bool? _isEnabled { get; set; }
    [JsonPropertyName("votes")]
    [JsonInclude]
    private uint? _numVotes { get; set; }
    [JsonPropertyName("state")]
    [JsonInclude]
    private uint? _state { get; set; }
    [JsonPropertyName("threshold")]
    [JsonInclude]
    private uint? _threshold { get; set; }
    [JsonPropertyName("version")]
    [JsonInclude]
    private uint? _version { get; set; }
    [JsonPropertyName("voting")]
    [JsonInclude]
    private uint? _voting { get; set; }
    [JsonPropertyName("window")]
    [JsonInclude]
    private uint? _window { get; set; }

    public ulong? GetEarliestHeight()
    {
        return _earliestHeight;
    }

    public void SetEarliestHeight(ulong? earliestHeight)
    {
        _earliestHeight = earliestHeight;
    }

    public bool? IsEnabled()
    {
        return _isEnabled;
    }

    public void SetIsEnabled(bool? isEnabled)
    {
        _isEnabled = isEnabled;
    }

    public uint? GetState()
    {
        return _state;
    }

    public void SetState(uint? state)
    {
        _state = state;
    }

    public uint? GetThreshold()
    {
        return _threshold;
    }

    public void SetThreshold(uint? threshold)
    {
        _threshold = threshold;
    }

    public uint? GetVersion()
    {
        return _version;
    }

    public void SetVersion(uint? version)
    {
        _version = version;
    }

    public uint? GetNumVotes()
    {
        return _numVotes;
    }

    public void SetNumVotes(uint? numVotes)
    {
        _numVotes = numVotes;
    }

    public uint? GetWindow()
    {
        return _window;
    }

    public void SetWindow(uint? window)
    {
        _window = window;
    }

    public uint? GetVoting()
    {
        return _voting;
    }

    public void SetVoting(uint? voting)
    {
        _voting = voting;
    }
}