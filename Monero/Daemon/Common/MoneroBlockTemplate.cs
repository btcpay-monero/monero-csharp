namespace Monero.Daemon.Common;

public class MoneroBlockTemplate
{
    private string? blockHashingBlob;
    private string? blockTemplateBlob;
    private ulong? difficulty;
    private ulong? expectedReward;
    private ulong? height;
    private string? nextSeedHash;
    private string? prevHash;
    private ulong? reservedOffset;
    private string? seedHash;
    private ulong? seedHeight;

    public string? GetBlockTemplateBlob()
    {
        return blockTemplateBlob;
    }

    public void SetBlockTemplateBlob(string? blockTemplateBlob)
    {
        this.blockTemplateBlob = blockTemplateBlob;
    }

    public string? GetBlockHashingBlob()
    {
        return blockHashingBlob;
    }

    public void SetBlockHashingBlob(string? blockHashingBlob)
    {
        this.blockHashingBlob = blockHashingBlob;
    }

    public ulong? GetDifficulty()
    {
        return difficulty;
    }

    public void SetDifficulty(ulong? difficulty)
    {
        this.difficulty = difficulty;
    }

    public ulong? GetExpectedReward()
    {
        return expectedReward;
    }

    public void SetExpectedReward(ulong? expectedReward)
    {
        this.expectedReward = expectedReward;
    }

    public ulong? GetHeight()
    {
        return height;
    }

    public void SetHeight(ulong? height)
    {
        this.height = height;
    }

    public string? GetPrevHash()
    {
        return prevHash;
    }

    public void SetPrevHash(string? prevHash)
    {
        this.prevHash = prevHash;
    }

    public ulong? GetReservedOffset()
    {
        return reservedOffset;
    }

    public void SetReservedOffset(ulong? reservedOffset)
    {
        this.reservedOffset = reservedOffset;
    }

    public ulong? GetSeedHeight()
    {
        return seedHeight;
    }

    public void SetSeedHeight(ulong? seedHeight)
    {
        this.seedHeight = seedHeight;
    }

    public string? GetSeedHash()
    {
        return seedHash;
    }

    public void SetSeedHash(string? seedHash)
    {
        this.seedHash = seedHash;
    }

    public string? GetNextSeedHash()
    {
        return nextSeedHash;
    }

    public void SetNextSeedHash(string? nextSeedHash)
    {
        this.nextSeedHash = nextSeedHash;
    }
}