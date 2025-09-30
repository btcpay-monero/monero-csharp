using System.Text.Json.Serialization;

using Monero.Common;

namespace Monero.Daemon.Common;

public class MoneroDaemonInfo : MoneroRpcResponse
{
    [JsonPropertyName("adjusted_time")]
    public ulong? AdjustedTimestamp { get; set; }

    [JsonPropertyName("block_size_limit")]
    public ulong? BlockSizeLimit { get; set; }

    [JsonPropertyName("block_size_median")]
    public ulong? BlockSizeMedian { get; set; }

    [JsonPropertyName("block_weight_limit")]
    public ulong? BlockWeightLimit { get; set; }

    [JsonPropertyName("block_weight_median")]
    public ulong? BlockWeightMedian { get; set; }

    [JsonPropertyName("bootstrap_daemon_address")]
    public string? BootstrapDaemonAddress { get; set; }

    [JsonPropertyName("credits")]
    public ulong? Credits { get; set; }

    [JsonPropertyName("cumulative_difficulty")]
    public ulong? CumulativeDifficulty { get; set; }

    [JsonPropertyName("database_size")]
    public ulong? DatabaseSize { get; set; }

    [JsonPropertyName("difficulty")]
    public ulong? Difficulty { get; set; }

    [JsonPropertyName("free_space")]
    public ulong? FreeSpace { get; set; }

    [JsonPropertyName("height")]
    public long Height { get; set; }

    [JsonPropertyName("height_without_bootstrap")]
    public ulong? HeightWithoutBootstrap { get; set; }

    [JsonPropertyName("busy_syncing")]
    public bool IsBusySyncing { get; set; }

    [JsonPropertyName("offline")]
    public bool IsOffline { get; set; }

    [JsonPropertyName("restricted")]
    public bool IsRestricted { get; set; }

    [JsonPropertyName("synchronized")]
    public bool IsSynchronized { get; set; }


    public MoneroNetworkType? NetworkType;

    [JsonPropertyName("alt_blocks_count")]
    public ulong? NumAltBlocks { get; set; }

    [JsonPropertyName("incoming_connections_count")]
    public uint? NumIncomingConnections { get; set; }

    [JsonPropertyName("grey_peerlist_size")]
    public uint? NumOfflinePeers { get; set; }

    [JsonPropertyName("white_peerlist_size")]
    public uint? NumOnlinePeers { get; set; }

    [JsonPropertyName("outgoing_connections_count")]
    public uint? NumOutgoingConnections { get; set; }

    [JsonPropertyName("rpc_connections_count")]
    public uint? NumRpcConnections { get; set; }

    [JsonPropertyName("tx_count")]
    public uint? NumTxs { get; set; }

    [JsonPropertyName("tx_pool_size")]
    public uint? NumTxsPool { get; set; }

    [JsonPropertyName("start_time")]
    public ulong? StartTimestamp { get; set; }

    [JsonPropertyName("target")]
    public ulong? Target { get; set; }

    [JsonPropertyName("target_height")]
    public long? TargetHeight { get; set; }

    [JsonPropertyName("top_block_hash")]
    public string? TopBlockHash { get; set; }

    [JsonPropertyName("update_available")]
    public bool? UpdateAvailable { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("was_bootstrap_ever_used")]
    public bool? WasBootstrapEverUsed { get; set; }
}