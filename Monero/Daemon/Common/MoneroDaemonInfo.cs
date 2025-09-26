using System.Text.Json.Serialization;

using Monero.Common;

namespace Monero.Daemon.Common;

public class MoneroDaemonInfo : MoneroRpcResponse
{
    [JsonPropertyName("adjusted_time")]
    [JsonInclude]
    private ulong? _adjustedTimestamp { get; set; }
    [JsonPropertyName("block_size_limit")]
    [JsonInclude]
    private ulong? _blockSizeLimit { get; set; }
    [JsonPropertyName("block_size_median")]
    [JsonInclude]
    private ulong? _blockSizeMedian { get; set; }
    [JsonPropertyName("block_weight_limit")]
    [JsonInclude]
    private ulong? _blockWeightLimit { get; set; }
    [JsonPropertyName("block_weight_median")]
    [JsonInclude]
    private ulong? _blockWeightMedian { get; set; }
    [JsonPropertyName("bootstrap_daemon_address")]
    [JsonInclude]
    private string? _bootstrapDaemonAddress { get; set; }
    [JsonPropertyName("credits")]
    [JsonInclude]
    private ulong? _credits { get; set; }
    [JsonPropertyName("cumulative_difficulty")]
    [JsonInclude]
    private ulong? _cumulativeDifficulty { get; set; }
    [JsonPropertyName("database_size")]
    [JsonInclude]
    private ulong? _databaseSize { get; set; }
    [JsonPropertyName("difficulty")]
    [JsonInclude]
    private ulong? _difficulty { get; set; }
    [JsonPropertyName("free_space")]
    [JsonInclude]
    private ulong? _freeSpace { get; set; }
    [JsonPropertyName("height")]
    [JsonInclude]
    private ulong? _height { get; set; }
    [JsonPropertyName("height_without_bootstrap")]
    [JsonInclude]
    private ulong? _heightWithoutBootstrap { get; set; }
    [JsonPropertyName("busy_syncing")]
    [JsonInclude]
    private bool? _isBusySyncing { get; set; }
    [JsonPropertyName("offline")]
    [JsonInclude]
    private bool? _isOffline { get; set; }
    [JsonPropertyName("restricted")]
    [JsonInclude]
    private bool? _isRestricted { get; set; }
    [JsonPropertyName("synchronized")]
    [JsonInclude]
    private bool? _isSynchronized { get; set; }
    private MoneroNetworkType? _networkType;
    [JsonPropertyName("alt_blocks_count")]
    [JsonInclude]
    private ulong? _numAltBlocks { get; set; }
    [JsonPropertyName("incoming_connections_count")]
    [JsonInclude]
    private uint? _numIncomingConnections { get; set; }
    [JsonPropertyName("grey_peerlist_size")]
    [JsonInclude]
    private uint? _numOfflinePeers { get; set; }
    [JsonPropertyName("white_peerlist_size")]
    [JsonInclude]
    private uint? _numOnlinePeers { get; set; }
    [JsonPropertyName("outgoing_connections_count")]
    [JsonInclude]
    private uint? _numOutgoingConnections { get; set; }
    [JsonPropertyName("rpc_connections_count")]
    [JsonInclude]
    private uint? _numRpcConnections { get; set; }
    [JsonPropertyName("tx_count")]
    [JsonInclude]
    private uint? _numTxs { get; set; }
    [JsonPropertyName("tx_pool_size")]
    [JsonInclude]
    private uint? _numTxsPool { get; set; }
    [JsonPropertyName("start_time")]
    [JsonInclude]
    private ulong? _startTimestamp { get; set; }
    [JsonPropertyName("target")]
    [JsonInclude]
    private ulong? _target { get; set; }
    [JsonPropertyName("target_height")]
    [JsonInclude]
    private ulong? _targetHeight { get; set; }
    [JsonPropertyName("top_block_hash")]
    [JsonInclude]
    private string? _topBlockHash { get; set; }
    [JsonPropertyName("update_available")]
    [JsonInclude]
    private bool? _updateAvailable { get; set; }
    [JsonPropertyName("version")]
    [JsonInclude]
    private string? _version { get; set; }
    [JsonPropertyName("was_bootstrap_ever_used")]
    [JsonInclude]
    private bool? _wasBootstrapEverUsed { get; set; }

    public string? GetVersion()
    {
        return _version;
    }

    public void SetVersion(string? version)
    {
        _version = version;
    }

    public ulong? GetNumAltBlocks()
    {
        return _numAltBlocks;
    }

    public void SetNumAltBlocks(ulong? numAltBlocks)
    {
        _numAltBlocks = numAltBlocks;
    }

    public ulong? GetBlockSizeLimit()
    {
        return _blockSizeLimit;
    }

    public void SetBlockSizeLimit(ulong? blockSizeLimit)
    {
        _blockSizeLimit = blockSizeLimit;
    }

    public ulong? GetBlockSizeMedian()
    {
        return _blockSizeMedian;
    }

    public void SetBlockSizeMedian(ulong? blockSizeMedian)
    {
        _blockSizeMedian = blockSizeMedian;
    }

    public ulong? GetBlockWeightLimit()
    {
        return _blockWeightLimit;
    }

    public MoneroDaemonInfo SetBlockWeightLimit(ulong? blockWeightLimit)
    {
        _blockWeightLimit = blockWeightLimit;
        return this;
    }

    public ulong? GetBlockWeightMedian()
    {
        return _blockWeightMedian;
    }

    public void SetBlockWeightMedian(ulong? blockWeightMedian)
    {
        _blockWeightMedian = blockWeightMedian;
    }

