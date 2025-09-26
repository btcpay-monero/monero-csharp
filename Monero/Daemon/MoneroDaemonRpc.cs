using Monero.Common;
using Monero.Daemon.Common;
using Monero.Daemon.Rpc;

using MoneroJsonRpcParams = System.Collections.Generic.Dictionary<string, object?>;

namespace Monero.Daemon;

public class MoneroDaemonRpc : IMoneroDaemon
{
    private readonly List<MoneroDaemonListener> _listeners = [];
    private readonly MoneroRpcConnection rpc;
    private DaemonPoller? daemonPoller;

    public MoneroDaemonRpc(MoneroRpcConnection connection)
    {
        rpc = connection;
        CheckConnection();
    }

    public void AddListener(MoneroDaemonListener listener)
    {
        lock (_listeners)
        {
            _listeners.Add(listener);
            RefreshListening();
        }
    }

    public void RemoveListener(MoneroDaemonListener listener)
    {
        lock (_listeners)
        {
            _listeners.Remove(listener);
            RefreshListening();
        }
    }

    public List<MoneroDaemonListener> GetListeners()
    {
        return [.. _listeners];
    }

    public async Task<MoneroDaemonUpdateCheckResult> CheckForUpdate()
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("command", "check");
        MoneroDaemonUpdateCheckResult respMap = await rpc.SendPathRequest<MoneroDaemonUpdateCheckResult>("update", parameters);
        CheckResponseStatus(respMap);
        return respMap;
    }

    public async Task<MoneroDaemonUpdateDownloadResult> DownloadUpdate(string? path)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("command", "download");
        parameters.Add("path", path);
        MoneroDaemonUpdateDownloadResult resp = await rpc.SendPathRequest<MoneroDaemonUpdateDownloadResult>("update", parameters);
        CheckResponseStatus(resp);
        return resp;
    }

    public async Task FlushTxPool(List<string> txHashes)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("txids", txHashes);
        MoneroJsonRpcResponse<MoneroRpcResponse> resp = await rpc.SendJsonRequest<MoneroRpcResponse>("flush_txpool", parameters);
        CheckResponseStatus(resp.Result);
    }

    public async Task<List<string>> GetAltBlockHashes()
    {
        GetAltBlocksHashesResult resp = await rpc.SendPathRequest<GetAltBlocksHashesResult>("get_alt_blocks_hashes");
        CheckResponseStatus(resp);
        return resp.BlockHashes;
    }

    public async Task<List<MoneroAltChain>> GetAltChains()
    {
        MoneroJsonRpcResponse<GetAlternateChainsResult> resp = await rpc.SendJsonRequest<GetAlternateChainsResult>("get_alternate_chains");
        GetAlternateChainsResult result = resp.Result!;
        CheckResponseStatus(result);
        return result.Chains;
    }

    public async Task<MoneroBlock> GetBlockByHash(string blockHash)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("hash", blockHash);
        MoneroJsonRpcResponse<MoneroBlock> respMap = await rpc.SendJsonRequest<MoneroBlock>("get_block", parameters);
        MoneroBlock resultMap = respMap.Result!;
        resultMap.Init();
        return resultMap;
    }

    public async Task<MoneroBlock> GetBlockByHeight(ulong blockHeight)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("height", blockHeight);
        MoneroJsonRpcResponse<MoneroBlock> respMap = await rpc.SendJsonRequest<MoneroBlock>("get_block", parameters);
        MoneroBlock rpcBlock = respMap.Result!;
        rpcBlock.Init();
        return rpcBlock;
    }

    public async Task<string> GetBlockHash(ulong height)
    {
        List<ulong> param = [height];
        MoneroJsonRpcResponse<string> respMap = await rpc.SendJsonRequest("on_get_block_hash", param);
        string hash = respMap.Result ?? "";
        if (!MoneroUtils.IsValidHex(hash))
        {
            if (string.IsNullOrEmpty(hash))
            {
                throw new MoneroError("Invalid response from daemon: null or empty block hash");
            }

            throw new MoneroError(hash);
        }

        return respMap.Result ?? "";
    }

    public Task<List<string>> GetBlockHashes(List<string> blockHashes, ulong startHeight)
    {
        throw new NotImplementedException("MoneroDaemonRpc.getBlockHashes() not implemented.");
    }

    public async Task<MoneroBlockHeader> GetBlockHeaderByHash(string blockHash)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("hash", blockHash);
        MoneroJsonRpcResponse<GetLastBlockHeaderResult>
            respMap = await rpc.SendJsonRequest<GetLastBlockHeaderResult>("get_block_header_by_hash", parameters);
        GetLastBlockHeaderResult? resultMap = respMap.Result!;

        if (resultMap == null)
        {
            throw new MoneroError("Block header is null");
        }

        return resultMap.BlockHeader;
    }

    public async Task<MoneroBlockHeader> GetBlockHeaderByHeight(ulong blockHeight)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("height", blockHeight);
        MoneroJsonRpcResponse<GetLastBlockHeaderResult> respMap =
            await rpc.SendJsonRequest<GetLastBlockHeaderResult>("get_block_header_by_height", parameters);
        GetLastBlockHeaderResult? resultMap = respMap.Result;
        if (resultMap == null)
        {
            throw new MoneroError("Block header is null");
        }

        return resultMap.BlockHeader;
    }

    public async Task<List<MoneroBlockHeader>> GetBlockHeadersByRange(ulong startHeight, ulong endHeight)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("start_height", startHeight);
        parameters.Add("end_height", endHeight);
        MoneroJsonRpcResponse<GetBlockHeadersRangeResult> respMap =
            await rpc.SendJsonRequest<GetBlockHeadersRangeResult>("get_block_headers_range", parameters);
        GetBlockHeadersRangeResult resultMap = respMap.Result!;
        return resultMap.Headers;
    }

    public Task<List<MoneroBlock>> GetBlocksByHash(List<string> blockHashes, ulong startHeight, bool prune)
    {
        throw new NotImplementedException("MoneroDaemonRpc.getBlocksByHash(): not implemented.");
    }

    public async Task<MoneroBlockTemplate> GetBlockTemplate(string walletAddress, int? reserveSize)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("wallet_address", walletAddress);
        parameters.Add("reserve_size", reserveSize);
        MoneroJsonRpcResponse<MoneroBlockTemplate>
            respMap = await rpc.SendJsonRequest<MoneroBlockTemplate>("get_block_template", parameters);
        MoneroBlockTemplate? resultMap = respMap.Result;

        if (resultMap == null)
        {
            throw new MoneroError("Block template is null");
        }

        return resultMap;
    }

    public async Task<int> GetDownloadLimit()
    {
        return (await GetBandwidthLimits())[0];
    }

    public async Task<MoneroFeeEstimate> GetFeeEstimate(int? graceBlocks)
    {
        MoneroJsonRpcResponse<MoneroFeeEstimate> resp = await rpc.SendJsonRequest<MoneroFeeEstimate>("get_fee_estimate");
        MoneroFeeEstimate result = resp.Result!;
        CheckResponseStatus(result);
        return result;
    }

    public async Task<MoneroHardForkInfo> GetHardForkInfo()
    {
        MoneroJsonRpcResponse<MoneroHardForkInfo> resp = await rpc.SendJsonRequest<MoneroHardForkInfo>("hard_fork_info");
        MoneroHardForkInfo result = resp.Result!;
        CheckResponseStatus(result);
        return result;
    }

    public async Task<ulong> GetHeight()
    {
        MoneroJsonRpcResponse<GetBlockCountResult> respMap = await rpc.SendJsonRequest<GetBlockCountResult>("get_block_count");
        GetBlockCountResult resultMap = respMap.Result!;
        return resultMap.BlockCount;
    }

    public async Task<MoneroDaemonInfo> GetInfo()
    {
        MoneroJsonRpcResponse<MoneroDaemonInfo> resp = await rpc.SendJsonRequest<MoneroDaemonInfo>("get_info");
        MoneroDaemonInfo result = resp.Result!;
        CheckResponseStatus(result);
        return result;
    }

    public async Task<List<MoneroKeyImage.SpentStatus>> GetKeyImageSpentStatuses(List<string> keyImages)
    {
        if (keyImages == null || keyImages.Count == 0)
        {
            throw new MoneroError("Must provide key images to check the status of");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_images", keyImages);
        IsKeyImageSpentResult resp = await rpc.SendPathRequest<IsKeyImageSpentResult>("is_key_image_spent", parameters);
        CheckResponseStatus(resp);
        List<MoneroKeyImage.SpentStatus> statuses = [];
        foreach (int bi in resp.SpentStatus)
        {
            statuses.Add((MoneroKeyImage.SpentStatus)bi);
        }

        return statuses;
    }

    public async Task<List<MoneroPeer>> GetKnownPeers()
    {
        // send request
        GetPeerListResult respMap = await rpc.SendPathRequest<GetPeerListResult>("get_peer_list");
        CheckResponseStatus(respMap);

        List<MoneroPeer> peers = [];
        peers.AddRange(respMap.GrayList);
        peers.AddRange(respMap.WhiteList);

        return peers;
    }

    public async Task<MoneroBlockHeader> GetLastBlockHeader()
    {
        MoneroJsonRpcResponse<GetLastBlockHeaderResult> respMap = await rpc.SendJsonRequest<GetLastBlockHeaderResult>("get_last_block_header");
        GetLastBlockHeaderResult resultMap = respMap.Result!;
        CheckResponseStatus(resultMap);
        return resultMap.BlockHeader;
    }

    public async Task<MoneroMiningStatus> GetMiningStatus()
    {
        MoneroMiningStatus resp = await rpc.SendPathRequest<MoneroMiningStatus>("mining_status");
        CheckResponseStatus(resp);
        return resp;
    }

    public async Task<MoneroMinerTxSum> GetMinerTxSum(ulong height, ulong? numBlocks)
    {
        if (numBlocks == null)
        {
            numBlocks = await GetHeight();
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("height", height);
        parameters.Add("count", numBlocks);
        MoneroJsonRpcResponse<MoneroMinerTxSum> respMap =
            await rpc.SendJsonRequest<MoneroMinerTxSum>("get_coinbase_tx_sum", parameters);
        MoneroMinerTxSum resultMap = respMap.Result!;
        CheckResponseStatus(resultMap);
        return resultMap;
    }

    public Task<List<MoneroOutputDistributionEntry>> GetOutputDistribution(List<ulong> amounts,
        bool? isCumulative, ulong? startHeight, ulong? endHeight)
    {
        throw new NotImplementedException(
            "MoneroDaemonRpc.GetOutputDistribution(): not implemented (response 'distribution' field is binary)");
    }

    public async Task<List<MoneroOutputHistogramEntry>> GetOutputHistogram(List<ulong>? amounts,
        int? minCount, int? maxCount, bool? isUnlocked, int? recentCutoff)
    {
        // build request params
        MoneroJsonRpcParams parameters = [];
        parameters.Add("amounts", amounts);
        parameters.Add("min_count", minCount);
        parameters.Add("max_count", maxCount);
        parameters.Add("unlocked", isUnlocked);
        parameters.Add("recent_cutoff", recentCutoff);

        // send rpc request
        MoneroJsonRpcResponse<GetOutputHistogramResult> resp = await rpc.SendJsonRequest<GetOutputHistogramResult>("get_output_histogram", parameters);
        GetOutputHistogramResult result = resp.Result!;
        CheckResponseStatus(result);

        return result.Histogram;
    }

    public async Task<List<MoneroOutput>> GetOutputs(List<MoneroOutput> outputs)
    {
        MoneroJsonRpcParams parameters = [];
        List<Dictionary<string, object?>> outs = [];

        foreach (MoneroOutput output in outputs)
        {
            Dictionary<string, object?> outParam = [];

            outParam.Add("amount", output.GetAmount());
            outParam.Add("index", output.GetIndex());

            outs.Add(outParam);
        }

        parameters.Add("outputs", outs);
        parameters.Add("get_tx_id", true);

        MoneroJsonRpcResponse<GetOutputsResult> resp = await rpc.SendJsonRequest<GetOutputsResult>("get_outs", parameters);
        GetOutputsResult result = resp.Result!;

        CheckResponseStatus(result);
        return result.Outs;
    }

    public async Task<List<MoneroBan>> GetPeerBans()
    {
        MoneroJsonRpcResponse<GetBansResult> resp = await rpc.SendJsonRequest<GetBansResult>("get_bans");
        GetBansResult result = resp.Result!;
        CheckResponseStatus(result);
        return result.Bans;
    }

    public async Task<List<MoneroPeer>> GetPeers()
    {
        MoneroJsonRpcResponse<GetConnectionsResult> resp = await rpc.SendJsonRequest<GetConnectionsResult>("get_connections");
        GetConnectionsResult result = resp.Result!;
        CheckResponseStatus(result);
        return result.Connections;
    }

    public async Task<MoneroDaemonSyncInfo> GetSyncInfo()
    {
        MoneroJsonRpcResponse<MoneroDaemonSyncInfo> resp = await rpc.SendJsonRequest<MoneroDaemonSyncInfo>("sync_info");
        MoneroDaemonSyncInfo result = resp.Result!;
        CheckResponseStatus(result);
        return result;
    }

    public async Task<List<string>> GetTxHexes(List<string> txHashes, bool prune)
    {
        List<string> hexes = [];
        List<MoneroTx> txs = await GetTxs(txHashes, prune);
        foreach (MoneroTx tx in txs)
        {
            hexes.Add(prune ? tx.GetPrunedHex()! : tx.GetFullHex()!);
        }

        return hexes;
    }

    public async Task<List<MoneroTx>> GetTxPool()
    {
        // send rpc request
        GetTransactionPoolResult resp = await rpc.SendPathRequest<GetTransactionPoolResult>("get_transaction_pool");
        CheckResponseStatus(resp);

        return resp.Txs;
    }

    public async Task<List<string>> GetTxPoolHashes()
    {
        // TODO move to binary request
        // send rpc request
        GetTransactionPoolResult resp = await rpc.SendPathRequest<GetTransactionPoolResult>("get_transaction_pool");
        CheckResponseStatus(resp);

        return resp.TxHashes;
    }

    public async Task<MoneroTxPoolStats> GetTxPoolStats()
    {
        GetTxPoolStatsResult resp = await rpc.SendPathRequest<GetTxPoolStatsResult>("get_transaction_pool_stats");
        CheckResponseStatus(resp);
        return resp.TxPoolStats;
    }

    public async Task<List<MoneroTx>> GetTxs(List<string> txHashes, bool prune)
    {
        // validate input
        if (txHashes.Count == 0)
        {
            throw new MoneroError("Must provide an array of transaction hashes");
        }

        // fetch transactions
        MoneroJsonRpcParams parameters = [];
        parameters.Add("txs_hashes", txHashes);
        parameters.Add("decode_as_json", true);
        parameters.Add("prune", prune);
        GetTransactionsResult respMap = await rpc.SendPathRequest<GetTransactionsResult>("get_transactions", parameters);
        try
        {
            CheckResponseStatus(respMap);
        }
        catch (MoneroError e)
        {
            if (e.Message.Contains("Failed to parse hex representation of transaction hash"))
            {
                throw new MoneroError("Invalid transaction hash", e.GetCode());
            }

            throw new MoneroError(e);
        }

        return respMap.Txs;
    }

    public async Task<int> GetUploadLimit()
    {
        return (await GetBandwidthLimits())[1];
    }

    public async Task<MoneroVersion> GetVersion()
    {
        MoneroJsonRpcResponse<MoneroVersion> resp = await rpc.SendJsonRequest<MoneroVersion>("get_version");
        MoneroVersion result = resp.Result!;
        return result;
    }

    public async Task<bool> IsTrusted()
    {
        MoneroRpcResponse resp = await rpc.SendPathRequest<MoneroRpcResponse>("get_height");
        CheckResponseStatus(resp);
        return resp.Untrusted == false;
    }

    public async Task<MoneroPruneResult> PruneBlockchain(bool check)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("check", check);
        MoneroJsonRpcResponse<MoneroPruneResult> resp = await rpc.SendJsonRequest<MoneroPruneResult>("prune_blockchain", parameters);
        MoneroPruneResult resultMap = resp.Result!;
        CheckResponseStatus(resultMap);
        return resultMap;
    }

    public async Task RelayTxsByHash(List<string> txHashes)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("txids", txHashes);
        MoneroJsonRpcResponse<MoneroRpcResponse> resp = await rpc.SendJsonRequest<MoneroRpcResponse>("relay_tx", parameters);
        CheckResponseStatus(resp.Result);
    }

    public async Task<int> ResetDownloadLimit()
    {
        return (await SetBandwidthLimits(-1, 0))[0];
    }

    public async Task<int> ResetUploadLimit()
    {
        return (await SetBandwidthLimits(0, -1))[1];
    }

    public async Task<int> SetDownloadLimit(int limit)
    {
        if (limit == -1)
        {
            return await ResetDownloadLimit();
        }

        if (limit <= 0)
        {
            throw new MoneroError("Download limit must be an integer greater than 0");
        }

        return (await SetBandwidthLimits(limit, 0))[0];
    }

    public async Task SetIncomingPeerLimit(int limit)
    {
        if (limit < 0)
        {
            throw new MoneroError("Incoming peer limit must be >= 0");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("in_peers", limit);
        MoneroRpcResponse resp = await rpc.SendPathRequest<MoneroRpcResponse>("in_peers", parameters);
        CheckResponseStatus(resp);
    }

    public async Task SetOutgoingPeerLimit(int limit)
    {
        if (limit < 0)
        {
            throw new MoneroError("Outgoing peer limit must be >= 0");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("out_peers", limit);
        MoneroRpcResponse resp = await rpc.SendPathRequest<MoneroRpcResponse>("out_peers", parameters);
        CheckResponseStatus(resp);
    }

    public async Task SetPeerBans(List<MoneroBan> bans)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("bans", bans);
        MoneroJsonRpcResponse<MoneroRpcResponse> resp = await rpc.SendJsonRequest<MoneroRpcResponse>("set_bans", parameters);
        CheckResponseStatus(resp.Result);
    }

    public async Task<int> SetUploadLimit(int limit)
    {
        if (limit == -1)
        {
            return await ResetUploadLimit();
        }

        if (limit <= 0)
        {
            throw new MoneroError("Upload limit must be an integer greater than 0");
        }

        return (await SetBandwidthLimits(0, limit))[1];
    }

    public async Task StartMining(string? address, ulong? numThreads, bool? isBackground, bool? ignoreBattery)
    {
        if (string.IsNullOrEmpty(address))
        {
            throw new MoneroError("Must provide address to mine to");
        }

        if (numThreads == null)
        {
            throw new MoneroError("Number of threads must be an integer greater than 0");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("miner_address", address);
        parameters.Add("threads_count", numThreads);
        parameters.Add("do_background_mining", isBackground);
        parameters.Add("ignore_battery", ignoreBattery);
        MoneroRpcResponse resp = await rpc.SendPathRequest<MoneroRpcResponse>("start_mining", parameters);
        CheckResponseStatus(resp);
    }

    public async Task Stop()
    {
        CheckResponseStatus(await rpc.SendPathRequest<MoneroRpcResponse>("stop_daemon"));
    }

    public async Task StopMining()
    {
        MoneroRpcResponse resp = await rpc.SendPathRequest<MoneroRpcResponse>("stop_mining");
        CheckResponseStatus(resp);
    }

    public async Task SubmitBlocks(List<string> blockBlobs)
    {
        if (blockBlobs.Count == 0)
        {
            throw new MoneroError("Must provide an array of mined block blobs to submit");
        }

        MoneroJsonRpcResponse<MoneroRpcResponse> resp = await rpc.SendJsonRequest<MoneroRpcResponse>("submit_block", blockBlobs);
        CheckResponseStatus(resp.Result);
    }

    public async Task<MoneroSubmitTxResult> SubmitTxHex(string txHex, bool doNotRelay)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("tx_as_hex", txHex);
        parameters.Add("do_not_relay", doNotRelay);
        MoneroSubmitTxResult resp = await rpc.SendPathRequest<MoneroSubmitTxResult>("send_raw_transaction", parameters);
        // set isGood based on status
        try
        {
            CheckResponseStatus(resp);
            resp.SetIsGood(true);
        }
        catch (MoneroError e)
        {
            MoneroUtils.Log(1, e.Message);
            resp.SetIsGood(false);
        }

        return resp;
    }

    public Task<MoneroBlockHeader> WaitForNextBlockHeader()
    {
        object syncLock = new();
        lock (syncLock)
        {
            MoneroDaemonListener listener = new NextBlockListener(this, syncLock);
            AddListener(listener);
            try
            {
                // wait for the next block header
                Monitor.Wait(syncLock);
            }
            catch (ThreadInterruptedException)
            {
                throw new MoneroError("Waiting for next block header was interrupted");
            }
            finally
            {
                RemoveListener(listener);
            }

            return Task.FromResult(listener.GetLastBlockHeader() ?? throw new MoneroError("No block header received"));
        }
    }

    public async Task<bool> IsRestricted()
    {
        if (IsLocal())
        {
            return false;
        }

        MoneroDaemonInfo info = await GetInfo();
        return info.IsRestricted() == true;
    }

    private void CheckConnection()
    {
        if (rpc.IsConnected() == true || rpc.GetUri() == null || rpc.GetUri()!.Length == 0)
        {
            return;
        }

        rpc.CheckConnection().GetAwaiter().GetResult();
    }

    private bool IsLocal()
    {
        MoneroRpcConnection connection = GetRpcConnection();

        string? uri = connection.GetUri();

        if (uri == null)
        {
            return false;
        }

        return uri.Contains("127.0.0.1") || uri.Contains("localhost");
    }

    public MoneroRpcConnection GetRpcConnection()
    {
        return rpc;
    }

    #region Private Methods

    private async Task<List<int>> GetBandwidthLimits()
    {
        LimitResult resp = await rpc.SendPathRequest<LimitResult>("get_limit");
        CheckResponseStatus(resp);
        return [resp.LimitDown, resp.LimitUp];
    }

    private async Task<List<int>> SetBandwidthLimits(int? downLimit, int? upLimit)
    {
        if (downLimit == null)
        {
            downLimit = 0;
        }

        if (upLimit == null)
        {
            upLimit = 0;
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("limit_down", downLimit);
        parameters.Add("limit_up", upLimit);
        LimitResult resp = await rpc.SendPathRequest<LimitResult>("set_limit", parameters);
        CheckResponseStatus(resp);
        return [resp.LimitDown, resp.LimitUp];
    }

    #endregion

    #region Private Static Methods

    private static void CheckResponseStatus(MoneroRpcResponse? resp)
    {
        if (resp == null)
        {
            throw new MoneroRpcError("Empty response from server");
        }

        string status = string.IsNullOrEmpty(resp.Status) ? "Empty status response from server" : resp.Status;
        // TODO download update status is empty
        if (!"".Equals(status) && !"OK".Equals(status))
        {
            throw new MoneroRpcError(status);
        }
    }

    public static ulong? PrefixedHexToUulong(string hex)
    {
        if (!hex.StartsWith("0x"))
        {
            throw new ArgumentException("Given hex does not start with \"0x\": " + hex);
        }

        return Convert.ToUInt64(hex.Substring(2), 16);
    }

    private void RefreshListening()
    {
        if (daemonPoller == null && _listeners.Count > 0)
        {
            daemonPoller = new DaemonPoller(this);
        }

        if (daemonPoller != null)
        {
            daemonPoller.SetIsPolling(_listeners.Count > 0);
        }
    }

    #endregion
}

internal class NextBlockListener : MoneroDaemonListener
{
    private readonly MoneroDaemonRpc daemon;
    private readonly object syncLock;

    public NextBlockListener(MoneroDaemonRpc daemon, object syncLock)
    {
        // nothing to construct
        this.daemon = daemon;
        this.syncLock = syncLock;
    }

    public override void OnBlockHeader(MoneroBlockHeader header)
    {
        base.OnBlockHeader(header);
        lock (syncLock)
        {
            // notify waiting thread
            Monitor.Pulse(syncLock);
        }
    }
}

internal class DaemonPoller
{
    private static readonly ulong DEFAULT_POLL_PERIOD_IN_MS = 10000; // Poll every X ms

    private readonly MoneroDaemonRpc daemon;
    private readonly TaskLooper looper;
    private MoneroBlockHeader? lastHeader;

    public DaemonPoller(MoneroDaemonRpc daemon)
    {
        this.daemon = daemon;
        looper = new TaskLooper(async () => { await Poll(); });
    }

    public void SetIsPolling(bool isPolling)
    {
        if (isPolling)
        {
            looper.Start(DEFAULT_POLL_PERIOD_IN_MS); // TODO: allow configurable Poll period
        }
        else
        {
            looper.Stop();
        }
    }

    private async Task Poll()
    {
        try
        {
            // get first header for comparison
            if (lastHeader == null)
            {
                lastHeader = await daemon.GetLastBlockHeader();
                return;
            }

            // fetch and compare the latest block header
            MoneroBlockHeader header = await daemon.GetLastBlockHeader();
            string? headerHash = header.GetHash();
            if (headerHash != null && !headerHash.Equals(lastHeader.GetHash()))
            {
                lastHeader = header;
                lock (daemon.GetListeners())
                {
                    AnnounceBlockHeader(header);
                }
            }
        }
        catch (Exception e)
        {
            MoneroUtils.Log(0, $"[daemon-poller] error: {e.Message}");
        }
    }

    private void AnnounceBlockHeader(MoneroBlockHeader header)
    {
        foreach (MoneroDaemonListener listener in daemon.GetListeners())
        {
            try
            {
                listener.OnBlockHeader(header);
            }
            catch (Exception e)
            {
                MoneroUtils.Log(0, "Error calling listener on new block header: " + e.Message);
            }
        }
    }
}