using Monero.Common;

namespace Monero.Daemon.Common;

public class MoneroDaemonInfo
{
    private ulong? adjustedTimestamp;
    private ulong? blockSizeLimit;
    private ulong? blockSizeMedian;
    private ulong? blockWeightLimit;
    private ulong? blockWeightMedian;
    private string? bootstrapDaemonAddress;
    private ulong? credits;
    private ulong? cumulativeDifficulty;
    private ulong? databaseSize;
    private ulong? difficulty;
    private ulong? freeSpace;
    private ulong? height;
    private ulong? heightWithoutBootstrap;
    private bool? isBusySyncing;
    private bool? isOffline;
    private bool? isRestricted;
    private bool? isSynchronized;
    private MoneroNetworkType? networkType;
    private ulong? numAltBlocks;
    private uint? numIncomingConnections;
    private uint? numOfflinePeers;
    private uint? numOnlinePeers;
    private uint? numOutgoingConnections;
    private uint? numRpcConnections;
    private uint? numTxs;
    private uint? numTxsPool;
    private ulong? startTimestamp;
    private ulong? target;
    private ulong? targetHeight;
    private string? topBlockHash;
    private bool? updateAvailable;
    private string? version;
    private bool? wasBootstrapEverUsed;

    public string? GetVersion()
    {
        return version;
    }

    public void SetVersion(string? version)
    {
        this.version = version;
    }

    public ulong? GetNumAltBlocks()
    {
        return numAltBlocks;
    }

    public void SetNumAltBlocks(ulong? numAltBlocks)
    {
        this.numAltBlocks = numAltBlocks;
    }

    public ulong? GetBlockSizeLimit()
    {
        return blockSizeLimit;
    }

    public void SetBlockSizeLimit(ulong? blockSizeLimit)
    {
        this.blockSizeLimit = blockSizeLimit;
    }

    public ulong? GetBlockSizeMedian()
    {
        return blockSizeMedian;
    }

    public void SetBlockSizeMedian(ulong? blockSizeMedian)
    {
        this.blockSizeMedian = blockSizeMedian;
    }

    public ulong? GetBlockWeightLimit()
    {
        return blockWeightLimit;
    }

    public MoneroDaemonInfo SetBlockWeightLimit(ulong? blockWeightLimit)
    {
        this.blockWeightLimit = blockWeightLimit;
        return this;
    }

    public ulong? GetBlockWeightMedian()
    {
        return blockWeightMedian;
    }

    public void SetBlockWeightMedian(ulong? blockWeightMedian)
    {
        this.blockWeightMedian = blockWeightMedian;
    }

    public string? GetBootstrapDaemonAddress()
    {
        return bootstrapDaemonAddress;
    }

    public void SetBootstrapDaemonAddress(string? bootstrapDaemonAddress)
    {
        this.bootstrapDaemonAddress = bootstrapDaemonAddress;
    }

    public ulong? GetDifficulty()
    {
        return difficulty;
    }

    public void SetDifficulty(ulong? difficulty)
    {
        this.difficulty = difficulty;
    }

    public ulong? GetCumulativeDifficulty()
    {
        return cumulativeDifficulty;
    }

    public void SetCumulativeDifficulty(ulong? cumulativeDifficulty)
    {
        this.cumulativeDifficulty = cumulativeDifficulty;
    }

    public ulong? GetFreeSpace()
    {
        return freeSpace;
    }

    public void SetFreeSpace(ulong? freeSpace)
    {
        this.freeSpace = freeSpace;
    }

    public uint? GetNumOfflinePeers()
    {
        return numOfflinePeers;
    }

    public void SetNumOfflinePeers(uint? numOfflinePeers)
    {
        this.numOfflinePeers = numOfflinePeers;
    }

    public uint? GetNumOnlinePeers()
    {
        return numOnlinePeers;
    }

    public void SetNumOnlinePeers(uint? numOnlinePeers)
    {
        this.numOnlinePeers = numOnlinePeers;
    }

    public ulong? GetHeight()
    {
        return height;
    }

    public void SetHeight(ulong? height)
    {
        this.height = height;
    }

