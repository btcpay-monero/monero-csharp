using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon;

public abstract class MoneroDaemonDefault : IMoneroDaemon
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

    public abstract Task<MoneroDaemonUpdateCheckResult> CheckForUpdate();

    public virtual Task<MoneroDaemonUpdateDownloadResult> DownloadUpdate()
    {
        return DownloadUpdate(null);
    }

    public abstract Task<MoneroDaemonUpdateDownloadResult> DownloadUpdate(string? path);

    public virtual async Task FlushTxPool()
    {
        List<string> emptyList = [];
        await FlushTxPool(emptyList);
    }

    public virtual async Task FlushTxPool(string txHash)
    {
        await FlushTxPool([txHash]);
    }

    public abstract Task FlushTxPool(List<string> txHashes);

    public abstract Task<List<string>> GetAltBlockHashes();

    public abstract Task<List<MoneroAltChain>> GetAltChains();

    public abstract Task<MoneroBlock> GetBlockByHash(string blockHash);

    public abstract Task<MoneroBlock> GetBlockByHeight(ulong blockHeight);

    public abstract Task<string> GetBlockHash(ulong height);

    public abstract Task<List<string>> GetBlockHashes(List<string> blockHashes, ulong startHeight);

    public abstract Task<MoneroBlockHeader> GetBlockHeaderByHash(string blockHash);

    public abstract Task<MoneroBlockHeader> GetBlockHeaderByHeight(ulong blockHeight);

    public abstract Task<List<MoneroBlockHeader>> GetBlockHeadersByRange(ulong startHeight, ulong endHeight);

    public abstract Task<List<MoneroBlock>> GetBlocksByHash(List<string> blockHashes, ulong startHeight, bool prune);

    public abstract Task<List<MoneroBlock>> GetBlocksByHeight(List<ulong> blockHeights);

    public abstract Task<List<MoneroBlock>> GetBlocksByRange(ulong? startHeight, ulong? endHeight);

    public virtual async Task<List<MoneroBlock>> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight)
    {
        return await GetBlocksByRangeChunked(startHeight, endHeight, null);
    }

    public abstract Task<List<MoneroBlock>> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight,
        ulong? maxChunkSize);

    public virtual async Task<MoneroBlockTemplate> GetBlockTemplate(string walletAddress)
    {
        return await GetBlockTemplate(walletAddress, null);
    }

    public abstract Task<MoneroBlockTemplate> GetBlockTemplate(string walletAddress, int? reserveSize);

    public abstract Task<int> GetDownloadLimit();

    public virtual async Task<MoneroFeeEstimate> GetFeeEstimate()
    {
        return await GetFeeEstimate(null);
    }

    public abstract Task<MoneroFeeEstimate> GetFeeEstimate(int? graceBlocks);

    public virtual async Task<MoneroMinerTxSum> GetMinerTxSum(ulong height)
    {
        return await GetMinerTxSum(height, null);
    }

    public abstract Task<MoneroMinerTxSum> GetMinerTxSum(ulong height, ulong? numBlocks);

    public abstract Task<MoneroHardForkInfo> GetHardForkInfo();

    public abstract Task<ulong> GetHeight();

    public abstract Task<MoneroDaemonInfo> GetInfo();

    public virtual async Task<MoneroKeyImage.SpentStatus> GetKeyImageSpentStatus(string keyImage)
    {
        return (await GetKeyImageSpentStatuses([keyImage]))[0];
    }

    public abstract Task<List<MoneroKeyImage.SpentStatus>> GetKeyImageSpentStatuses(List<string> keyImage);

    public abstract Task<List<MoneroPeer>> GetKnownPeers();

    public abstract Task<MoneroBlockHeader> GetLastBlockHeader();

    public virtual List<MoneroDaemonListener> GetListeners()
    {
        return [.. _listeners];
    }

    public abstract Task<MoneroMiningStatus> GetMiningStatus();

    public virtual async Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts)
    {
        return await GetOutputDistribution(amounts, null, null, null);
    }

    public virtual async Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts,
        bool? isCumulative)
    {
        return await GetOutputDistribution(amounts, isCumulative, null, null);
    }

    public virtual async Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts,
        bool? isCumulative,
        ulong? startHeight)
    {
        return await GetOutputDistribution(amounts, isCumulative, startHeight, null);
    }

    public abstract Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts,
        bool? isCumulative,
        ulong? startHeight, ulong? endHeight);

    public virtual async Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram()
    {
        return await GetOutputHistogram(null, null, null, null, null);
    }

    public virtual async Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts)
    {
        return await GetOutputHistogram(amounts, null, null, null, null);
    }

    public virtual async Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts, int? minCount)
    {
        return await GetOutputHistogram(amounts, minCount, null, null, null);
    }

    public virtual async Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts, int? minCount,
        int? maxCount)
    {
        return await GetOutputHistogram(amounts, minCount, maxCount, null, null);
    }

    public virtual async Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts, int? minCount,
        int? maxCount, bool? isUnlocked)
    {
        return await GetOutputHistogram(amounts, minCount, maxCount, isUnlocked, null);
    }

    public abstract Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts, int? minCount,
        int? maxCount, bool? isUnlocked, int? recentCutoff);

    public abstract Task<List<MoneroOutput>> GetOutputs(List<MoneroOutput> outputs);

    public abstract Task<List<MoneroBan>> GetPeerBans();

    public abstract Task<List<MoneroPeer>> GetPeers();

    public abstract Task<MoneroDaemonSyncInfo> GetSyncInfo();

    public virtual async Task<MoneroTx?> GetTx(string txHash, bool prune)
    {
        List<MoneroTx> txs = await GetTxs([txHash], prune);

        return txs.FirstOrDefault();
    }

    public virtual async Task<string?> GetTxHex(string txHash)
    {
        return await GetTxHex(txHash, false);
    }

    public virtual async Task<string?> GetTxHex(string txHash, bool prune)
    {
        List<string> hexes = await GetTxHexes([txHash], prune);

        return hexes.FirstOrDefault();
    }

    public virtual async Task<List<string>> GetTxHexes(List<string> txHashes)
    {
        return await GetTxHexes(txHashes, false);
    }

    public abstract Task<List<string>> GetTxHexes(List<string> txHashes, bool prune);

    public abstract Task<List<MoneroTx>> GetTxPool();

    public abstract Task<List<string>> GetTxPoolHashes();

    public abstract Task<MoneroTxPoolStats> GetTxPoolStats();

    public virtual async Task<List<MoneroTx>> GetTxs(List<string> txHashes)
    {
        return await GetTxs(txHashes, false);
    }

    public abstract Task<List<MoneroTx>> GetTxs(List<string> txHashes, bool prune);

    public abstract Task<int> GetUploadLimit();

    public abstract Task<MoneroVersion> GetVersion();

    public abstract Task<bool> IsTrusted();

    public abstract Task<MoneroPruneResult> PruneBlockchain(bool check);

    public virtual async Task RelayTxByHash(string txHash)
    {
        await RelayTxsByHash([txHash]);
    }

    public abstract Task RelayTxsByHash(List<string> txHashes);

    public virtual void RemoveListener(MoneroDaemonListener listener)
    {
        lock (_listeners)
        {
            _listeners.Remove(listener);
        }
    }

    public abstract Task<int> ResetDownloadLimit();

    public abstract Task<int> ResetUploadLimit();

    public abstract Task<int> SetDownloadLimit(int limit);

    public abstract Task SetIncomingPeerLimit(int limit);

    public abstract Task SetOutgoingPeerLimit(int limit);

    public virtual async Task SetPeerBan(MoneroBan ban)
    {
        await SetPeerBans([ban]);
    }

    public abstract Task SetPeerBans(List<MoneroBan> bans);

    public abstract Task<int> SetUploadLimit(int limit);

    public abstract Task StartMining(string? address, ulong? numThreads, bool? isBackground, bool? ignoreBattery);

    public abstract Task Stop();

    public abstract Task StopMining();

    public virtual async Task SubmitBlock(string blockBlob)
    {
        await SubmitBlocks([blockBlob]);
    }

    public abstract Task SubmitBlocks(List<string> blockBlobs);

    public async Task<MoneroSubmitTxResult> SubmitTxHex(string txHex)
    {
        return await SubmitTxHex(txHex, false);
    }

    public abstract Task<MoneroSubmitTxResult> SubmitTxHex(string txHex, bool doNotRelay);

    public abstract Task<MoneroBlockHeader> WaitForNextBlockHeader();

    public virtual async Task<MoneroTx?> GetTx(string txHash)
    {
        return await GetTx(txHash, false);
    }
}