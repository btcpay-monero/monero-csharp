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
        this._earliestHeight = earliestHeight;
    }

    public bool? IsEnabled()
    {
        return _isEnabled;
    }

    public void SetIsEnabled(bool? isEnabled)
    {
        this._isEnabled = isEnabled;
    }

    public uint? GetState()
    {
        return _state;
    }

    public void SetState(uint? state)
    {
        this._state = state;
    }

    public uint? GetThreshold()
    {
        return _threshold;
    }

    public void SetThreshold(uint? threshold)
    {
        this._threshold = threshold;
    }

    public uint? GetVersion()
    {
        return _version;
    }

    public void SetVersion(uint? version)
    {
        this._version = version;
    }

    public uint? GetNumVotes()
    {
        return _numVotes;
    }

    public void SetNumVotes(uint? numVotes)
    {
        this._numVotes = numVotes;
    }

    public uint? GetWindow()
    {
        return _window;
    }

    public void SetWindow(uint? window)
    {
        this._window = window;
    }

    public uint? GetVoting()
    {
        return _voting;
    }

    public void SetVoting(uint? voting)
    {
        this._voting = voting;
    }
}