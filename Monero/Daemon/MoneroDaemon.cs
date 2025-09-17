using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon;

public interface MoneroDaemon
{
    void AddListener(MoneroDaemonListener listener);
    void RemoveListener(MoneroDaemonListener listener);
    List<MoneroDaemonListener> GetListeners();
    Task<MoneroVersion> GetVersion();
    Task<bool> IsTrusted();
    Task<ulong> GetHeight();
    Task<string> GetBlockHash(ulong height);
    Task<MoneroBlockTemplate> GetBlockTemplate(string walletAddress, int? reserveSize);
    Task<MoneroBlockTemplate> GetBlockTemplate(string walletAddress);
    Task<MoneroBlockHeader> GetLastBlockHeader();
    Task<MoneroBlockHeader> GetBlockHeaderByHash(string blockHash);
    Task<MoneroBlockHeader> GetBlockHeaderByHeight(ulong blockHeight);
    Task<List<MoneroBlockHeader>> GetBlockHeadersByRange(ulong startHeight, ulong endHeight);
    Task<MoneroBlock> GetBlockByHash(string blockHash);
    Task<List<MoneroBlock>> GetBlocksByHash(List<string> blockHashes, ulong startHeight, bool prune);
    Task<MoneroBlock> GetBlockByHeight(ulong blockHeight);
    Task<List<MoneroBlock>> GetBlocksByHeight(List<ulong> blockHeights);
    Task<List<MoneroBlock>> GetBlocksByRange(ulong? startHeight, ulong? endHeight);
    Task<List<MoneroBlock>> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight, ulong? maxChunkSize);
    Task<List<MoneroBlock>> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight);
    Task<List<string>> GetBlockHashes(List<string> blockHashes, ulong startHeight);
    Task<MoneroTx?> GetTx(string txHash, bool prune);
    Task<List<MoneroTx>> GetTxs(List<string> txHashes, bool prune);
    Task<List<MoneroTx>> GetTxs(List<string> txHashes);
    Task<string?> GetTxHex(string txHash, bool prune);
    Task<string?> GetTxHex(string txHash);
    Task<List<string>> GetTxHexes(List<string> txHashes, bool prune);
    Task<List<string>> GetTxHexes(List<string> txHashes);
    Task<MoneroFeeEstimate> GetFeeEstimate(int? graceBlocks);
    Task<MoneroFeeEstimate> GetFeeEstimate();
    Task<MoneroMinerTxSum> GetMinerTxSum(ulong height, ulong? numBlocks);
    Task<MoneroMinerTxSum> GetMinerTxSum(ulong height);
    Task<MoneroSubmitTxResult> SubmitTxHex(string txHex, bool doNotRelay);
    Task<MoneroSubmitTxResult> SubmitTxHex(string txHex);
    Task RelayTxByHash(string txHash);
    Task RelayTxsByHash(List<string> txHashes);
    Task<List<MoneroTx>> GetTxPool();
    Task<List<string>> GetTxPoolHashes();
    Task<MoneroTxPoolStats> GetTxPoolStats();
    Task FlushTxPool();
    Task FlushTxPool(string txHash);
    Task FlushTxPool(List<string> txHashes);
    Task<MoneroKeyImage.SpentStatus> GetKeyImageSpentStatus(string keyImage);
    Task<List<MoneroKeyImage.SpentStatus>> GetKeyImageSpentStatuses(List<string> keyImage);
    Task<List<MoneroOutput>> GetOutputs(List<MoneroOutput> outputs);
    Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts, int? minCount, int? maxCount,
        bool? isUnlocked, int? recentCutoff);
    Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts, int? minCount, int? maxCount,
        bool? isUnlocked);
    Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts, int? minCount, int? maxCount);
    Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts, int? minCount);
    Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts);
    Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram();
    Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts, bool? isCumulative,
        ulong? startHeight, ulong? endHeight);
    Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts, bool? isCumulative,
        ulong? startHeight);
    Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts, bool? isCumulative);
    Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts);
    Task<MoneroDaemonInfo> GetInfo();
    Task<MoneroDaemonSyncInfo> GetSyncInfo();
    Task<MoneroHardForkInfo> GetHardForkInfo();
    Task<List<MoneroAltChain>> GetAltChains();
    Task<List<string>> GetAltBlockHashes();
    Task<int> GetDownloadLimit();
    Task<int> SetDownloadLimit(int limit);
    Task<int> ResetDownloadLimit();
    Task<int> GetUploadLimit();
    Task<int> SetUploadLimit(int limit);
    Task<int> ResetUploadLimit();
    Task<List<MoneroPeer>> GetPeers();
    Task<List<MoneroPeer>> GetKnownPeers();
    Task SetOutgoingPeerLimit(int limit);
    Task SetIncomingPeerLimit(int limit);
    Task<List<MoneroBan>> GetPeerBans();
    Task SetPeerBan(MoneroBan ban);
    Task SetPeerBans(List<MoneroBan> bans);
    Task StartMining(string? address, ulong? numThreads, bool? isBackground, bool? ignoreBattery);
    Task StopMining();
    Task<MoneroMiningStatus> GetMiningStatus();
    Task SubmitBlock(string blockBlob);
    Task SubmitBlocks(List<string> blockBlobs);
    Task<MoneroPruneResult> PruneBlockchain(bool check);
    Task<MoneroDaemonUpdateCheckResult> CheckForUpdate();
    Task<MoneroDaemonUpdateDownloadResult> DownloadUpdate();
    Task<MoneroDaemonUpdateDownloadResult> DownloadUpdate(string? path);
    Task Stop();
    Task<MoneroBlockHeader> WaitForNextBlockHeader();
}