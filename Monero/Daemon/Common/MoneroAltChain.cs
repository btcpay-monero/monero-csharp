namespace Monero.Daemon.Common;

public class MoneroAltChain
{
    private List<string>? _blockHashes;
    private ulong? _difficulty;
    private ulong? _height;
    private ulong? _length;
    private string? _mainChainParentBlockHash;

    public List<string>? GetBlockHashes()
    {
        return _blockHashes;
    }

    public MoneroAltChain SetBlockHashes(List<string>? blockHashes)
    {
        this._blockHashes = blockHashes;
        return this;
    }

    public ulong? GetDifficulty()
    {
        return _difficulty;
    }

    public MoneroAltChain SetDifficulty(ulong? difficulty)
    {
        this._difficulty = difficulty;
        return this;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public MoneroAltChain SetHeight(ulong? height)
    {
        this._height = height;
        return this;
    }

    public ulong? GetLength()
    {
        return _length;
    }

    public MoneroAltChain SetLength(ulong? length)
    {
        this._length = length;
        return this;
    }

    public string? GetMainChainParentBlockHash()
    {
        return _mainChainParentBlockHash;
    }

    public MoneroAltChain SetMainChainParentBlockHash(string? mainChainParentBlockHash)
    {
        this._mainChainParentBlockHash = mainChainParentBlockHash;
        return this;
    }
}