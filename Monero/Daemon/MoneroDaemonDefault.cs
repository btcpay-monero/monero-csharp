using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon;

public abstract class MoneroDaemonDefault : MoneroDaemon
{
    protected Dictionary<ulong, MoneroBlockHeader> _cachedHeaders = [];
    protected List<MoneroDaemonListener> _listeners = [];

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
        List<string> emptyList = [];
        FlushTxPool(emptyList);
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

    public virtual List<MoneroBlock> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight)
    {
        return GetBlocksByRangeChunked(startHeight, endHeight, null);
    }

    public abstract List<MoneroBlock> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight,
        ulong? maxChunkSize);

    public MoneroBlockTemplate GetBlockTemplate(string walletAddress)
    {
        return GetBlockTemplate(walletAddress, null);
    }

    public abstract MoneroBlockTemplate GetBlockTemplate(string walletAddress, int? reserveSize);

    public abstract int GetDownloadLimit();

    public virtual MoneroFeeEstimate GetFeeEstimate()
    {
        return GetFeeEstimate(null);
    }

    public abstract MoneroFeeEstimate GetFeeEstimate(int? graceBlocks);

    public virtual MoneroMinerTxSum GetMinerTxSum(ulong height)
    {
        return GetMinerTxSum(height, null);
    }

    public abstract MoneroMinerTxSum GetMinerTxSum(ulong height, ulong? numBlocks);

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

    public virtual List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts)
    {
        return GetOutputDistribution(amounts, null, null, null);
    }

    public virtual List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts, bool? isCumulative)
    {
        return GetOutputDistribution(amounts, isCumulative, null, null);
    }

    public virtual List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts, bool? isCumulative,
        ulong? startHeight)
    {
        return GetOutputDistribution(amounts, isCumulative, startHeight, null);
    }

    public abstract List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts, bool? isCumulative,
        ulong? startHeight, ulong? endHeight);

    public virtual List<MoneroOutputHistogramEntry> GetOutputHistogram()
    {
        return GetOutputHistogram(null, null, null, null, null);
    }

    public virtual List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts)
    {
        return GetOutputHistogram(amounts, null, null, null, null);
    }

    public virtual List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts, int? minCount)
    {
        return GetOutputHistogram(amounts, minCount, null, null, null);
    }

    public virtual List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts, int? minCount,
        int? maxCount)
    {
        return GetOutputHistogram(amounts, minCount, maxCount, null, null);
    }

    public virtual List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts, int? minCount,
        int? maxCount, bool? isUnlocked)
    {
        return GetOutputHistogram(amounts, minCount, maxCount, isUnlocked, null);
    }

    public abstract List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts, int? minCount,
        int? maxCount, bool? isUnlocked, int? recentCutoff);

    public abstract List<MoneroOutput> GetOutputs(List<MoneroOutput> outputs);

    public abstract List<MoneroBan> GetPeerBans();

    public abstract List<MoneroPeer> GetPeers();

    public abstract MoneroDaemonSyncInfo GetSyncInfo();

    public virtual MoneroTx? GetTx(string txHash)
    {
        return GetTx(txHash, false);
    }

    public virtual MoneroTx? GetTx(string txHash, bool prune)
    {
        List<MoneroTx> txs = GetTxs([txHash], prune);

        return txs.FirstOrDefault();
    }

    public virtual string? GetTxHex(string txHash)
    {
        return GetTxHex(txHash, false);
    }

    public virtual string? GetTxHex(string txHash, bool prune)
    {
        List<string> hexes = GetTxHexes([txHash], prune);

        return hexes.FirstOrDefault();
    }

    public virtual List<string> GetTxHexes(List<string> txHashes)
    {
        return GetTxHexes(txHashes, false);
    }

    public abstract List<string> GetTxHexes(List<string> txHashes, bool prune);

    public abstract List<MoneroTx> GetTxPool();

    public abstract List<string> GetTxPoolHashes();

    public abstract MoneroTxPoolStats GetTxPoolStats();

    public virtual List<MoneroTx> GetTxs(List<string> txHashes)
    {
        return GetTxs(txHashes, false);
    }

    public abstract List<MoneroTx> GetTxs(List<string> txHashes, bool prune);

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

    public MoneroSubmitTxResult SubmitTxHex(string txHex)
    {
        return SubmitTxHex(txHex, false);
    }

    public abstract MoneroSubmitTxResult SubmitTxHex(string txHex, bool doNotRelay);

    public abstract MoneroBlockHeader WaitForNextBlockHeader();
}