    public string? GetBootstrapDaemonAddress()
    {
        return _bootstrapDaemonAddress;
    }

    public void SetBootstrapDaemonAddress(string? bootstrapDaemonAddress)
    {
        _bootstrapDaemonAddress = bootstrapDaemonAddress;
    }

    public ulong? GetDifficulty()
    {
        return _difficulty;
    }

    public void SetDifficulty(ulong? difficulty)
    {
        _difficulty = difficulty;
    }

    public ulong? GetCumulativeDifficulty()
    {
        return _cumulativeDifficulty;
    }

    public void SetCumulativeDifficulty(ulong? cumulativeDifficulty)
    {
        _cumulativeDifficulty = cumulativeDifficulty;
    }

    public ulong? GetFreeSpace()
    {
        return _freeSpace;
    }

    public void SetFreeSpace(ulong? freeSpace)
    {
        _freeSpace = freeSpace;
    }

    public uint? GetNumOfflinePeers()
    {
        return _numOfflinePeers;
    }

    public void SetNumOfflinePeers(uint? numOfflinePeers)
    {
        _numOfflinePeers = numOfflinePeers;
    }

    public uint? GetNumOnlinePeers()
    {
        return _numOnlinePeers;
    }

    public void SetNumOnlinePeers(uint? numOnlinePeers)
    {
        _numOnlinePeers = numOnlinePeers;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public void SetHeight(ulong? height)
    {
        _height = height;
    }

    public ulong? GetHeightWithoutBootstrap()
    {
        return _heightWithoutBootstrap;
    }

    public void SetHeightWithoutBootstrap(ulong? heightWithoutBootstrap)
    {
        _heightWithoutBootstrap = heightWithoutBootstrap;
    }

    public MoneroNetworkType? GetNetworkType()
    {
        return _networkType;
    }

    public void SetNetworkType(MoneroNetworkType? networkType)
    {
        _networkType = networkType;
    }

    public bool? IsOffline()
    {
        return _isOffline;
    }

    public void SetIsOffline(bool? isOffline)
    {
        _isOffline = isOffline;
    }

    public uint? GetNumIncomingConnections()
    {
        return _numIncomingConnections;
    }

    public void SetNumIncomingConnections(uint? numIncomingConnections)
    {
        _numIncomingConnections = numIncomingConnections;
    }

    public uint? GetNumOutgoingConnections()
    {
        return _numOutgoingConnections;
    }

    public void SetNumOutgoingConnections(uint? numOutgoingConnections)
    {
        _numOutgoingConnections = numOutgoingConnections;
    }

    public uint? GetNumRpcConnections()
    {
        return _numRpcConnections;
    }

    public void SetNumRpcConnections(uint? numRpcConnections)
    {
        _numRpcConnections = numRpcConnections;
    }

    public ulong? GetStartTimestamp()
    {
        return _startTimestamp;
    }

    public void SetStartTimestamp(ulong? startTimestamp)
    {
        _startTimestamp = startTimestamp;
    }

    public ulong? GetAdjustedTimestamp()
    {
        return _adjustedTimestamp;
    }

    public void SetAdjustedTimestamp(ulong? adjustedTimestamp)
    {
        _adjustedTimestamp = adjustedTimestamp;
    }

    public ulong? GetTarget()
    {
        return _target;
    }

    public void SetTarget(ulong? target)
    {
        _target = target;
    }

    public ulong? GetTargetHeight()
    {
        return _targetHeight;
    }

    public void SetTargetHeight(ulong? targetHeight)
    {
        _targetHeight = targetHeight;
    }

    public string? GetTopBlockHash()
    {
        return _topBlockHash;
    }

    public void SetTopBlockHash(string? topBlockHash)
    {
        _topBlockHash = topBlockHash;
    }

    public uint? GetNumTxs()
    {
        return _numTxs;
    }

    public void SetNumTxs(uint? numTxs)
    {
        _numTxs = numTxs;
    }

    public uint? GetNumTxsPool()
    {
        return _numTxsPool;
    }

    public void SetNumTxsPool(uint? numTxsPool)
    {
        _numTxsPool = numTxsPool;
    }

    public bool? GetWasBootstrapEverUsed()
    {
        return _wasBootstrapEverUsed;
    }

    public void SetWasBootstrapEverUsed(bool? wasBootstrapEverUsed)
    {
        _wasBootstrapEverUsed = wasBootstrapEverUsed;
    }

    public ulong? GetDatabaseSize()
    {
        return _databaseSize;
    }

    public void SetDatabaseSize(ulong? databaseSize)
    {
        _databaseSize = databaseSize;
    }

    public bool? GetUpdateAvailable()
    {
        return _updateAvailable;
    }

    public void SetUpdateAvailable(bool? updateAvailable)
    {
        _updateAvailable = updateAvailable;
    }

    public ulong? GetCredits()
    {
        return _credits;
    }

    public void SetCredits(ulong? credits)
    {
        _credits = credits;
    }

    public bool? IsBusySyncing()
    {
        return _isBusySyncing;
    }

    public void SetIsBusySyncing(bool? isBusySyncing)
    {
        _isBusySyncing = isBusySyncing;
    }

    public bool? IsSynchronized()
    {
        return _isSynchronized;
    }

    public void SetIsSynchronized(bool? isSynchronized)
    {
        _isSynchronized = isSynchronized;
    }

    public bool? IsRestricted()
    {
        return _isRestricted;
    }

    public void SetIsRestricted(bool? isRestricted)
    {
        _isRestricted = isRestricted;
    }
}