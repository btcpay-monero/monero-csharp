namespace Monero.Daemon.Common;

public class MoneroBlockTemplate
{
    private string? _blockHashingBlob;
    private string? _blockTemplateBlob;
    private ulong? _difficulty;
    private ulong? _expectedReward;
    private ulong? _height;
    private string? _nextSeedHash;
    private string? _prevHash;
    private ulong? _reservedOffset;
    private string? _seedHash;
    private ulong? _seedHeight;

    public string? GetBlockTemplateBlob()
    {
        return _blockTemplateBlob;
    }

    public void SetBlockTemplateBlob(string? blockTemplateBlob)
    {
        this._blockTemplateBlob = blockTemplateBlob;
    }

    public string? GetBlockHashingBlob()
    {
        return _blockHashingBlob;
    }

    public void SetBlockHashingBlob(string? blockHashingBlob)
    {
        this._blockHashingBlob = blockHashingBlob;
    }

    public ulong? GetDifficulty()
    {
        return _difficulty;
    }

    public void SetDifficulty(ulong? difficulty)
    {
        this._difficulty = difficulty;
    }

    public ulong? GetExpectedReward()
    {
        return _expectedReward;
    }

    public void SetExpectedReward(ulong? expectedReward)
    {
        this._expectedReward = expectedReward;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public void SetHeight(ulong? height)
    {
        this._height = height;
    }

    public string? GetPrevHash()
    {
        return _prevHash;
    }

    public void SetPrevHash(string? prevHash)
    {
        this._prevHash = prevHash;
    }

    public ulong? GetReservedOffset()
    {
        return _reservedOffset;
    }

    public void SetReservedOffset(ulong? reservedOffset)
    {
        this._reservedOffset = reservedOffset;
    }

    public ulong? GetSeedHeight()
    {
        return _seedHeight;
    }

    public void SetSeedHeight(ulong? seedHeight)
    {
        this._seedHeight = seedHeight;
    }

    public string? GetSeedHash()
    {
        return _seedHash;
    }

    public void SetSeedHash(string? seedHash)
    {
        this._seedHash = seedHash;
    }

    public string? GetNextSeedHash()
    {
        return _nextSeedHash;
    }

    public void SetNextSeedHash(string? nextSeedHash)
    {
        this._nextSeedHash = nextSeedHash;
    }
}