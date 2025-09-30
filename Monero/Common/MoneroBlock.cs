using System.Text.Json.Serialization;

namespace Monero.Common;

public class MoneroBlock : MoneroBlockHeader
{
    [JsonPropertyName("block_header")]
    public MoneroBlockHeader? BlockHeader { get; set; }

    [JsonPropertyName("blob")]
    public string? Hex { get; set; }

    [JsonPropertyName("miner_tx")]
    public MoneroTx? MinerTx { get; set; }

    [JsonPropertyName("tx_hashes")]
    public List<string> TxHashes { get; set; }

    public List<MoneroTx>? Txs;

    public MoneroBlock()
    {
        TxHashes = [];
    }

    [JsonPropertyName("json")]
    public string? Json { get; set; }

    public void Init()
    {
        Merge(BlockHeader);
    }
}