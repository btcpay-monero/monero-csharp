using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon;

public abstract class MoneroDaemonDefault : MoneroDaemon
{
    protected List<MoneroDaemonListener> _listeners = [];
    protected Dictionary<ulong, MoneroBlockHeader> _cachedHeaders = [];

    public virtual void AddListener(MoneroDaemonListener listener)
    {
        lock (_listeners)
        {
            _listeners.Add(listener);
        }
    }

    public abstract MoneroDaemonUpdateCheckResult CheckForUpdate();

    public virtual MoneroDaemonUpdateDownloadResult DownloadUpdate()
    {
        return DownloadUpdate(null);
    }

    public abstract MoneroDaemonUpdateDownloadResult DownloadUpdate(string? path);

    public virtual void FlushTxPool()
    {
        FlushTxPool(new List<string>());
    }

    public virtual void FlushTxPool(string txHash)
    {
        FlushTxPool([txHash]);
    }

    public abstract void FlushTxPool(List<string> txHashes);

    public abstract List<string> GetAltBlockHashes();

    public abstract List<MoneroAltChain> GetAltChains();

    public abstract MoneroBlock GetBlockByHash(string blockHash);

    public abstract MoneroBlock GetBlockByHeight(ulong blockHeight);

    public abstract string GetBlockHash(ulong height);

    public abstract List<string> GetBlockHashes(List<string> blockHashes, ulong startHeight);

    public abstract MoneroBlockHeader GetBlockHeaderByHash(string blockHash);

    public abstract MoneroBlockHeader GetBlockHeaderByHeight(ulong blockHeight);

    public abstract List<MoneroBlockHeader> GetBlockHeadersByRange(ulong startHeight, ulong endHeight);

    public abstract List<MoneroBlock> GetBlocksByHash(List<string> blockHashes, ulong startHeight, bool prune);

    public abstract List<MoneroBlock> GetBlocksByHeight(List<ulong> blockHeights);

    public abstract List<MoneroBlock> GetBlocksByRange(ulong? startHeight, ulong? endHeight);

    public abstract List<MoneroBlock> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight, ulong? maxChunkSize = null);

    public abstract MoneroBlockTemplate GetBlockTemplate(string walletAddress, int? reserveSize = null);

    public abstract int GetDownloadLimit();

    public abstract MoneroFeeEstimate GetFeeEstimate(int? graceBlocks = null);

    public abstract MoneroMinerTxSum GetMinerTxSum(ulong height, ulong? numBlocks = null);

    public abstract MoneroHardForkInfo GetHardForkInfo();

    public abstract ulong GetHeight();

    public abstract MoneroDaemonInfo GetInfo();

    public virtual MoneroKeyImage.SpentStatus GetKeyImageSpentStatus(string keyImage)
    {
        return GetKeyImageSpentStatuses([keyImage])[0];
    }

    public abstract List<MoneroKeyImage.SpentStatus> GetKeyImageSpentStatuses(List<string> keyImage);

    public abstract List<MoneroPeer> GetKnownPeers();

    public abstract MoneroBlockHeader GetLastBlockHeader();

    public virtual List<MoneroDaemonListener> GetListeners()
    {
        return [.. _listeners];
    }

    public abstract MoneroMiningStatus GetMiningStatus();

    public abstract List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts, bool? isCumulative = null, ulong? startHeight = null, ulong? endHeight = null);

    public abstract List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts = null, int? minCount = null, int? maxCount = null, bool? isUnlocked = null, int? recentCutoff = null);

    public abstract List<MoneroOutput> GetOutputs(List<MoneroOutput> outputs);

    public abstract List<MoneroBan> GetPeerBans();

    public abstract List<MoneroPeer> GetPeers();

    public abstract MoneroDaemonSyncInfo GetSyncInfo();

    public virtual MoneroTx? GetTx(string txHash, bool prune = false)
    {
        var txs = GetTxs([txHash], prune);

        return txs.FirstOrDefault();
    }

    public virtual string? GetTxHex(string txHash, bool prune = false)
    {
        var hexes = GetTxHexes([txHash], prune);

        return hexes.FirstOrDefault();
    }

    public abstract List<string> GetTxHexes(List<string> txHashes, bool prune = false);

    public abstract List<MoneroTx> GetTxPool();

    public abstract List<string> GetTxPoolHashes();

    public abstract MoneroTxPoolStats GetTxPoolStats();

    public abstract List<MoneroTx> GetTxs(List<string> txHashes, bool prune = false);

    public abstract int GetUploadLimit();

    public abstract MoneroVersion GetVersion();

    public abstract bool IsTrusted();

    public abstract MoneroPruneResult PruneBlockchain(bool check);

    public virtual void RelayTxByHash(string txHash)
    {
        RelayTxsByHash([txHash]);
    }

    public abstract void RelayTxsByHash(List<string> txHashes);

    public virtual void RemoveListener(MoneroDaemonListener listener)
    {
        lock (_listeners)
        {
            _listeners.Remove(listener);
        }
    }

    public abstract int ResetDownloadLimit();

    public abstract int ResetUploadLimit();

    public abstract int SetDownloadLimit(int limit);

    public abstract void SetIncomingPeerLimit(int limit);

    public abstract void SetOutgoingPeerLimit(int limit);

    public virtual void SetPeerBan(MoneroBan ban)
    {
        SetPeerBans([ban]);
    }

    public abstract void SetPeerBans(List<MoneroBan> bans);

    public abstract int SetUploadLimit(int limit);

    public abstract void StartMining(string? address, ulong? numThreads, bool? isBackground, bool? ignoreBattery);

    public abstract void Stop();

    public abstract void StopMining();

    public virtual void SubmitBlock(string blockBlob)
    {
        SubmitBlocks([blockBlob]);
    }

    public abstract void SubmitBlocks(List<string> blockBlobs);

    public abstract MoneroSubmitTxResult SubmitTxHex(string txHex, bool doNotRelay = false);

    public abstract MoneroBlockHeader WaitForNextBlockHeader();
}
