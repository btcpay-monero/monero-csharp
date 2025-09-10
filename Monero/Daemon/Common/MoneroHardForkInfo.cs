namespace Monero.Daemon.Common;

public class MoneroHardForkInfo
{
    private ulong? credits;
    private ulong? earliestHeight;
    private bool? isEnabled;
    private uint? numVotes;
    private uint? state;
    private uint? threshold;
    private string? topBlockHash;
    private uint? version;
    private uint? voting;
    private uint? window;

    public ulong? GetEarliestHeight()
    {
        return earliestHeight;
    }

    public void SetEarliestHeight(ulong? earliestHeight)
    {
        this.earliestHeight = earliestHeight;
    }

    public bool? IsEnabled()
    {
        return isEnabled;
    }

    public void SetIsEnabled(bool? isEnabled)
    {
        this.isEnabled = isEnabled;
    }

    public uint? GetState()
    {
        return state;
    }

    public void SetState(uint? state)
    {
        this.state = state;
    }

    public uint? GetThreshold()
    {
        return threshold;
    }

    public void SetThreshold(uint? threshold)
    {
        this.threshold = threshold;
    }

    public uint? GetVersion()
    {
        return version;
    }

    public void SetVersion(uint? version)
    {
        this.version = version;
    }

    public uint? GetNumVotes()
    {
        return numVotes;
    }

    public void SetNumVotes(uint? numVotes)
    {
        this.numVotes = numVotes;
    }

    public uint? GetWindow()
    {
        return window;
    }

    public void SetWindow(uint? window)
    {
        this.window = window;
    }

    public uint? GetVoting()
    {
        return voting;
    }

    public void SetVoting(uint? voting)
    {
        this.voting = voting;
    }

    public ulong? GetCredits()
    {
        return credits;
    }

    public void SetCredits(ulong? credits)
    {
        this.credits = credits;
    }

    public string? GetTopBlockHash()
    {
        return topBlockHash;
    }

    public void SetTopBlockHash(string? topBlockHash)
    {
        this.topBlockHash = topBlockHash;
    }
}