﻿using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon
{
    public interface MoneroDaemon
    {
        public void AddListener(MoneroDaemonListener listener);
        public void RemoveListener(MoneroDaemonListener listener);
        public List<MoneroDaemonListener> GetListeners();
        public MoneroVersion GetVersion();
        public bool IsTrusted();
        public ulong GetHeight();
        public string GetBlockHash(ulong height);
        public MoneroBlockTemplate GetBlockTemplate(string walletAddress, int? reserveSize = null);
        public MoneroBlockHeader GetLastBlockHeader();
        public MoneroBlockHeader GetBlockHeaderByHash(string blockHash);
        public MoneroBlockHeader GetBlockHeaderByHeight(ulong blockHeight);
        public List<MoneroBlockHeader> GetBlockHeadersByRange(ulong startHeight, ulong endHeight);
        public MoneroBlock GetBlockByHash(string blockHash);
        public List<MoneroBlock> GetBlocksByHash(List<string> blockHashes, ulong startHeight, bool prune);
        public MoneroBlock GetBlockByHeight(ulong blockHeight);
        public List<MoneroBlock> GetBlocksByHeight(List<ulong> blockHeights);
        public List<MoneroBlock> GetBlocksByRange(ulong? startHeight, ulong? endHeight);
        public List<MoneroBlock> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight, ulong? maxChunkSize = null);
        public List<string> GetBlockHashes(List<string> blockHashes, ulong startHeight);
        public MoneroTx? GetTx(string txHash, bool prune = false);
        public List<MoneroTx> GetTxs(List<string> txHashes, bool prune = false);
        public string? GetTxHex(string txHash, bool prune = false);
        public List<string> GetTxHexes(List<string> txHashes, bool prune = false);
        public MoneroFeeEstimate GetFeeEstimate(int? graceBlocks = null);
        public MoneroMinerTxSum GetMinerTxSum(ulong height, ulong? numBlocks = null);
        public MoneroSubmitTxResult SubmitTxHex(string txHex, bool doNotRelay = false);
        public void RelayTxByHash(string txHash);
        public void RelayTxsByHash(List<string> txHashes);
        public List<MoneroTx> GetTxPool();
        public List<string> GetTxPoolHashes();
        public MoneroTxPoolStats GetTxPoolStats();
        public void FlushTxPool();
        public void FlushTxPool(string txHash);
        public void FlushTxPool(List<string> txHashes);
        public MoneroKeyImage.SpentStatus GetKeyImageSpentStatus(string keyImage);
        public List<MoneroKeyImage.SpentStatus> GetKeyImageSpentStatuses(List<string> keyImage);
        public List<MoneroOutput> GetOutputs(List<MoneroOutput> outputs);
        public List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts = null, int? minCount = null, int? maxCount = null, bool? isUnlocked = null, int? recentCutoff = null);
        public List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts, bool? isCumulative = null, ulong? startHeight = null, ulong? endHeight = null);
        public MoneroDaemonInfo GetInfo();
        public MoneroDaemonSyncInfo GetSyncInfo();
        public MoneroHardForkInfo GetHardForkInfo();
        public List<MoneroAltChain> GetAltChains();
        public List<string> GetAltBlockHashes();
        public int GetDownloadLimit();
        public int SetDownloadLimit(int limit);
        public int ResetDownloadLimit();
        public int GetUploadLimit();
        public int SetUploadLimit(int limit);
        public int ResetUploadLimit();
        public List<MoneroPeer> GetPeers();
        public List<MoneroPeer> GetKnownPeers();
        public void SetOutgoingPeerLimit(int limit);
        public void SetIncomingPeerLimit(int limit);
        public List<MoneroBan> GetPeerBans();
        public void SetPeerBan(MoneroBan ban);
        public void SetPeerBans(List<MoneroBan> bans);
        public void StartMining(string? address, ulong? numThreads, bool? isBackground, bool? ignoreBattery);
        public void StopMining();
        public MoneroMiningStatus GetMiningStatus();
        public void SubmitBlock(string blockBlob);
        public void SubmitBlocks(List<string> blockBlobs);
        public MoneroPruneResult PruneBlockchain(bool check);
        public MoneroDaemonUpdateCheckResult CheckForUpdate();
        public MoneroDaemonUpdateDownloadResult DownloadUpdate(string? path = null);
        public void Stop();
        public MoneroBlockHeader WaitForNextBlockHeader();
    }
}
