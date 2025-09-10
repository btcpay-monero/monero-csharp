namespace Monero.Daemon.Common;

public class MoneroAltChain
{
    private List<string>? blockHashes;
    private ulong? difficulty;
    private ulong? height;
    private ulong? length;
    private string? mainChainParentBlockHash;

    public List<string>? GetBlockHashes()
    {
        return blockHashes;
    }

    public MoneroAltChain SetBlockHashes(List<string>? blockHashes)
    {
        this.blockHashes = blockHashes;
        return this;
    }

    public ulong? GetDifficulty()
    {
        return difficulty;
    }

    public MoneroAltChain SetDifficulty(ulong? difficulty)
    {
        this.difficulty = difficulty;
        return this;
    }

    public ulong? GetHeight()
    {
        return height;
    }

    public MoneroAltChain SetHeight(ulong? height)
    {
        this.height = height;
        return this;
    }

    public ulong? GetLength()
    {
        return length;
    }

    public MoneroAltChain SetLength(ulong? length)
    {
        this.length = length;
        return this;
    }

    public string? GetMainChainParentBlockHash()
    {
        return mainChainParentBlockHash;
    }

    public MoneroAltChain SetMainChainParentBlockHash(string? mainChainParentBlockHash)
    {
        this.mainChainParentBlockHash = mainChainParentBlockHash;
        return this;
    }
}