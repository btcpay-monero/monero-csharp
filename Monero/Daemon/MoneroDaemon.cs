using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon;

public interface MoneroDaemon
{
    void AddListener(MoneroDaemonListener listener);
    void RemoveListener(MoneroDaemonListener listener);
    List<MoneroDaemonListener> GetListeners();
    MoneroVersion GetVersion();
    bool IsTrusted();
    ulong GetHeight();
    string GetBlockHash(ulong height);
    MoneroBlockTemplate GetBlockTemplate(string walletAddress, int? reserveSize = null);
    MoneroBlockHeader GetLastBlockHeader();
    MoneroBlockHeader GetBlockHeaderByHash(string blockHash);
    MoneroBlockHeader GetBlockHeaderByHeight(ulong blockHeight);
    List<MoneroBlockHeader> GetBlockHeadersByRange(ulong startHeight, ulong endHeight);
    MoneroBlock GetBlockByHash(string blockHash);
    List<MoneroBlock> GetBlocksByHash(List<string> blockHashes, ulong startHeight, bool prune);
    MoneroBlock GetBlockByHeight(ulong blockHeight);
    List<MoneroBlock> GetBlocksByHeight(List<ulong> blockHeights);
    List<MoneroBlock> GetBlocksByRange(ulong? startHeight, ulong? endHeight);
    List<MoneroBlock> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight, ulong? maxChunkSize = null);
    List<string> GetBlockHashes(List<string> blockHashes, ulong startHeight);
    MoneroTx? GetTx(string txHash, bool prune = false);
    List<MoneroTx> GetTxs(List<string> txHashes, bool prune = false);
    string? GetTxHex(string txHash, bool prune = false);
    List<string> GetTxHexes(List<string> txHashes, bool prune = false);
    MoneroFeeEstimate GetFeeEstimate(int? graceBlocks = null);
    MoneroMinerTxSum GetMinerTxSum(ulong height, ulong? numBlocks = null);
    MoneroSubmitTxResult SubmitTxHex(string txHex, bool doNotRelay = false);
    void RelayTxByHash(string txHash);
    void RelayTxsByHash(List<string> txHashes);
    List<MoneroTx> GetTxPool();
    List<string> GetTxPoolHashes();
    MoneroTxPoolStats GetTxPoolStats();
    void FlushTxPool();
    void FlushTxPool(string txHash);
    void FlushTxPool(List<string> txHashes);
    MoneroKeyImage.SpentStatus GetKeyImageSpentStatus(string keyImage);
    List<MoneroKeyImage.SpentStatus> GetKeyImageSpentStatuses(List<string> keyImage);
    List<MoneroOutput> GetOutputs(List<MoneroOutput> outputs);

    List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts = null, int? minCount = null,
        int? maxCount = null, bool? isUnlocked = null, int? recentCutoff = null);

    List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts, bool? isCumulative = null,
        ulong? startHeight = null, ulong? endHeight = null);

    MoneroDaemonInfo GetInfo();
    MoneroDaemonSyncInfo GetSyncInfo();
    MoneroHardForkInfo GetHardForkInfo();
    List<MoneroAltChain> GetAltChains();
    List<string> GetAltBlockHashes();
    int GetDownloadLimit();
    int SetDownloadLimit(int limit);
    int ResetDownloadLimit();
    int GetUploadLimit();
    int SetUploadLimit(int limit);
    int ResetUploadLimit();
    List<MoneroPeer> GetPeers();
    List<MoneroPeer> GetKnownPeers();
    void SetOutgoingPeerLimit(int limit);
    void SetIncomingPeerLimit(int limit);
    List<MoneroBan> GetPeerBans();
    void SetPeerBan(MoneroBan ban);
    void SetPeerBans(List<MoneroBan> bans);
    void StartMining(string? address, ulong? numThreads, bool? isBackground, bool? ignoreBattery);
    void StopMining();
    MoneroMiningStatus GetMiningStatus();
    void SubmitBlock(string blockBlob);
    void SubmitBlocks(List<string> blockBlobs);
    MoneroPruneResult PruneBlockchain(bool check);
    MoneroDaemonUpdateCheckResult CheckForUpdate();
    MoneroDaemonUpdateDownloadResult DownloadUpdate();
    MoneroDaemonUpdateDownloadResult DownloadUpdate(string? path);
    void Stop();
    MoneroBlockHeader WaitForNextBlockHeader();
}