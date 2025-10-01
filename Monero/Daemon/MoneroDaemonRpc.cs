using Monero.Common;
using Monero.Daemon.Common;
using Monero.Daemon.Rpc;

using MoneroJsonRpcParams = System.Collections.Generic.Dictionary<string, object?>;

namespace Monero.Daemon;

public class MoneroDaemonRpc : IMoneroDaemon
{
    private readonly List<MoneroDaemonListener> _listeners = [];
    private readonly MoneroRpcConnection _rpc;
    private MoneroDaemonPoller? _daemonPoller;

    public MoneroDaemonRpc(MoneroRpcConnection connection)
    {
        _rpc = connection;
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
        var responseMap = await _rpc.SendPathRequest<MoneroDaemonUpdateCheckResult>("update", parameters);
        CheckResponseStatus(responseMap);
        return responseMap;
    }

    public async Task<MoneroDaemonUpdateDownloadResult> DownloadUpdate(string? path)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("command", "download");
        parameters.Add("path", path);
        var resp = await _rpc.SendPathRequest<MoneroDaemonUpdateDownloadResult>("update", parameters);
        CheckResponseStatus(resp);
        return resp;
    }

    public async Task FlushTxPool(List<string> txHashes)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("txids", txHashes);
        var response = await _rpc.SendCommandAsync<MoneroJsonRpcParams, MoneroRpcResponse>("flush_txpool", parameters);
        CheckResponseStatus(response);
    }

    public async Task<List<string>> GetAltBlockHashes()
    {
        var resp = await _rpc.SendPathRequest<GetAltBlocksHashesResult>("get_alt_blocks_hashes", []);
        CheckResponseStatus(resp);
        return resp.BlockHashes;
    }

    public async Task<List<MoneroAltChain>> GetAltChains()
    {
        var response = await _rpc.SendCommandAsync<NoRequestModel, GetAlternateChainsResult>("get_alternate_chains", NoRequestModel.Instance);
        CheckResponseStatus(response);
        return response.Chains;
    }

    public async Task<MoneroBlock> GetBlockByHash(string blockHash)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("hash", blockHash);
        var responseMap = await _rpc.SendCommandAsync<MoneroJsonRpcParams, MoneroBlock>("get_block", parameters);
        responseMap.Init();
        return responseMap;
    }

    public async Task<MoneroBlock> GetBlockByHeight(ulong blockHeight)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("height", blockHeight);
        var responseMap = await _rpc.SendCommandAsync<MoneroJsonRpcParams, MoneroBlock>("get_block", parameters);
        responseMap.Init();
        return responseMap;
    }

    public async Task<string> GetBlockHash(ulong height)
    {
        List<ulong> param = [height];
        var respMap = await _rpc.SendCommandAsync<List<ulong>, string>("on_get_block_hash", param);
        if (MoneroUtils.IsValidHex(respMap))
        {
            return respMap;
        }

        if (string.IsNullOrEmpty(respMap))
        {
            throw new MoneroError("Invalid response from daemon: null or empty block hash");
        }
        throw new MoneroError(respMap);
    }

    public Task<List<string>> GetBlockHashes(List<string> blockHashes, ulong startHeight)
    {
        throw new NotImplementedException("MoneroDaemonRpc.getBlockHashes() not implemented.");
    }

    public async Task<MoneroBlockHeader> GetBlockHeaderByHash(string blockHash)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("hash", blockHash);
        var responseMap = await _rpc.SendCommandAsync<MoneroJsonRpcParams, GetLastBlockHeaderResult>("get_block_header_by_hash", parameters);

        if (responseMap == null)
        {
            throw new MoneroError("Block header is null");
        }

        return responseMap.BlockHeader;
    }

    public async Task<MoneroBlockHeader> GetBlockHeaderByHeight(ulong blockHeight)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("height", blockHeight);
        var responseMap = await _rpc.SendCommandAsync<MoneroJsonRpcParams, GetLastBlockHeaderResult>("get_block_header_by_height", parameters);
        if (responseMap == null)
        {
            throw new MoneroError("Block header is null");
        }

        return responseMap.BlockHeader;
    }

    public async Task<List<MoneroBlockHeader>> GetBlockHeadersByRange(ulong startHeight, ulong endHeight)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("start_height", startHeight);
        parameters.Add("end_height", endHeight);
        var resultMap = await _rpc.SendCommandAsync<MoneroJsonRpcParams, GetBlockHeadersRangeResult>("get_block_headers_range", parameters);
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
        var resultMap = await _rpc.SendCommandAsync<MoneroJsonRpcParams, MoneroBlockTemplate>("get_block_template", parameters);

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

    public async Task<MoneroFeeEstimate> GetFeeEstimate()
    {
        var response = await _rpc.SendCommandAsync<NoRequestModel, MoneroFeeEstimate>("get_fee_estimate", NoRequestModel.Instance);
        CheckResponseStatus(response);
        return response;
    }

    public async Task<MoneroHardForkInfo> GetHardForkInfo()
    {
        var response = await _rpc.SendCommandAsync<NoRequestModel, MoneroHardForkInfo>("hard_fork_info", NoRequestModel.Instance);
        CheckResponseStatus(response);
        return response;
    }

    public async Task<ulong> GetHeight()
    {
        var responseMap = await _rpc.SendCommandAsync<NoRequestModel, GetBlockCountResult>("get_block_count", NoRequestModel.Instance);
        return responseMap.BlockCount;
    }

    public async Task<MoneroDaemonInfo> GetInfo()
    {
        var response = await _rpc.SendCommandAsync<NoRequestModel, MoneroDaemonInfo>("get_info", NoRequestModel.Instance);
        CheckResponseStatus(response);
        return response;
    }

    public async Task<List<MoneroKeyImage.SpentStatus>> GetKeyImageSpentStatuses(List<string> keyImages)
    {
        if (keyImages == null || keyImages.Count == 0)
        {
            throw new MoneroError("Must provide key images to check the status of");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_images", keyImages);
        var resp = await _rpc.SendPathRequest<IsKeyImageSpentResult>("is_key_image_spent", parameters);
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
        var respMap = await _rpc.SendPathRequest<GetPeerListResult>("get_peer_list", []);
        CheckResponseStatus(respMap);

        List<MoneroPeer> peers = [];
        peers.AddRange(respMap.GrayList);
        peers.AddRange(respMap.WhiteList);

        return peers;
    }

    public async Task<MoneroBlockHeader> GetLastBlockHeader()
    {
        var respMap = await _rpc.SendJsonRequest<GetLastBlockHeaderResult>("get_last_block_header", []);
        GetLastBlockHeaderResult resultMap = respMap.Result!;
        CheckResponseStatus(resultMap);
        return resultMap.BlockHeader;
    }

    public async Task<MoneroMiningStatus> GetMiningStatus()
    {
        var resp = await _rpc.SendPathRequest<MoneroMiningStatus>("mining_status", []);
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
        var resultMap = await _rpc.SendCommandAsync<MoneroJsonRpcParams, MoneroMinerTxSum>("get_coinbase_tx_sum", parameters);
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
        var result = await _rpc.SendCommandAsync<MoneroJsonRpcParams, GetOutputHistogramResult>("get_output_histogram", parameters);
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

        var result = await _rpc.SendCommandAsync<MoneroJsonRpcParams, GetOutputsResult>("get_outs", parameters);

        CheckResponseStatus(result);
        return result.Outs;
    }

    public async Task<List<MoneroBan>> GetPeerBans()
    {
        var response = await _rpc.SendCommandAsync<NoRequestModel, GetBansResult>("get_bans", NoRequestModel.Instance);
        CheckResponseStatus(response);
        return response.Bans;
    }

    public async Task<List<MoneroPeer>> GetPeers()
    {
        var response = await _rpc.SendCommandAsync<NoRequestModel, GetConnectionsResult>("get_connections", NoRequestModel.Instance);
        CheckResponseStatus(response);
        return response.Connections;
    }

    public async Task<MoneroDaemonSyncInfo> GetSyncInfo()
    {
        var response = await _rpc.SendCommandAsync<NoRequestModel, MoneroDaemonSyncInfo>("sync_info", NoRequestModel.Instance);
        CheckResponseStatus(response);
        return response;
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
        var resp = await _rpc.SendPathRequest<GetTransactionPoolResult>("get_transaction_pool", []);
        CheckResponseStatus(resp);

        return resp.Txs;
    }

    public async Task<List<string>> GetTxPoolHashes()
    {
        // TODO move to binary request
        // send rpc request
        var resp = await _rpc.SendPathRequest<GetTransactionPoolResult>("get_transaction_pool", []);
        CheckResponseStatus(resp);

        return resp.TxHashes;
    }

    public async Task<MoneroTxPoolStats> GetTxPoolStats()
    {
        var resp = await _rpc.SendPathRequest<GetTxPoolStatsResult>("get_transaction_pool_stats", []);
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
        var respMap = await _rpc.SendPathRequest<GetTransactionsResult>("get_transactions", parameters);
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
        return await _rpc.SendCommandAsync<NoRequestModel, MoneroVersion>("get_version", NoRequestModel.Instance);
    }

    public async Task<bool> IsTrusted()
    {
        var resp = await _rpc.SendPathRequest<MoneroRpcResponse>("get_height", []);
        CheckResponseStatus(resp);
        return resp.Untrusted == false;
    }

    public async Task<MoneroPruneResult> PruneBlockchain(bool check)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("check", check);
        var resultMap = await _rpc.SendCommandAsync<MoneroJsonRpcParams, MoneroPruneResult>("prune_blockchain", parameters);
        CheckResponseStatus(resultMap);
        return resultMap;
    }

    public async Task RelayTxsByHash(List<string> txHashes)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("txids", txHashes);
        var response = await _rpc.SendCommandAsync<MoneroJsonRpcParams, MoneroRpcResponse>("relay_tx", parameters);
        CheckResponseStatus(response);
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
        var resp = await _rpc.SendPathRequest<MoneroRpcResponse>("in_peers", parameters);
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
        var resp = await _rpc.SendPathRequest<MoneroRpcResponse>("out_peers", parameters);
        CheckResponseStatus(resp);
    }

    public async Task SetPeerBans(List<MoneroBan> bans)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("bans", bans);
        var response = await _rpc.SendCommandAsync<MoneroJsonRpcParams, MoneroRpcResponse>("set_bans", parameters);
        CheckResponseStatus(response);
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
        var resp = await _rpc.SendPathRequest<MoneroRpcResponse>("start_mining", parameters);
        CheckResponseStatus(resp);
    }

    public async Task Stop()
    {
        CheckResponseStatus(await _rpc.SendPathRequest<MoneroRpcResponse>("stop_daemon", []));
    }

    public async Task StopMining()
    {
        var resp = await _rpc.SendPathRequest<MoneroRpcResponse>("stop_mining", []);
        CheckResponseStatus(resp);
    }

    public async Task SubmitBlocks(List<string> blockBlobs)
    {
        if (blockBlobs.Count == 0)
        {
            throw new MoneroError("Must provide an array of mined block blobs to submit");
        }

        var resp = await _rpc.SendCommandAsync<List<string>, MoneroRpcResponse>("submit_block", blockBlobs);
        CheckResponseStatus(resp);
    }

    public async Task<MoneroSubmitTxResult> SubmitTxHex(string txHex, bool doNotRelay)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("tx_as_hex", txHex);
        parameters.Add("do_not_relay", doNotRelay);
        var resp = await _rpc.SendPathRequest<MoneroSubmitTxResult>("send_raw_transaction", parameters);
        // set isGood based on status
        try
        {
            CheckResponseStatus(resp);
            resp.IsGood = true;
        }
        catch (MoneroError e)
        {
            MoneroUtils.Log(1, e.Message);
            resp.IsGood = false;
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

    public MoneroRpcConnection GetRpcConnection()
    {
        return _rpc;
    }

    #region Private Methods

    private async Task<List<int>> GetBandwidthLimits()
    {
        LimitResult resp = await _rpc.SendPathRequest<LimitResult>("get_limit", []);
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
        LimitResult resp = await _rpc.SendPathRequest<LimitResult>("set_limit", parameters);
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

    private void RefreshListening()
    {
        if (_daemonPoller == null && _listeners.Count > 0)
        {
            _daemonPoller = new MoneroDaemonPoller(this);
        }

        if (_daemonPoller != null)
        {
            _daemonPoller.SetIsPolling(_listeners.Count > 0);
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