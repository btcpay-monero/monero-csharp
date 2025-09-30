using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroAltChain
{
    [JsonPropertyName("block_hashes")]
    public List<string>? BlockHashes { get; set; }

    [JsonPropertyName("difficulty")]
    public ulong? Difficulty { get; set; }

    [JsonPropertyName("height")]
    public ulong? Height { get; set; }

    [JsonPropertyName("length")]
    public ulong? Length { get; set; }

    [JsonPropertyName("main_chain_parent_block")]
    public string? MainChainParentBlockHash { get; set; }
}