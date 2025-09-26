using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroAltChain
{
    [JsonPropertyName("block_hashes")]
    [JsonInclude]
    private List<string>? _blockHashes { get; set; }
    [JsonPropertyName("difficulty")]
    [JsonInclude]
    private ulong? _difficulty { get; set; }
    [JsonPropertyName("height")]
    [JsonInclude]
    private ulong? _height { get; set; }
    [JsonPropertyName("length")]
    [JsonInclude]
    private ulong? _length { get; set; }
    [JsonPropertyName("main_chain_parent_block")]
    [JsonInclude]
    private string? _mainChainParentBlockHash { get; set; }

    public List<string>? GetBlockHashes()
    {
        return _blockHashes;
    }

    public MoneroAltChain SetBlockHashes(List<string>? blockHashes)
    {
        _blockHashes = blockHashes;
        return this;
    }

    public ulong? GetDifficulty()
    {
        return _difficulty;
    }

    public MoneroAltChain SetDifficulty(ulong? difficulty)
    {
        _difficulty = difficulty;
        return this;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public MoneroAltChain SetHeight(ulong? height)
    {
        _height = height;
        return this;
    }

    public ulong? GetLength()
    {
        return _length;
    }

    public MoneroAltChain SetLength(ulong? length)
    {
        _length = length;
        return this;
    }

    public string? GetMainChainParentBlockHash()
    {
        return _mainChainParentBlockHash;
    }

    public MoneroAltChain SetMainChainParentBlockHash(string? mainChainParentBlockHash)
    {
        _mainChainParentBlockHash = mainChainParentBlockHash;
        return this;
    }
}