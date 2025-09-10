using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon;

public class MoneroDaemonLws : MoneroDaemonDefault
{
    #region Override Base Methods

    public override MoneroDaemonUpdateCheckResult CheckForUpdate()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroDaemonUpdateDownloadResult DownloadUpdate()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroDaemonUpdateDownloadResult DownloadUpdate(string path)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void FlushTxPool(List<string> txHashes)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<string> GetAltBlockHashes()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroAltChain> GetAltChains()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroBlock GetBlockByHash(string blockHash)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroBlock GetBlockByHeight(ulong blockHeight)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override string GetBlockHash(ulong height)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<string> GetBlockHashes(List<string> blockHashes, ulong startHeight)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroBlockHeader GetBlockHeaderByHash(string blockHash)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroBlockHeader GetBlockHeaderByHeight(ulong blockHeight)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroBlockHeader> GetBlockHeadersByRange(ulong startHeight, ulong endHeight)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroBlock> GetBlocksByHash(List<string> blockHashes, ulong startHeight, bool prune)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroBlock> GetBlocksByHeight(List<ulong> blockHeights)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroBlock> GetBlocksByRange(ulong? startHeight, ulong? endHeight)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroBlock> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight, ulong? maxChunkSize = null)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroBlockTemplate GetBlockTemplate(string walletAddress, int? reserveSize = null)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override int GetDownloadLimit()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroFeeEstimate GetFeeEstimate(int? graceBlocks = null)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroHardForkInfo GetHardForkInfo()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override ulong GetHeight()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroDaemonInfo GetInfo()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroKeyImage.SpentStatus> GetKeyImageSpentStatuses(List<string> keyImage)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroPeer> GetKnownPeers()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroBlockHeader GetLastBlockHeader()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroMinerTxSum GetMinerTxSum(ulong height, ulong? numBlocks = null)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroMiningStatus GetMiningStatus()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts, bool? isCumulative = null, ulong? startHeight = null, ulong? endHeight = null)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts = null, int? minCount = null, int? maxCount = null, bool? isUnlocked = null, int? recentCutoff = null)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroOutput> GetOutputs(List<MoneroOutput> outputs)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroBan> GetPeerBans()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroPeer> GetPeers()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroDaemonSyncInfo GetSyncInfo()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroTx GetTx(string txHash, bool prune = false)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override string GetTxHex(string txHash, bool prune = false)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<string> GetTxHexes(List<string> txHashes, bool prune = false)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroTx> GetTxPool()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<string> GetTxPoolHashes()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroTxPoolStats GetTxPoolStats()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override List<MoneroTx> GetTxs(List<string> txHashes, bool prune = false)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override int GetUploadLimit()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroVersion GetVersion()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override bool IsTrusted()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroPruneResult PruneBlockchain(bool check)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void RelayTxsByHash(List<string> txHashes)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override int ResetDownloadLimit()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override int ResetUploadLimit()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override int SetDownloadLimit(int limit)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void SetIncomingPeerLimit(int limit)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void SetOutgoingPeerLimit(int limit)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void SetPeerBans(List<MoneroBan> bans)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override int SetUploadLimit(int limit)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void StartMining(string? address, ulong? numThreads, bool? isBackground, bool? ignoreBattery)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void Stop()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void StopMining()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override void SubmitBlocks(List<string> blockBlobs)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroSubmitTxResult SubmitTxHex(string txHex, bool doNotRelay = false)
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    public override MoneroBlockHeader WaitForNextBlockHeader()
    {
        throw new NotImplementedException("Not supported by monero-lws");
    }

    #endregion

    #region LWS Administration Methods

    public void AddWallet(string address, string viewKey)
    {
        throw new NotImplementedException("Not implemented");
    }

    public void EnableWallets(List<string> addresses)
    {
        throw new NotImplementedException("Not implemented");
    }

    public void EnableWallet(string address)
    {
        EnableWallets([address]);
    }

    public void DisableWallets(List<string> addresses)
    {
        throw new NotImplementedException("Not implemented");
    }

    public void DisableWallet(string address)
    {
        DisableWallets([address]);
    }

    public void RemoveWallets(List<string> addresses)
    {
        throw new NotImplementedException("Not implemented");
    }

    public void RemoveWallet(string address)
    {
        RemoveWallets([address]);
    }

    public void RescanWallets(List<string> addresses, ulong height = 0)
    {
        throw new NotImplementedException("Not implemented");
    }

    public void RescanWallet(string address, ulong height = 0)
    {
        RescanWallets([address], height);
    }

    public void AcceptRequests(string requestType, List<string> addresses)
    {
        throw new NotImplementedException("Not implemented");
    }

    public void AcceptRescanRequests(List<string> addresses)
    {
        AcceptRequests("import", addresses);
    }

    public void AcceptCreateRequests(List<string> addresses)
    {
        AcceptRequests("create", addresses);
    }

    public void RejectRequests(string requestType, List<string> addresses)
    {
        throw new NotImplementedException("Not implemented");
    }

    public void RejectRescanRequests(List<string> addresses)
    {
        RejectRequests("import", addresses);
    }

    public void RejectCreateRequests(List<string> addresses)
    {
        RejectRequests("create", addresses);
    }

    public string ValidateWalletKeys(string publicSpendKey, string publicViewKey, string privateViewKey)
    {
        throw new NotImplementedException("Not implemented");
    }

    #endregion

}
