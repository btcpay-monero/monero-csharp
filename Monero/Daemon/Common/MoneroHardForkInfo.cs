namespace Monero.Daemon.Common;

public class MoneroHardForkInfo : MoneroRpcPaymentInfo
{
    private ulong? _earliestHeight;
    private bool? _isEnabled;
    private uint? _numVotes;
    private uint? _state;
    private uint? _threshold;
    private uint? _version;
    private uint? _voting;
    private uint? _window;

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