    public ulong? GetHeightWithoutBootstrap()
    {
        return heightWithoutBootstrap;
    }

    public void SetHeightWithoutBootstrap(ulong? heightWithoutBootstrap)
    {
        this.heightWithoutBootstrap = heightWithoutBootstrap;
    }

    public MoneroNetworkType? GetNetworkType()
    {
        return networkType;
    }

    public void SetNetworkType(MoneroNetworkType? networkType)
    {
        this.networkType = networkType;
    }

    public bool? IsOffline()
    {
        return isOffline;
    }

    public void SetIsOffline(bool? isOffline)
    {
        this.isOffline = isOffline;
    }

    public uint? GetNumIncomingConnections()
    {
        return numIncomingConnections;
    }

    public void SetNumIncomingConnections(uint? numIncomingConnections)
    {
        this.numIncomingConnections = numIncomingConnections;
    }

    public uint? GetNumOutgoingConnections()
    {
        return numOutgoingConnections;
    }

    public void SetNumOutgoingConnections(uint? numOutgoingConnections)
    {
        this.numOutgoingConnections = numOutgoingConnections;
    }

    public uint? GetNumRpcConnections()
    {
        return numRpcConnections;
    }

    public void SetNumRpcConnections(uint? numRpcConnections)
    {
        this.numRpcConnections = numRpcConnections;
    }

    public ulong? GetStartTimestamp()
    {
        return startTimestamp;
    }

    public void SetStartTimestamp(ulong? startTimestamp)
    {
        this.startTimestamp = startTimestamp;
    }

    public ulong? GetAdjustedTimestamp()
    {
        return adjustedTimestamp;
    }

    public void SetAdjustedTimestamp(ulong? adjustedTimestamp)
    {
        this.adjustedTimestamp = adjustedTimestamp;
    }

    public ulong? GetTarget()
    {
        return target;
    }

    public void SetTarget(ulong? target)
    {
        this.target = target;
    }

    public ulong? GetTargetHeight()
    {
        return targetHeight;
    }

    public void SetTargetHeight(ulong? targetHeight)
    {
        this.targetHeight = targetHeight;
    }

    public string? GetTopBlockHash()
    {
        return topBlockHash;
    }

    public void SetTopBlockHash(string? topBlockHash)
    {
        this.topBlockHash = topBlockHash;
    }

    public uint? GetNumTxs()
    {
        return numTxs;
    }

    public void SetNumTxs(uint? numTxs)
    {
        this.numTxs = numTxs;
    }

    public uint? GetNumTxsPool()
    {
        return numTxsPool;
    }

    public void SetNumTxsPool(uint? numTxsPool)
    {
        this.numTxsPool = numTxsPool;
    }

    public bool? GetWasBootstrapEverUsed()
    {
        return wasBootstrapEverUsed;
    }

    public void SetWasBootstrapEverUsed(bool? wasBootstrapEverUsed)
    {
        this.wasBootstrapEverUsed = wasBootstrapEverUsed;
    }

    public ulong? GetDatabaseSize()
    {
        return databaseSize;
    }

    public void SetDatabaseSize(ulong? databaseSize)
    {
        this.databaseSize = databaseSize;
    }

    public bool? GetUpdateAvailable()
    {
        return updateAvailable;
    }

    public void SetUpdateAvailable(bool? updateAvailable)
    {
        this.updateAvailable = updateAvailable;
    }

    public ulong? GetCredits()
    {
        return credits;
    }

    public void SetCredits(ulong? credits)
    {
        this.credits = credits;
    }

    public bool? IsBusySyncing()
    {
        return isBusySyncing;
    }

    public void SetIsBusySyncing(bool? isBusySyncing)
    {
        this.isBusySyncing = isBusySyncing;
    }

    public bool? IsSynchronized()
    {
        return isSynchronized;
    }

    public void SetIsSynchronized(bool? isSynchronized)
    {
        this.isSynchronized = isSynchronized;
    }

    public bool? IsRestricted()
    {
        return isRestricted;
    }

    public void SetIsRestricted(bool? isRestricted)
    {
        this.isRestricted = isRestricted;
    }
}