using Monero.Common;

namespace Monero.Daemon.Common;

public class MoneroDaemonInfo
{
    private ulong? _adjustedTimestamp;
    private ulong? _blockSizeLimit;
    private ulong? _blockSizeMedian;
    private ulong? _blockWeightLimit;
    private ulong? _blockWeightMedian;
    private string? _bootstrapDaemonAddress;
    private ulong? _credits;
    private ulong? _cumulativeDifficulty;
    private ulong? _databaseSize;
    private ulong? _difficulty;
    private ulong? _freeSpace;
    private ulong? _height;
    private ulong? _heightWithoutBootstrap;
    private bool? _isBusySyncing;
    private bool? _isOffline;
    private bool? _isRestricted;
    private bool? _isSynchronized;
    private MoneroNetworkType? _networkType;
    private ulong? _numAltBlocks;
    private uint? _numIncomingConnections;
    private uint? _numOfflinePeers;
    private uint? _numOnlinePeers;
    private uint? _numOutgoingConnections;
    private uint? _numRpcConnections;
    private uint? _numTxs;
    private uint? _numTxsPool;
    private ulong? _startTimestamp;
    private ulong? _target;
    private ulong? _targetHeight;
    private string? _topBlockHash;
    private bool? _updateAvailable;
    private string? _version;
    private bool? _wasBootstrapEverUsed;

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