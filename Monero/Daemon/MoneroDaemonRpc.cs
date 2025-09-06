﻿using Monero.Common;
using Monero.Daemon.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

using MoneroJsonRpcParams = System.Collections.Generic.Dictionary<string, object?>;

namespace Monero.Daemon
{
    public class MoneroDaemonRpc : MoneroDaemonDefault
    {
        private static readonly string DefaultId = "0000000000000000000000000000000000000000000000000000000000000000";
        private static readonly ulong MaxReqSize = 3000000;  // max request size when fetching blocks from daemon
        private static readonly uint NumHeadersPerReq = 750;
        
        private MoneroRpcConnection? _rpc;
        private Process? _process;
        private Thread? _outputThread;
        private DaemonPoller? _daemonPoller;

        public MoneroDaemonRpc(MoneroRpcConnection connection)
        {
            _rpc = connection;
            CheckConnection();
        }

        public MoneroDaemonRpc(string uri, string? username = null, string? password = null)
        {
            _rpc = new MoneroRpcConnection(uri, username, password);
            CheckConnection();
        }

        public MoneroDaemonRpc(List<string> cmd)
        {
            if (cmd == null || cmd.Count == 0)
                throw new ArgumentException("Command list cannot be empty");

            var startInfo = new ProcessStartInfo
            {
                FileName = cmd[0],
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            for (int i = 1; i < cmd.Count; i++)
            {
                startInfo.ArgumentList.Add(cmd[i]);
            }

            startInfo.Environment["LANG"] = "en_US.UTF-8";

            _process = new Process { StartInfo = startInfo };
            _process.Start();

            StreamReader reader = _process.StandardOutput;
            string? line;
            bool started = false;
            var capturedOutput = new List<string>();
            string uri = "";
            string zmqUri = "";
            string username = "";
            string password = "";

            while ((line = reader.ReadLine()) != null)
            {
                Console.WriteLine("[monerod] " + line);
                capturedOutput.Add(line);

                var match = Regex.Match(line, @"Binding on ([\d.]+).*:(\d+)");
                if (match.Success)
                {
                    string host = match.Groups[1].Value;
                    string port = match.Groups[2].Value;
                    bool ssl = false;

                    int sslIndex = cmd.IndexOf("--rpc-ssl");
                    if (sslIndex >= 0 && sslIndex + 1 < cmd.Count)
                        ssl = cmd[sslIndex + 1].ToLower() == "enabled";

                    uri = (ssl ? "https://" : "http://") + host + ":" + port;
                }

                if (line.Contains("Starting wallet RPC server"))
                {
                    _outputThread = new Thread(() =>
                    {
                        string? bgLine;
                        try
                        {
                            while ((bgLine = reader.ReadLine()) != null)
                            {
                                Console.WriteLine("[monerod] " + bgLine);
                            }
                        }
                        catch (Exception) {  }
                    });
                    _outputThread.Start();

                    started = true;
                    break;
                }
            }

            if (!started)
            {
                _process.Kill();
                throw new Exception("Failed to start monerod:\n" + string.Join("\n", capturedOutput));
            }

            int loginIndex = cmd.IndexOf("--rpc-login");
            if (loginIndex >= 0 && loginIndex + 1 < cmd.Count)
            {
                string[] parts = cmd[loginIndex + 1].Split(':');
                if (parts.Length == 2)
                {
                    username = parts[0];
                    password = parts[1];
                }
            }

            int zmqIndex = cmd.IndexOf("--zmq-pub");
            if (zmqIndex >= 0 && zmqIndex + 1 < cmd.Count)
            {
                zmqUri = cmd[zmqIndex + 1];
            }

            _rpc = new MoneroRpcConnection(uri, username, password, zmqUri);
            CheckConnection();
        }

        public bool IsRestricted()
        {
            if (IsLocal()) return false;

            var info = GetInfo();
            return info.IsRestricted() == true;
        }

        private void CheckConnection()
        {
            if (_rpc == null || _rpc.IsConnected() == true || _rpc.GetUri() == null || _rpc.GetUri()!.Length == 0) return;

            _rpc.CheckConnection();
        }

        private bool IsLocal()
        {
            var connection = GetRpcConnection();

            if (connection == null) return false;

            var uri = connection.GetUri();

            if (uri == null) return false;

            return uri.Contains("127.0.0.1") || uri.Contains("localhost");
        }

        public Process? GetProcess()
        {
            return _process;
        }

        public int StopProcess(bool force = false)
        {
            if (_process == null) throw new MoneroError("MoneroDaemonRpc instance not created from new process");
            _listeners.Clear();
            RefreshListening();
            if (force) _process.Kill(true);
            else _process.Close();
            try { 
                _process.WaitForExit();
                if (_outputThread != null)
                {
                    _outputThread.Join();
                }
                var exitCode = _process.ExitCode;
                _process = null;
                return exitCode;
            }
            catch (Exception e) { throw new MoneroError(e.Message); }
        }

        public override void AddListener(MoneroDaemonListener listener)
        {
            base.AddListener(listener);
            RefreshListening();
        }

        public override void RemoveListener(MoneroDaemonListener listener)
        {
            base.RemoveListener(listener);
            RefreshListening();
        }

        public MoneroRpcConnection? GetRpcConnection()
        {
            return _rpc;
        }

        public void SetProxyUri(string? uri)
        {
            _rpc!.SetProxyUri(uri);
        }

        public string? GetProxyUri()
        {
            return _rpc!.GetProxyUri();
        }

        public bool IsConnected()
        {
            try
            {
                GetVersion();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override MoneroDaemonUpdateCheckResult CheckForUpdate()
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("command", "check");
            var respMap = _rpc!.SendPathRequest("update", parameters);
            CheckResponseStatus(respMap);
            return ConvertRpcUpdateCheckResult(respMap);
        }

        public override MoneroDaemonUpdateDownloadResult DownloadUpdate(string? path = null)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("command", "download");
            parameters.Add("path", path);
            var resp = _rpc!.SendPathRequest("update", parameters);
            CheckResponseStatus(resp);
            return ConvertRpcUpdateDownloadResult(resp);
        }

        public override void FlushTxPool(List<string> txHashes)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txids", txHashes);
            var resp = _rpc!.SendJsonRequest("flush_txpool", parameters);
            CheckResponseStatus(resp.Result);
        }

        public override List<string> GetAltBlockHashes()
        {
            var resp = _rpc!.SendPathRequest("get_alt_blocks_hashes");
            CheckResponseStatus(resp);
            if (!resp.ContainsKey("blks_hashes")) return [];
            return ((JArray)resp["blks_hashes"]).ToObject<List<string>>()!;
        }

        public override List<MoneroAltChain> GetAltChains()
        {
            var resp = _rpc!.SendJsonRequest("get_alternate_chains");
            var result = resp.Result!;
            CheckResponseStatus(result);
            List<MoneroAltChain> chains = [];
            if (!result.ContainsKey("chains")) return chains;
            foreach (var rpcChain in (JArray)result["chains"]) chains.Add(ConvertRpcAltChain(rpcChain.ToObject<Dictionary<string, object>>()!));
            return chains;
        }

        public override MoneroBlock GetBlockByHash(string blockHash)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("hash", blockHash);
            var respMap = _rpc!.SendJsonRequest("get_block", parameters);
            var resultMap = respMap.Result!;
            MoneroBlock block = ConvertRpcBlock(resultMap);
            return block;
        }

        public override MoneroBlock GetBlockByHeight(ulong blockHeight)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("height", blockHeight);
            var respMap = _rpc!.SendJsonRequest("get_block", parameters);
            var rpcBlock = respMap.Result!;
            MoneroBlock block = ConvertRpcBlock(rpcBlock);
            return block;
        }

        public override string GetBlockHash(ulong height)
        {
            var param = new List<ulong>() { height };
            var respMap = _rpc!.SendJsonStringRequest("on_get_block_hash", param);
            var hash = respMap.Result ?? "";
            if (!MoneroUtils.IsValidHex(hash)) 
            {
                if (string.IsNullOrEmpty(hash)) throw new MoneroError("Invalid response from daemon: null or empty block hash");
                else throw new MoneroError(hash);
            }

            return respMap.Result ?? "";
        }

        public override List<string> GetBlockHashes(List<string> blockHashes, ulong startHeight)
        {
            throw new NotImplementedException("MoneroDaemonRpc.getBlockHashes() not implemented.");
        }

        public override MoneroBlockHeader GetBlockHeaderByHash(string blockHash)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("hash", blockHash);
            var respMap = _rpc!.SendJsonRequest("get_block_header_by_hash", parameters);
            var resultMap = respMap.Result!;
            MoneroBlockHeader header = ConvertRpcBlockHeader(((JObject)resultMap["block_header"]).ToObject<Dictionary<string, object>>());
            return header;
        }

        public override MoneroBlockHeader GetBlockHeaderByHeight(ulong blockHeight)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("height", blockHeight);
            var respMap = _rpc!.SendJsonRequest("get_block_header_by_height", parameters);
            var resultMap = respMap.Result!;
            MoneroBlockHeader header = ConvertRpcBlockHeader(((JObject)resultMap["block_header"]).ToObject<Dictionary<string, object>>());
            return header;
        }

        public override List<MoneroBlockHeader> GetBlockHeadersByRange(ulong startHeight, ulong endHeight)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("start_height", startHeight);
            parameters.Add("end_height", endHeight);
            var respMap = _rpc!.SendJsonRequest("get_block_headers_range", parameters);
            var resultMap = respMap.Result!;
            var rpcHeaders = (JArray)resultMap["headers"];
            List<MoneroBlockHeader> headers = [];
            foreach (var rpcHeader in rpcHeaders)
            {
                MoneroBlockHeader header = ConvertRpcBlockHeader(rpcHeader.ToObject<Dictionary<string, object>>());
                headers.Add(header);
            }
            return headers;
        }

        public override List<MoneroBlock> GetBlocksByHash(List<string> blockHashes, ulong startHeight, bool prune)
        {
            throw new NotImplementedException("MoneroDaemonRpc.getBlocksByHash(): not implemented.");
        }

        public override List<MoneroBlock> GetBlocksByHeight(List<ulong> blockHeights)
        {
            // fetch blocks in binary
            MoneroJsonRpcParams parameters = [];
            parameters.Add("heights", blockHeights);
            byte[] respBin = _rpc!.SendBinaryRequest("get_blocks_by_height.bin", parameters);

            // Convert binary blocks to map
            var rpcResp = MoneroUtils.BinaryBlocksToMap(respBin);
            CheckResponseStatus(rpcResp);

            // build blocks with transactions
            List<MoneroBlock> blocks = [];
            var rpcBlocks = (JArray)rpcResp["blocks"];
            var rpcTxs = (List<List<Dictionary<string, object>>>)rpcResp["txs"];
            if (rpcBlocks.Count != rpcTxs.Count) throw new MoneroError("Blocks and txs count mismatch");
            for (int blockIdx = 0; blockIdx < rpcBlocks.Count; blockIdx++)
            {

                // build block
                MoneroBlock block = ConvertRpcBlock(rpcBlocks[blockIdx].ToObject<Dictionary<string, object>>()!);
                block.SetHeight(blockHeights[blockIdx]);
                blocks.Add(block);

                // build transactions
                List<MoneroTx> txs = [];
                for (int txIdx = 0; txIdx < rpcTxs[blockIdx].Count; txIdx++)
                {
                    var tx = new MoneroTx();
                    txs.Add(tx);
                    List<string> txHashes = ((JArray)rpcBlocks[blockIdx]["tx_hashes"]!).ToObject<List<string>>()!;
                    tx.SetHash(txHashes[txIdx]);
                    tx.SetIsConfirmed(true);
                    tx.SetInTxPool(false);
                    tx.SetIsMinerTx(false);
                    tx.SetRelay(true);
                    tx.SetIsRelayed(true);
                    tx.SetIsFailed(false);
                    tx.SetIsDoubleSpendSeen(false);
                    var blockTxs = rpcTxs[blockIdx];
                    ConvertRpcTx(blockTxs[txIdx], tx);
                }

                // merge into one block
                block.SetTxs([]);
                foreach (MoneroTx tx in txs)
                {
                    if (tx.GetBlock() != null) block.Merge(tx.GetBlock());
                    else block.GetTxs()!.Add(tx.SetBlock(block));
                }
            }

            return blocks;
        }

        public override List<MoneroBlock> GetBlocksByRange(ulong? startHeight, ulong? endHeight)
        {
            if (startHeight == null) startHeight = 0;
            if (endHeight == null) endHeight = GetHeight() - 1;
            List<ulong> heights = [];
            for (ulong height = (ulong)startHeight; height <= endHeight; height++) heights.Add(height);
            return GetBlocksByHeight(heights);
        }

        public override List<MoneroBlock> GetBlocksByRangeChunked(ulong? startHeight, ulong? endHeight, ulong? maxChunkSize = null)
        {
            if (startHeight == null) startHeight = 0;
            if (endHeight == null) endHeight = GetHeight() - 1;
            ulong lastHeight = (ulong)startHeight - 1;
            List<MoneroBlock> blocks = [];
            while (lastHeight < endHeight)
            {
                blocks.AddRange(GetMaxBlocks(lastHeight + 1, endHeight, maxChunkSize));
                lastHeight = (ulong)blocks[blocks.Count - 1].GetHeight()!;
            }
            return blocks;
        }

        public override MoneroBlockTemplate GetBlockTemplate(string walletAddress, int? reserveSize = null)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("wallet_address", walletAddress);
            parameters.Add("reserve_size", reserveSize);
            var respMap = _rpc!.SendJsonRequest("get_block_template", parameters);
            var resultMap = respMap.Result!;
            MoneroBlockTemplate template = ConvertRpcBlockTemplate(resultMap);
            return template;
        }

        public override int GetDownloadLimit()
        {
            return GetBandwidthLimits()[0];
        }

        public override MoneroFeeEstimate GetFeeEstimate(int? graceBlocks = null)
        {
            var resp = _rpc!.SendJsonRequest("get_fee_estimate");
            var result = resp.Result!;
            CheckResponseStatus(result);
            MoneroFeeEstimate feeEstimate = new MoneroFeeEstimate();
            feeEstimate.SetFee(Convert.ToUInt64(result["fee"]));
            feeEstimate.SetQuantizationMask(Convert.ToUInt64(result["quantization_mask"]));
            if (result.ContainsKey("fees"))
            {
                List<ulong> fees = [];
                foreach (var fee in ((JArray)result["fees"])) fees.Add(Convert.ToUInt64(fee));
                feeEstimate.SetFees(fees);
            }
            return feeEstimate;
        }

        public override MoneroHardForkInfo GetHardForkInfo()
        {
            var resp = _rpc!.SendJsonRequest("hard_fork_info");
            var result = resp.Result!;
            CheckResponseStatus(result);
            return ConvertRpcHardForkInfo(result);
        }

        public override ulong GetHeight()
        {
            var respMap = _rpc!.SendJsonRequest("get_block_count");
            var resultMap = respMap.Result!;
            return (Convert.ToUInt64(resultMap["count"]));
        }

        public override MoneroDaemonInfo GetInfo()
        {
            var resp = _rpc!.SendJsonRequest("get_info");
            var result = resp.Result!;
            CheckResponseStatus(result);
            return ConvertRpcInfo(result)!;
        }

        public override List<MoneroKeyImage.SpentStatus> GetKeyImageSpentStatuses(List<string> keyImages)
        {
            if (keyImages == null || keyImages.Count == 0) throw new MoneroError("Must provide key images to check the status of");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_images", keyImages);
            var resp = _rpc!.SendPathRequest("is_key_image_spent", parameters);
            CheckResponseStatus(resp);
            List<MoneroKeyImage.SpentStatus> statuses = [];
            foreach (int bi in (List<int>)resp["spent_status"])
            {
                statuses.Add((MoneroKeyImage.SpentStatus)bi);
            }
            return statuses;
        }

        public override List<MoneroPeer> GetKnownPeers()
        {
            // send request
            var respMap = _rpc!.SendPathRequest("get_peer_list");
            CheckResponseStatus(respMap);

            // build peers
            List<MoneroPeer> peers = [];
            if (respMap.ContainsKey("gray_list"))
            {
                foreach (var rpcPeer in (JArray)respMap["gray_list"])
                {
                    MoneroPeer peer = ConvertRpcPeer(rpcPeer.ToObject<Dictionary<string, object>>());
                    peer.SetIsOnline(false); // gray list means offline last checked
                    peers.Add(peer);
                }
            }
            if (respMap.ContainsKey("white_list"))
            {
                foreach (var rpcPeer in  (JArray)respMap["white_list"])
                {
                    MoneroPeer peer = ConvertRpcPeer(rpcPeer.ToObject<Dictionary<string, object>>());
                    peer.SetIsOnline(true); // white list means online last checked
                    peers.Add(peer);
                }
            }
            return peers;
        }

        public override MoneroBlockHeader GetLastBlockHeader()
        {
            var respMap = _rpc!.SendJsonRequest("get_last_block_header");
            var resultMap = respMap.Result!;
            CheckResponseStatus(resultMap);
            MoneroBlockHeader header = ConvertRpcBlockHeader(((JObject)resultMap["block_header"]).ToObject<Dictionary<string, object>>());
            return header;
        }

        public override MoneroMiningStatus GetMiningStatus()
        {
            var resp = _rpc!.SendPathRequest("mining_status");
            CheckResponseStatus(resp);
            return ConvertRpcMiningStatus(resp);
        }

        public override MoneroMinerTxSum GetMinerTxSum(ulong height, ulong? numBlocks = null)
        {
            if (numBlocks == null) numBlocks = GetHeight();
            MoneroJsonRpcParams parameters = [];
            parameters.Add("height", height);
            parameters.Add("count", numBlocks);
            var respMap = _rpc!.SendJsonRequest("get_coinbase_tx_sum", parameters);
            var resultMap = respMap.Result!;
            CheckResponseStatus(resultMap);
            MoneroMinerTxSum txSum = new MoneroMinerTxSum();
            txSum.SetEmissionSum(Convert.ToUInt64(resultMap["emission_amount"]));
            txSum.SetFeeSum(Convert.ToUInt64(resultMap["fee_amount"]));
            return txSum;
        }

        public override List<MoneroOutputDistributionEntry> GetOutputDistribution(List<ulong> amounts, bool? isCumulative = null, ulong? startHeight = null, ulong? endHeight = null)
        {
            throw new NotImplementedException("MoneroDaemonRpc.GetOutputDistribution(): not implemented (response 'distribution' field is binary)");
        }

        public override List<MoneroOutputHistogramEntry> GetOutputHistogram(List<ulong>? amounts = null, int? minCount = null, int? maxCount = null, bool? isUnlocked = null, int? recentCutoff = null)
        {
            // build request params
            MoneroJsonRpcParams parameters = [];
            parameters.Add("amounts", amounts);
            parameters.Add("min_count", minCount);
            parameters.Add("max_count", maxCount);
            parameters.Add("unlocked", isUnlocked);
            parameters.Add("recent_cutoff", recentCutoff);

            // send rpc request
            var resp = _rpc!.SendJsonRequest("get_output_histogram", parameters);
            var result = resp.Result!;
            CheckResponseStatus(result);

            // build histogram entries from response
            List<MoneroOutputHistogramEntry> entries = [];
            if (!result.ContainsKey("histogram")) return entries;
            foreach (var rpcEntry in (JArray)result["histogram"])
            {
                entries.Add(ConvertRpcOutputHistogramEntry(rpcEntry.ToObject<Dictionary<string, object>>()));
            }
            return entries;
        }

        public override List<MoneroOutput> GetOutputs(List<MoneroOutput> outputs)
        {
            MoneroJsonRpcParams parameters = [];
            List<Dictionary<string, object?>> outs = [];

            foreach (var output in outputs)
            {
                Dictionary<string, object?> outParam = [];
                
                outParam.Add("amount", output.GetAmount());
                outParam.Add("index", output.GetIndex());
                
                outs.Add(outParam);
            }
            
            parameters.Add("outputs", outs);
            parameters.Add("get_tx_id", true);

            var resp = _rpc!.SendJsonRequest("get_outs", parameters);
            var result =  resp.Result!;
            
            CheckResponseStatus(result);
            if (result.ContainsKey("outs"))
            {
                return ConvertRpcOuts(((JArray)result["outs"]).ToObject<List<Dictionary<string, object>>>()!);
            }

            return [];
        }

        public override List<MoneroBan> GetPeerBans()
        {
            var resp = _rpc!.SendJsonRequest("get_bans");
            var result = resp.Result!;
            CheckResponseStatus(result);
            List<MoneroBan> bans = [];
            foreach (var rpcBan in (JArray)result["bans"])
            {
                MoneroBan ban = new MoneroBan();
                ban.SetHost((string?)rpcBan["host"]);
                ban.SetIp(Convert.ToUInt32(rpcBan["ip"]));
                ban.SetSeconds(Convert.ToUInt64(rpcBan["seconds"]));
                bans.Add(ban);
            }
            return bans;
        }

        public override List<MoneroPeer> GetPeers()
        {
            var resp = _rpc!.SendJsonRequest("get_connections");
            var result = resp.Result!;
            CheckResponseStatus(result);
            List<MoneroPeer> connections = [];
            if (!result.ContainsKey("connections")) return connections;
            foreach (var rpcConnection in (JArray)result["connections"])
            {
                connections.Add(ConvertRpcConnection(rpcConnection.ToObject<Dictionary<string, object>>()));
            }
            return connections;
        }

        public override MoneroDaemonSyncInfo GetSyncInfo()
        {
            var resp = _rpc!.SendJsonRequest("sync_info");
            var result = resp.Result!;
            CheckResponseStatus(result);
            return ConvertRpcSyncInfo(result);
        }

        public override List<string> GetTxHexes(List<string> txHashes, bool prune = false)
        {
            List<string> hexes = [];
            foreach (MoneroTx tx in GetTxs(txHashes, prune)) hexes.Add(prune ? tx.GetPrunedHex()! : tx.GetFullHex()!);
            return hexes;
        }

        public override List<MoneroTx> GetTxPool()
        {
            // send rpc request
            var resp = _rpc!.SendPathRequest("get_transaction_pool");
            CheckResponseStatus(resp);

            // build txs
            List<MoneroTx> txs = [];
            if (resp.ContainsKey("transactions"))
            {
                foreach (var rpcTx in (JArray)resp["transactions"])
                {
                    var tx = new MoneroTx();
                    txs.Add(tx);
                    tx.SetIsConfirmed(false);
                    tx.SetIsMinerTx(false);
                    tx.SetInTxPool(true);
                    tx.SetNumConfirmations(0);
                    ConvertRpcTx(rpcTx.ToObject<Dictionary<string, object>>(), tx);
                }
            }

            return txs;
        }

        public override List<string> GetTxPoolHashes()
        {
            // TODO move to binary request
            // send rpc request
            var resp = _rpc!.SendPathRequest("get_transaction_pool");
            CheckResponseStatus(resp);
            
            var txHashes = (JArray?)resp.GetValueOrDefault("tx_hashes");

            return (txHashes != null) ? txHashes.ToObject<List<string>>()! : [];
        }

        public override MoneroTxPoolStats GetTxPoolStats()
        {
            var resp = _rpc!.SendPathRequest("get_transaction_pool_stats");
            CheckResponseStatus(resp);
            return ConvertRpcTxPoolStats(((JObject)resp["pool_stats"]).ToObject<Dictionary<string, object>>());
        }

        public override List<MoneroTx> GetTxs(List<string> txHashes, bool prune = false)
        {
            // validate input
            if (txHashes.Count == 0) throw new MoneroError("Must provide an array of transaction hashes");

            // fetch transactions
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txs_hashes", txHashes);
            parameters.Add("decode_as_json", true);
            parameters.Add("prune", prune);
            var respMap = _rpc!.SendPathRequest("get_transactions", parameters);
            try
            {
                CheckResponseStatus(respMap);
            }
            catch (MoneroError e)
            {
                if (e.Message.Contains("Failed to parse hex representation of transaction hash")) throw new MoneroError("Invalid transaction hash", e.GetCode());
                throw new MoneroError(e);
            }

            List<MoneroTx> txs = [];

            if (respMap.ContainsKey("txs"))
            {
                //  interpret response
                var rpcTxs = (JArray)respMap["txs"];

                // build transaction models
                for (int i = 0; i < rpcTxs.Count; i++)
                {
                    MoneroTx tx = new MoneroTx();
                    tx.SetIsMinerTx(false);
                    txs.Add(ConvertRpcTx(rpcTxs[i].ToObject<Dictionary<string, object>>(), tx)!);
                }
            }
            
            return txs;
        }

        public override int GetUploadLimit()
        {
            return GetBandwidthLimits()[1];
        }

        public override MoneroVersion GetVersion()
        {
            var resp = _rpc!.SendJsonRequest("get_version");
            var result = resp.Result!;
            Int64 version64 = (Int64)result.GetValueOrDefault("version", 0);
            var version = Convert.ToInt32(version64.ToString());
            return new MoneroVersion(version, (bool)result["release"]);
        }

        public override bool IsTrusted()
        {
            var resp = _rpc!.SendPathRequest("get_height");
            CheckResponseStatus(resp);
            return !(bool)resp["untrusted"];
        }

        public override MoneroPruneResult PruneBlockchain(bool check)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("check", check);
            var resp = _rpc!.SendJsonRequest("prune_blockchain", parameters, 0);
            var resultMap = resp.Result!;
            CheckResponseStatus(resultMap);
            MoneroPruneResult result = new MoneroPruneResult();
            result.SetIsPruned((bool)resultMap["pruned"]);
            result.SetPruningSeed(Convert.ToInt32(resultMap["pruning_seed"]));
            return result;
        }

        public override void RelayTxsByHash(List<string> txHashes)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txids", txHashes);
            var resp = _rpc!.SendJsonRequest("relay_tx", parameters);
            CheckResponseStatus(resp.Result);
        }

        public override int ResetDownloadLimit()
        {
            return SetBandwidthLimits(-1, 0)[0];
        }

        public override int ResetUploadLimit()
        {
            return SetBandwidthLimits(0, -1)[1];
        }

        public override int SetDownloadLimit(int limit)
        {
            if (limit == -1) return ResetDownloadLimit();
            if (limit <= 0) throw new MoneroError("Download limit must be an integer greater than 0");
            return SetBandwidthLimits(limit, 0)[0];
        }

        public override void SetIncomingPeerLimit(int limit)
        {
            if (limit < 0) throw new MoneroError("Incoming peer limit must be >= 0");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("in_peers", limit);
            var resp = _rpc!.SendPathRequest("in_peers", parameters);
            CheckResponseStatus(resp);
        }

        public override void SetOutgoingPeerLimit(int limit)
        {
            if (limit < 0) throw new MoneroError("Outgoing peer limit must be >= 0");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("out_peers", limit);
            var resp = _rpc!.SendPathRequest("out_peers", parameters);
            CheckResponseStatus(resp);
        }

        public override void SetPeerBans(List<MoneroBan> bans)
        {
            var rpcBans = new List<Dictionary<string, object?>>();
            foreach (MoneroBan ban in bans) rpcBans.Add(ConvertToRpcBan(ban));
            MoneroJsonRpcParams parameters = [];
            parameters.Add("bans", rpcBans);
            var resp = _rpc!.SendJsonRequest("set_bans", parameters);
            CheckResponseStatus(resp.Result);
        }

        public override int SetUploadLimit(int limit)
        {
            if (limit == -1) return ResetUploadLimit();
            if (limit <= 0) throw new MoneroError("Upload limit must be an integer greater than 0");
            return SetBandwidthLimits(0, limit)[1];
        }

        public override void StartMining(string? address, ulong? numThreads, bool? isBackground, bool? ignoreBattery)
        {
            if (string.IsNullOrEmpty(address)) throw new MoneroError("Must provide address to mine to");
            if (numThreads == null) throw new MoneroError("Number of threads must be an integer greater than 0");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("miner_address", address);
            parameters.Add("threads_count", numThreads);
            parameters.Add("do_background_mining", isBackground);
            parameters.Add("ignore_battery", ignoreBattery);
            var resp = _rpc!.SendPathRequest("start_mining", parameters);
            CheckResponseStatus(resp);
        }

        public override void Stop()
        {
            CheckResponseStatus(_rpc!.SendPathRequest("stop_daemon"));
        }

        public override void StopMining()
        {
            var resp = _rpc!.SendPathRequest("stop_mining");
            CheckResponseStatus(resp);
        }

        public override void SubmitBlocks(List<string> blockBlobs)
        {
            if (blockBlobs.Count == 0) throw new MoneroError("Must provide an array of mined block blobs to submit");
            var resp = _rpc!.SendJsonRequest("submit_block", blockBlobs);
            CheckResponseStatus(resp.Result);
        }

        public override MoneroSubmitTxResult SubmitTxHex(string txHex, bool doNotRelay = false)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("tx_as_hex", txHex);
            parameters.Add("do_not_relay", doNotRelay);
            var resp = _rpc!.SendPathRequest("send_raw_transaction", parameters);
            MoneroSubmitTxResult submitResult = ConvertRpcSubmitTxResult(resp);

            // set isGood based on status
            try
            {
                CheckResponseStatus(resp);
                submitResult.SetIsGood(true);
            }
            catch (MoneroError e)
            {
                MoneroUtils.Log(1, e.Message);
                submitResult.SetIsGood(false);
            }
            return submitResult;
        }

        public override MoneroBlockHeader WaitForNextBlockHeader()
        {
            var syncLock = new object();
            lock (syncLock) {
                MoneroDaemonListener listener = new NextBlockListener(this, syncLock);
                AddListener(listener);
                try
                {
                    // wait for next block header
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

                return listener.GetLastBlockHeader() ?? throw new MoneroError("No block header received");
            }
        }

        #region Private Methods

        private List<int> GetBandwidthLimits()
        {
            var resp = _rpc!.SendPathRequest("get_limit");
            CheckResponseStatus(resp);
            return new List<int> { Convert.ToInt32(resp["limit_down"]), Convert.ToInt32(resp["limit_up"]) };
        }

        private List<int> SetBandwidthLimits(int? downLimit, int? upLimit)
        {
            if (downLimit == null) downLimit = 0;
            if (upLimit == null) upLimit = 0;
            MoneroJsonRpcParams parameters = [];
            parameters.Add("limit_down", downLimit);
            parameters.Add("limit_up", upLimit);
            var resp = _rpc!.SendPathRequest("set_limit", parameters);
            CheckResponseStatus(resp);
            return new List<int> { Convert.ToInt32(resp["limit_down"]), Convert.ToInt32(resp["limit_up"]) };
        }

        private List<MoneroBlock> GetMaxBlocks(ulong? startHeight = null, ulong? maxHeight = null, ulong? chunkSize = null)
        {
            if (startHeight == null) startHeight = 0;
            if (maxHeight == null) maxHeight = GetHeight() - 1;
            if (chunkSize == null) chunkSize = MaxReqSize;

            // determine end height to fetch
            ulong reqSize = 0;
            ulong endHeight = (ulong)startHeight - 1;
            while (reqSize < chunkSize && endHeight < maxHeight)
            {

                // get header of next block
                MoneroBlockHeader header = GetBlockHeaderByHeightCached(endHeight + 1, (ulong)maxHeight);

                // block cannot be bigger than max request size
                if (!(header.GetSize() <= chunkSize)) throw new MoneroError("Block exceeds maximum request size: " + header.GetSize());

                // done iterating if fetching block would exceed max request size
                if (reqSize + header.GetSize() > chunkSize) break;

                // otherwise block is included
                reqSize += (ulong)header.GetSize()!;
                endHeight++;
            }
            return endHeight >= startHeight ? GetBlocksByRange((ulong)startHeight, endHeight) : [];
        }

        private MoneroBlockHeader GetBlockHeaderByHeightCached(ulong height, ulong maxHeight)
        {
            // get header from cache
            MoneroBlockHeader? cachedHeader = _cachedHeaders.GetValueOrDefault(height);
            if (cachedHeader != null) return cachedHeader;

            // fetch and cache headers if not in cache
            ulong endHeight = Math.Min(maxHeight, height + NumHeadersPerReq - 1);  // TODO: could specify end height to cache to optimize small requests (would like to have time profiling in place though)
            List<MoneroBlockHeader> headers = GetBlockHeadersByRange(height, endHeight);
            foreach (MoneroBlockHeader header in headers)
            {
                _cachedHeaders.Add((ulong)header.GetHeight()!, header);
            }

            // return the cached header
            return _cachedHeaders[height];
        }

        #endregion

        #region Private Static Methods

        private static void CheckResponseStatus(Dictionary<string, object>? resp)
        {
            if (resp == null) throw new MoneroRpcError("Empty response from server");
            string status = (string)(resp.GetValueOrDefault("status") ?? "Empty status response from server");
            if (!"OK".Equals(status)) throw new MoneroRpcError(status);
        }

        private static MoneroTx? ConvertRpcTx(Dictionary<string, object>? rpcTx, MoneroTx? tx = null)
        {
            if (rpcTx == null) return null;
            if (tx == null) tx = new MoneroTx();

            //    System.out.println("******** BUILDING TX ***********");
            //    System.out.println(rpcTx);
            //    System.out.println(tx.toString());

            // initialize from rpc map
            MoneroBlock? block = null;
            foreach (string key in rpcTx.Keys)
            {
                object val = rpcTx[key];
                if (key.Equals("tx_hash") || key.Equals("id_hash")) tx.SetHash(GenUtils.Reconcile(tx.GetHash(), (string)val));
                else if (key.Equals("block_timestamp"))
                {
                    if (block == null) block = new MoneroBlock();
                    block.SetTimestamp(GenUtils.Reconcile(block.GetTimestamp(), Convert.ToUInt64(val)));
                }
                else if (key.Equals("block_height"))
                {
                    if (block == null) block = new MoneroBlock();
                    block.SetHeight(GenUtils.Reconcile(block.GetHeight(), Convert.ToUInt64(val)));
                }
                else if (key.Equals("last_relayed_time")) tx.SetLastRelayedTimestamp(GenUtils.Reconcile(tx.GetLastRelayedTimestamp(), Convert.ToUInt64(val)));
                else if (key.Equals("receive_time") || key.Equals("received_timestamp")) tx.SetReceivedTimestamp(GenUtils.Reconcile(tx.GetReceivedTimestamp(), Convert.ToUInt64(val)));
                else if (key.Equals("confirmations")) tx.SetNumConfirmations(GenUtils.Reconcile(tx.GetNumConfirmations(), Convert.ToUInt64(val)));
                else if (key.Equals("in_pool"))
                {
                    tx.SetIsConfirmed(GenUtils.Reconcile(tx.IsConfirmed(), !(bool)val));
                    tx.SetInTxPool(GenUtils.Reconcile(tx.InTxPool(), (bool)val));
                }
                else if (key.Equals("double_spend_seen")) tx.SetIsDoubleSpendSeen(GenUtils.Reconcile(tx.IsDoubleSpendSeen(), (bool)val));
                else if (key.Equals("version")) tx.SetVersion(GenUtils.Reconcile(tx.GetVersion(), Convert.ToUInt32(val)));
                else if (key.Equals("extra"))
                {
                    if (val is string) {
                        MoneroUtils.Log(0, "extra field as string not being assigned to byte[]: " + key + ": " + val); // TODO: how to set string to int[]? - or, extra is string which can encode byte[]
                    } else
                    {
                        List<byte> bytes = [];
                        foreach (ulong bi in (JArray)val) bytes.Add((byte)bi);
                        tx.SetExtra(GenUtils.Reconcile(tx.GetExtra(), GenUtils.ListToByteArray(bytes)));
                    }
                }
                else if (key.Equals("vin")) {
                    var rpcInputs = ((JArray)val).ToObject<List<Dictionary<string, object>>>()!;
                    if (rpcInputs.Count != 1 || !rpcInputs[0].ContainsKey("gen")) {  // ignore miner input TODO: why? probably needs re-enabled
                        List<MoneroOutput> inputs = [];
                        foreach (Dictionary<string, object> rpcInput in rpcInputs) inputs.Add(ConvertRpcOutput(rpcInput, tx));
                        tx.SetInputs(inputs);
                    }
                }
                else if (key.Equals("vout"))
                {
                    List<Dictionary<string, object>> rpcOutputs = ((JArray)val).ToObject<List<Dictionary<string, object>>>()!;
                    List<MoneroOutput> outputs = [];
                    foreach (Dictionary<string, object> rpcOutput in rpcOutputs) outputs.Add(ConvertRpcOutput(rpcOutput, tx));
                    tx.SetOutputs(outputs);
                }
                else if (key.Equals("rct_signatures"))
                {
                    Dictionary<string, object> rctSignaturesMap = ((JObject)val).ToObject<Dictionary<string, object>>()!;
                    tx.SetRctSignatures(GenUtils.Reconcile(tx.GetRctSignatures(), rctSignaturesMap));
                    if (rctSignaturesMap.ContainsKey("txnFee")) tx.SetFee(GenUtils.Reconcile(tx.GetFee(), Convert.ToUInt64(rctSignaturesMap["txnFee"])));
                }
                else if (key.Equals("rctsig_prunable")) tx.SetRctSigPrunable(GenUtils.Reconcile(tx.GetRctSigPrunable(), val));
                else if (key.Equals("unlock_time")) tx.SetUnlockTime(GenUtils.Reconcile(tx.GetUnlockTime(), Convert.ToUInt64(val)));
                else if (key.Equals("as_json") || key.Equals("tx_json")) { }  // handled last so tx is as initialized as possible
                else if (key.Equals("as_hex") || key.Equals("tx_blob")) tx.SetFullHex(GenUtils.Reconcile(tx.GetFullHex(), "".Equals(val) ? null : (string)val));
                else if (key.Equals("blob_size")) tx.SetSize(GenUtils.Reconcile(tx.GetSize(), Convert.ToUInt64(val)));
                else if (key.Equals("weight")) tx.SetWeight(GenUtils.Reconcile(tx.GetWeight(), Convert.ToUInt64(val)));
                else if (key.Equals("fee")) tx.SetFee(GenUtils.Reconcile(tx.GetFee(), Convert.ToUInt64(val)));
                else if (key.Equals("relayed")) tx.SetIsRelayed(GenUtils.Reconcile(tx.IsRelayed(), (bool)val));
                else if (key.Equals("output_indices"))
                {
                    List<ulong> indices = [];
                    foreach (var bi in (JArray)val) indices.Add(Convert.ToUInt64(bi));
                    tx.SetOutputIndices(GenUtils.Reconcile(tx.GetOutputIndices(), indices));
                }
                else if (key.Equals("do_not_relay")) tx.SetRelay(GenUtils.Reconcile(tx.GetRelay(), !(bool)val));
                else if (key.Equals("kept_by_block")) tx.SetIsKeptByBlock(GenUtils.Reconcile(tx.IsKeptByBlock(), (bool)val));
                else if (key.Equals("signatures")) tx.SetSignatures(GenUtils.Reconcile(tx.GetSignatures(), (List<string>)val));
                else if (key.Equals("last_failed_height"))
                {
                    ulong lastFailedHeight = Convert.ToUInt64(val);
                    if (lastFailedHeight == 0) tx.SetIsFailed(GenUtils.Reconcile(tx.IsFailed(), false));
                    else
                    {
                        tx.SetIsFailed(GenUtils.Reconcile(tx.IsFailed(), true));
                        tx.SetLastFailedHeight(GenUtils.Reconcile(tx.GetLastFailedHeight(), lastFailedHeight));
                    }
                }
                else if (key.Equals("last_failed_id_hash"))
                {
                    if (DefaultId.Equals(val)) tx.SetIsFailed(GenUtils.Reconcile(tx.IsFailed(), false));
                    else
                    {
                        tx.SetIsFailed(GenUtils.Reconcile(tx.IsFailed(), true));
                        tx.SetLastFailedHash(GenUtils.Reconcile(tx.GetLastFailedHash(), (string)val));
                    }
                }
                else if (key.Equals("max_used_block_height")) tx.SetMaxUsedBlockHeight(GenUtils.Reconcile(tx.GetMaxUsedBlockHeight(), Convert.ToUInt64(val)));
                else if (key.Equals("max_used_block_id_hash")) tx.SetMaxUsedBlockHash(GenUtils.Reconcile(tx.GetMaxUsedBlockHash(), (string)val));
                else if (key.Equals("prunable_hash")) tx.SetPrunableHash(GenUtils.Reconcile(tx.GetPrunableHash(), "".Equals(val) ? null : (string)val));
                else if (key.Equals("prunable_as_hex")) tx.SetPrunableHex(GenUtils.Reconcile(tx.GetPrunableHex(), "".Equals(val) ? null : (string)val));
                else if (key.Equals("pruned_as_hex")) tx.SetPrunedHex(GenUtils.Reconcile(tx.GetPrunedHex(), "".Equals(val) ? null : (string)val));
                else MoneroUtils.Log(0, "ignoring unexpected field in rpc tx: " + key + ": " + val);
            }
    
            // link block and tx
            if (block != null) tx.SetBlock(block.SetTxs([tx]));

            // TODO monerod: unconfirmed txs misreport block height and timestamp
            if (tx.GetBlock() != null && tx.GetBlock()!.GetHeight() != null && tx.GetBlock()!.GetHeight() == tx.GetBlock()!.GetTimestamp())
            {
                tx.SetBlock(null);
                tx.SetIsConfirmed(false);
            }

            // initialize remaining known fields
            if (tx.IsConfirmed() == true)
            {
                tx.SetRelay(GenUtils.Reconcile(tx.GetRelay(), true));
                tx.SetIsRelayed(GenUtils.Reconcile(tx.IsRelayed(), true));
                tx.SetIsFailed(GenUtils.Reconcile(tx.IsFailed(), false));
            }
            else
            {
                tx.SetNumConfirmations(0);
            }
            if (tx.IsFailed() == null) tx.SetIsFailed(false);
            if (tx.GetOutputIndices() != null && tx.GetOutputs() != null)
            {
                if (tx.GetOutputIndices()!.Count != tx.GetOutputs()!.Count) throw new MoneroError("Output and indices mismatch");
                for (int i = 0; i < tx.GetOutputs()!.Count; i++)
                {
                    tx.GetOutputs()![i].SetIndex(tx.GetOutputIndices()![i]);  // transfer output indices to outputs
                }
            }
            if (rpcTx.ContainsKey("as_json") && !"".Equals(rpcTx["as_json"])) ConvertRpcTx(JsonConvert.DeserializeObject<Dictionary<string, object>>((string)rpcTx["as_json"]), tx);
            if (rpcTx.ContainsKey("tx_json") && !"".Equals(rpcTx["tx_json"])) ConvertRpcTx(JsonConvert.DeserializeObject<Dictionary<string, object>>((string)rpcTx["tx_json"]), tx);
            if (tx.IsRelayed() != true) tx.SetLastRelayedTimestamp(null);  // TODO monerod: returns last_relayed_timestamp despite relayed: false, self inconsistent

            // return built transaction
            return tx;
        }

        private static MoneroBlockTemplate ConvertRpcBlockTemplate(Dictionary<string, object> rpcTemplate)
        {
            var template = new MoneroBlockTemplate();
            foreach (string key in rpcTemplate.Keys)
            {
                object val = rpcTemplate[key];
                if (key.Equals("blockhashing_blob")) template.SetBlockHashingBlob((string)val);
                else if (key.Equals("blocktemplate_blob")) template.SetBlockTemplateBlob((string)val);
                else if (key.Equals("expected_reward")) template.SetExpectedReward(Convert.ToUInt64(val));
                else if (key.Equals("difficulty")) { }  // handled by wide_difficulty
                else if (key.Equals("difficulty_top64")) { }  // handled by wide_difficulty
                else if (key.Equals("wide_difficulty")) template.SetDifficulty(GenUtils.Reconcile(template.GetDifficulty(), PrefixedHexToUulong((string)val)));
                else if (key.Equals("height")) template.SetHeight(Convert.ToUInt64(val));
                else if (key.Equals("prev_hash")) template.SetPrevHash((string)val);
                else if (key.Equals("reserved_offset")) template.SetReservedOffset(Convert.ToUInt64(val));
                else if (key.Equals("status")) { }  // handled elsewhere
                else if (key.Equals("untrusted")) { }  // handled elsewhere
                else if (key.Equals("seed_height")) template.SetSeedHeight(Convert.ToUInt64(val));
                else if (key.Equals("seed_hash")) template.SetSeedHash((string)val);
                else if (key.Equals("next_seed_hash")) template.SetNextSeedHash((string)val);
                else MoneroUtils.Log(0, "ignoring unexpected field in block template: " + key + ": " + val);
            }
            if ("".Equals(template.GetNextSeedHash())) template.SetNextSeedHash(null);
            return template;
        }

        private static MoneroBlockHeader ConvertRpcBlockHeader(Dictionary<string, object>? rpcHeader, MoneroBlockHeader? header = null)
        {
            if (header == null) header = new MoneroBlockHeader();
            if (rpcHeader == null) throw new MoneroError("Header is null");
            foreach (string key in rpcHeader.Keys)
            {
                object val = rpcHeader[key];
                if (key.Equals("block_size")) header.SetSize(GenUtils.Reconcile(header.GetSize(), Convert.ToUInt64(val)));
                else if (key.Equals("depth")) header.SetDepth(GenUtils.Reconcile(header.GetDepth(), Convert.ToUInt64(val)));
                else if (key.Equals("difficulty")) { }  // handled by wide_difficulty
                else if (key.Equals("cumulative_difficulty")) { } // handled by wide_cumulative_difficulty
                else if (key.Equals("difficulty_top64")) { }  // handled by wide_difficulty
                else if (key.Equals("cumulative_difficulty_top64")) { } // handled by wide_cumulative_difficulty
                else if (key.Equals("wide_difficulty")) header.SetDifficulty(GenUtils.Reconcile(header.GetDifficulty(), PrefixedHexToUulong((string)val)));
                else if (key.Equals("wide_cumulative_difficulty")) header.SetCumulativeDifficulty(GenUtils.Reconcile(header.GetCumulativeDifficulty(), PrefixedHexToUulong((string)val)));
                else if (key.Equals("hash")) header.SetHash(GenUtils.Reconcile(header.GetHash(), (string)val));
                else if (key.Equals("height")) header.SetHeight(GenUtils.Reconcile(header.GetHeight(), Convert.ToUInt64(val)));
                else if (key.Equals("major_version")) header.SetMajorVersion(GenUtils.Reconcile(header.GetMajorVersion(), Convert.ToUInt32(val)));
                else if (key.Equals("minor_version")) header.SetMinorVersion(GenUtils.Reconcile(header.GetMinorVersion(), Convert.ToUInt32(val)));
                else if (key.Equals("nonce")) header.SetNonce(GenUtils.Reconcile(header.GetNonce(), Convert.ToUInt64(val)));
                else if (key.Equals("num_txes")) header.SetNumTxs(GenUtils.Reconcile(header.GetNumTxs(), Convert.ToUInt32(val)));
                else if (key.Equals("orphan_status")) header.SetOrphanStatus(GenUtils.Reconcile(header.GetOrphanStatus(), (bool)val));
                else if (key.Equals("prev_hash") || key.Equals("prev_id")) header.SetPrevHash(GenUtils.Reconcile(header.GetPrevHash(), (string)val));
                else if (key.Equals("reward")) header.SetReward(GenUtils.Reconcile(header.GetReward(), Convert.ToUInt64(val)));
                else if (key.Equals("timestamp")) header.SetTimestamp(GenUtils.Reconcile(header.GetTimestamp(), Convert.ToUInt64(val)));
                else if (key.Equals("block_weight")) header.SetWeight(GenUtils.Reconcile(header.GetWeight(), Convert.ToUInt64(val)));
                else if (key.Equals("long_term_weight")) header.SetLongTermWeight(GenUtils.Reconcile(header.GetLongTermWeight(), Convert.ToUInt64(val)));
                else if (key.Equals("pow_hash")) header.SetPowHash(GenUtils.Reconcile(header.GetPowHash(), "".Equals(val) ? null : (string)val));
                else if (key.Equals("tx_hashes")) { }  // used in block model, not header model
                else if (key.Equals("miner_tx")) { }   // used in block model, not header model
                else if (key.Equals("miner_tx_hash")) header.SetMinerTxHash((string)val);
                else MoneroUtils.Log(0, "ignoring unexpected block header field: '" + key + "': " + val);
            }
            return header;
        }

        private static MoneroBlock ConvertRpcBlock(Dictionary<string, object> rpcBlock)
        {
            // build block
            var block = new MoneroBlock();
            ConvertRpcBlockHeader(rpcBlock.ContainsKey("block_header") ? ((JObject)rpcBlock["block_header"]).ToObject<Dictionary<string, object>>() : rpcBlock, block);
            block.SetHex((string)rpcBlock["blob"]);
            block.SetTxHashes(rpcBlock.ContainsKey("tx_hashes") ? ((JArray)rpcBlock["tx_hashes"]).ToObject<List<string>>() : new List<string>());

            // build miner tx
            Dictionary<string, object>? rpcMinerTx;
            if (rpcBlock.ContainsKey("json"))
            {
                rpcMinerTx = ((JObject)JsonConvert.DeserializeObject<Dictionary<string, object>>((string)rpcBlock["json"])!["miner_tx"]).ToObject<Dictionary<string, object>>();
            }
            else if (rpcBlock.ContainsKey("miner_tx"))
            {
                rpcMinerTx = ((JObject)rpcBlock["miner_tx"]).ToObject<Dictionary<string, object>>()!;
            }
            else
            {
                throw new MoneroError("Block does not contain miner tx");
            }
            
            MoneroTx minerTx = new MoneroTx().SetIsConfirmed(true).SetInTxPool(false).SetIsMinerTx(true);
            ConvertRpcTx(rpcMinerTx, minerTx);
            block.SetMinerTx(minerTx);

            return block;
        }

        private static MoneroOutput ConvertRpcOutput(Dictionary<string, object> rpcOutput, MoneroTx? tx)
        {
            var output = new MoneroOutput();
            output.SetTx(tx);
            foreach (string key in rpcOutput.Keys)
            {
                object val = rpcOutput[key];
                if (key.Equals("gen")) throw new MoneroError("Output with 'gen' from daemon rpc is miner tx which we ignore (i.e. each miner input is null)");
                else if (key.Equals("key"))
                {
                    var rpcKey = ((JObject)val).ToObject<Dictionary<string, object>>()!;
                    output.SetAmount(GenUtils.Reconcile(output.GetAmount(), Convert.ToUInt64(rpcKey["amount"])));
                    output.SetKeyImage(GenUtils.Reconcile(output.GetKeyImage(), new MoneroKeyImage((string)rpcKey["k_image"])));
                    List<ulong> ringOutputIndices = [];
                    foreach (ulong bi in (JArray)rpcKey["key_offsets"]) ringOutputIndices.Add(Convert.ToUInt64(bi));
                    output.SetRingOutputIndices(GenUtils.Reconcile(output.GetRingOutputIndices(), ringOutputIndices));
                }
                else if (key.Equals("amount")) output.SetAmount(GenUtils.Reconcile(output.GetAmount(), Convert.ToUInt64(val)));
                else if (key.Equals("target"))
                {
                    var valMap = ((JObject)val).ToObject<Dictionary<string, object>>()!;
                    string pubKey = valMap.ContainsKey("key") ? (string)valMap["key"] : (((JObject)valMap["tagged_key"])).ToObject<Dictionary<string, string>>()!["key"]; // TODO (monerod): rpc json uses {tagged_key={key=...}}, binary blocks use {key=...}
                    output.SetStealthPublicKey(GenUtils.Reconcile(output.GetStealthPublicKey(), pubKey));
                }
                else MoneroUtils.Log(0, "ignoring unexpected field output: " + key + ": " + val);
            }
            return output;
        }
        
        private static List<MoneroOutput> ConvertRpcOuts(List<Dictionary<string, object>> rpcOutputs)
        {
            List<MoneroOutput> outputs = [];
            Dictionary<ulong, MoneroBlock> blocksMap = [];
            Dictionary<string, MoneroTx> txsMap = [];
            
            foreach (var rpcOutput in rpcOutputs)
            {
                var output = new MoneroOutput();
                MoneroBlock? block = null;
                MoneroTx? tx = null;
                
                foreach (string key in rpcOutput.Keys)
                {
                    object val = rpcOutput[key];
                    if (key.Equals("key"))
                    {
                        output.SetStealthPublicKey((string?)val);
                    }
                    else if (key.Equals("height"))
                    {
                        ulong height = (ulong)val;
                        block = blocksMap.GetValueOrDefault(height);

                        if (block == null)
                        {
                            block = new MoneroBlock();
                            block.SetHeight(height);
                            block.SetTxs([]);
                            blocksMap.Add(height, block);
                        }
                    }
                    else if (key.Equals("txid"))
                    {
                        string txHash = (string)val;
                        tx = txsMap.GetValueOrDefault(txHash);

                        if (tx == null)
                        {
                            tx = new MoneroTx();
                            output.SetTx(tx);
                            tx.SetHash(txHash);
                            tx.SetOutputs([output]);
                            txsMap.Add(txHash, tx);
                        }
                        else
                        {
                            tx.GetOutputs()!.Add(output);
                        }
                    }
                    else if (key.Equals("unlocked"))
                    {
                        // TODO move MoneroOutputWallet.IsLocked to base class?
                    }
                    else MoneroUtils.Log(0, "ignoring unexpected field output: " + key + ": " + val);
                }

                if (tx != null)
                {
                    tx.SetBlock(block);

                    if (block != null && block.GetTxs()!.Find(x => x.GetHash() == tx.GetHash()) == null)
                    {
                        block.AddTx(tx);
                    }
                }
            }

            return outputs;
        }

        private static MoneroDaemonUpdateCheckResult ConvertRpcUpdateCheckResult(Dictionary<string, object> rpcResult)
        {
            var result = new MoneroDaemonUpdateCheckResult();
            foreach (string key in rpcResult.Keys)
            {
                object val = rpcResult[key];
                if (key.Equals("auto_uri")) result.SetAutoUri((string)val);
                else if (key.Equals("hash")) result.SetHash((string)val);
                else if (key.Equals("path")) { } // handled elsewhere
                else if (key.Equals("status")) { } // handled elsewhere
                else if (key.Equals("update")) result.SetIsUpdateAvailable((bool)val);
                else if (key.Equals("user_uri")) result.SetUserUri((string)val);
                else if (key.Equals("version")) result.SetVersion((string)val);
                else if (key.Equals("untrusted")) { }  // handled elsewhere
                else MoneroUtils.Log(0, "ignoring unexpected field in rpc check update result: " + key + ": " + val);
            }
            if ("".Equals(result.GetAutoUri())) result.SetAutoUri(null);
            if ("".Equals(result.GetUserUri())) result.SetUserUri(null);
            if ("".Equals(result.GetVersion())) result.SetVersion(null);
            if ("".Equals(result.GetHash())) result.SetHash(null);
            return result;
        }

        private static MoneroTxPoolStats ConvertRpcTxPoolStats(Dictionary<string, object>? rpcStats)
        {
            if (rpcStats == null) throw new MoneroError("Could not parse stats response");
            var stats = new MoneroTxPoolStats();
            foreach (string key in rpcStats.Keys)
            {
                object val = rpcStats[key];
                if (key.Equals("bytes_max")) stats.SetBytesMax(Convert.ToUInt64(val));
                else if (key.Equals("bytes_med")) stats.SetBytesMed(Convert.ToUInt64(val));
                else if (key.Equals("bytes_min")) stats.SetBytesMin(Convert.ToUInt64(val));
                else if (key.Equals("bytes_total")) stats.SetBytesTotal(Convert.ToUInt64(val));
                else if (key.Equals("histo_98pc")) stats.SetHisto98Pc(Convert.ToUInt64(val));
                else if (key.Equals("num_10m")) stats.SetNum10m(Convert.ToInt32(val));
                else if (key.Equals("num_double_spends")) stats.SetNumDoubleSpends(Convert.ToInt32(val));
                else if (key.Equals("num_failing")) stats.SetNumFailing(Convert.ToInt32(val));
                else if (key.Equals("num_not_relayed")) stats.SetNumNotRelayed(Convert.ToInt32(val));
                else if (key.Equals("oldest")) stats.SetOldestTimestamp(Convert.ToUInt64(val));
                else if (key.Equals("txs_total")) stats.SetNumTxs(Convert.ToInt32(val));
                else if (key.Equals("fee_total")) stats.SetFeeTotal(Convert.ToUInt64(val));
                else if (key.Equals("histo"))
                {
                    stats.SetHisto([]);
                    //foreach (var elem in (List<Dictionary<string, int>>)val)
                    //{
                        //stats.GetHisto().Add(elem["bytes"], elem["txs"]);
                    //}
                }
                else MoneroUtils.Log(0, "ignoring unexpected field in tx pool stats: '" + key + "': " + val);
            }

            // uninitialize some stats if not applicable
            if (stats.GetHisto98Pc() == 0) stats.SetHisto98Pc(null);
            if (stats.GetNumTxs() == 0)
            {
                stats.SetBytesMin(null);
                stats.SetBytesMed(null);
                stats.SetBytesMax(null);
                stats.SetHisto98Pc(null);
                stats.SetOldestTimestamp(null);
            }
            return stats;
        }

        private static MoneroDaemonUpdateDownloadResult ConvertRpcUpdateDownloadResult(Dictionary<string, object> rpcResult)
        {
            var result = new MoneroDaemonUpdateDownloadResult(ConvertRpcUpdateCheckResult(rpcResult));
            result.SetDownloadPath((string)rpcResult["path"]);
            if ("".Equals(result.GetDownloadPath())) result.SetDownloadPath(null);
            return result;
        }

        private static MoneroPeer ConvertRpcPeer(Dictionary<string, object>? rpcPeer)
        {
            if (rpcPeer == null) throw new MoneroError("Cannot Convert null rpc peer");
            MoneroPeer peer = new MoneroPeer();
            foreach (string key in rpcPeer.Keys)
            {
                object val = rpcPeer[key];
                if (key.Equals("host")) peer.SetHost((string)val);
                else if (key.Equals("id")) peer.SetId("" + val);  // TODO monero-wallet-rpc: peer id is big integer but string in `get_connections`
                else if (key.Equals("ip")) { } // host used instead which is consistently a string
                else if (key.Equals("last_seen")) peer.SetLastSeenTimestamp(Convert.ToUInt64(val));
                else if (key.Equals("port")) peer.SetPort(Convert.ToInt32(val));
                else if (key.Equals("rpc_port")) peer.SetRpcPort(Convert.ToInt32(val));
                else if (key.Equals("pruning_seed")) peer.SetPruningSeed(Convert.ToInt32(val));
                else if (key.Equals("rpc_credits_per_hash")) peer.SetRpcCreditsPerHash(Convert.ToUInt64(val));
                else MoneroUtils.Log(0, "ignoring unexpected field in rpc peer: " + key + ": " + val);
            }
            return peer;
        }

        private static MoneroSubmitTxResult ConvertRpcSubmitTxResult(Dictionary<string, object>? rpcResult)
        {
            if (rpcResult == null) throw new MoneroError("Cannot Convert null tx result");
            var result = new MoneroSubmitTxResult();
            foreach (string key in rpcResult.Keys)
            {
                object val = rpcResult[key];
                if (key.Equals("double_spend")) result.SetIsDoubleSpend((bool)val);
                else if (key.Equals("fee_too_low")) result.SetIsFeeTooLow((bool)val);
                else if (key.Equals("invalid_input")) result.SetHasInvalidInput((bool)val);
                else if (key.Equals("invalid_output")) result.SetHasInvalidOutput((bool)val);
                else if (key.Equals("too_few_outputs")) result.SetHasTooFewOutputs((bool)val);
                else if (key.Equals("low_mixin")) result.SetIsMixinTooLow((bool)val);
                else if (key.Equals("not_relayed")) result.SetIsRelayed((bool)val != true);
                else if (key.Equals("overspend")) result.SetIsOverspend((bool)val);
                else if (key.Equals("reason")) result.SetReason("".Equals(val) ? null : (string)val);
                else if (key.Equals("too_big")) result.SetIsTooBig((bool)val);
                else if (key.Equals("sanity_check_failed")) result.SetSanityCheckFailed((bool)val);
                else if (key.Equals("credits")) result.SetCredits(Convert.ToUInt64(val));
                else if (key.Equals("status") || key.Equals("untrusted")) { }  // handled elsewhere
                else if (key.Equals("top_hash")) result.SetTopBlockHash("".Equals(val) ? null : (string)val);
                else if (key.Equals("tx_extra_too_big")) result.SetIsTxExtraTooBig((bool)val);
                else if (key.Equals("nonzero_unlock_time")) result.SetIsNonzeroUnlockTime((bool)val);
                else MoneroUtils.Log(0, "ignoring unexpected field in submit tx hex result: " + key + ": " + val);
            }
            return result;
        }

        private static MoneroPeer ConvertRpcConnection(Dictionary<string, object>? rpcConnection)
        {
            if (rpcConnection == null) throw new MoneroRpcError("Cannot parse peer, empty response from server");
            MoneroPeer peer = new MoneroPeer();
            peer.SetIsOnline(true);
            foreach (string key in rpcConnection.Keys)
            {
                object val = rpcConnection[key];
                if (key.Equals("address")) peer.SetAddress((string)val);
                else if (key.Equals("avg_download")) peer.SetAvgDownload(Convert.ToUInt64(val));
                else if (key.Equals("avg_upload")) peer.SetAvgUpload(Convert.ToUInt64(val));
                else if (key.Equals("connection_id")) peer.SetHash((string)val);
                else if (key.Equals("current_download")) peer.SetCurrentDownload(Convert.ToUInt64(val));
                else if (key.Equals("current_upload")) peer.SetCurrentUpload(Convert.ToUInt64(val));
                else if (key.Equals("height")) peer.SetHeight(Convert.ToUInt64(val));
                else if (key.Equals("host")) peer.SetHost((string)val);
                else if (key.Equals("ip")) { } // host used instead which is consistently a string
                else if (key.Equals("incoming")) peer.SetIsIncoming((bool)val);
                else if (key.Equals("live_time")) peer.SetLiveTime(Convert.ToUInt64(val));
                else if (key.Equals("local_ip")) peer.SetIsLocalIp((bool)val);
                else if (key.Equals("localhost")) peer.SetIsLocalHost((bool)val);
                else if (key.Equals("peer_id")) peer.SetId((string)val);
                else if (key.Equals("port")) peer.SetPort(Convert.ToInt32(val));
                else if (key.Equals("rpc_port")) peer.SetRpcPort(Convert.ToInt32(val));
                else if (key.Equals("recv_count")) peer.SetNumReceives(Convert.ToInt32(val));
                else if (key.Equals("recv_idle_time")) peer.SetReceiveIdleTime(Convert.ToUInt64(val));
                else if (key.Equals("send_count")) peer.SetNumSends(Convert.ToInt32(val));
                else if (key.Equals("send_idle_time")) peer.SetSendIdleTime(Convert.ToUInt64(val));
                else if (key.Equals("state")) peer.SetState((string)val);
                else if (key.Equals("support_flags")) peer.SetNumSupportFlags(Convert.ToInt32(val));
                else if (key.Equals("pruning_seed")) peer.SetPruningSeed(Convert.ToInt32(val));
                else if (key.Equals("rpc_credits_per_hash")) peer.SetRpcCreditsPerHash(Convert.ToUInt64(val));
                else if (key.Equals("address_type"))
                {
                    int rpcType = Convert.ToInt32(val);
                    if (rpcType == 0) peer.SetType(MoneroConnectionType.Invalid);
                    else if (rpcType == 1) peer.SetType(MoneroConnectionType.Ipv4);
                    else if (rpcType == 2) peer.SetType(MoneroConnectionType.Ipv6);
                    else if (rpcType == 3) peer.SetType(MoneroConnectionType.Tor);
                    else if (rpcType == 4) peer.SetType(MoneroConnectionType.I2P);
                    else throw new MoneroError("Invalid RPC peer type, expected 0-4: " + rpcType);
                }
                else MoneroUtils.Log(0, "ignoring unexpected field in peer: " + key + ": " + val);
            }
            return peer;
        }

        private static MoneroOutputHistogramEntry ConvertRpcOutputHistogramEntry(Dictionary<string, object>? rpcEntry)
        {
            if (rpcEntry == null)
            {
                throw new MoneroRpcError("Cannot parse output histogram entry, empty response from server");
            }
            
            var entry = new MoneroOutputHistogramEntry();
            foreach (string key in rpcEntry.Keys)
            {
                object val = rpcEntry[key];
                if (key.Equals("amount")) entry.SetAmount(Convert.ToUInt64(val));
                else if (key.Equals("total_instances")) entry.SetNumInstances(Convert.ToUInt64(val));
                else if (key.Equals("unlocked_instances")) entry.SetNumUnlockedInstances(Convert.ToUInt64(val));
                else if (key.Equals("recent_instances")) entry.SetNumRecentInstances(Convert.ToUInt64(val));
                else MoneroUtils.Log(0, "ignoring unexpected field in output histogram: " + key + ": " + val);
            }
            return entry;
        }

        private static MoneroDaemonInfo? ConvertRpcInfo(Dictionary<string, object>? rpcInfo)
        {
            if (rpcInfo == null) return null;
            MoneroDaemonInfo info = new MoneroDaemonInfo();
            foreach (string key in rpcInfo.Keys)
            {
                object val = rpcInfo[key];
                if (key.Equals("version")) info.SetVersion((string)val);
                else if (key.Equals("alt_blocks_count")) info.SetNumAltBlocks(Convert.ToUInt64(val));
                else if (key.Equals("block_size_limit")) info.SetBlockSizeLimit(Convert.ToUInt64(val));
                else if (key.Equals("block_size_median")) info.SetBlockSizeMedian(Convert.ToUInt64(val));
                else if (key.Equals("block_weight_limit")) info.SetBlockWeightLimit(Convert.ToUInt64(val));
                else if (key.Equals("block_weight_median")) info.SetBlockWeightMedian(Convert.ToUInt64(val));
                else if (key.Equals("bootstrap_daemon_address")) { if (((string)val).Length > 0) info.SetBootstrapDaemonAddress((string)val); }
                else if (key.Equals("difficulty")) { }  // handled by wide_difficulty
                else if (key.Equals("cumulative_difficulty")) { } // handled by wide_cumulative_difficulty
                else if (key.Equals("difficulty_top64")) { }  // handled by wide_difficulty
                else if (key.Equals("cumulative_difficulty_top64")) { } // handled by wide_cumulative_difficulty
                else if (key.Equals("wide_difficulty")) info.SetDifficulty(GenUtils.Reconcile(info.GetDifficulty(), PrefixedHexToUulong((string)val)));
                else if (key.Equals("wide_cumulative_difficulty")) info.SetCumulativeDifficulty(GenUtils.Reconcile(info.GetCumulativeDifficulty(), PrefixedHexToUulong((string)val)));
                else if (key.Equals("free_space")) info.SetFreeSpace(Convert.ToUInt64(val.ToString()));
                else if (key.Equals("database_size")) info.SetDatabaseSize(Convert.ToUInt64(val));
                else if (key.Equals("grey_peerlist_size")) info.SetNumOfflinePeers(Convert.ToUInt32(val));
                else if (key.Equals("height")) info.SetHeight(Convert.ToUInt64(val));
                else if (key.Equals("height_without_bootstrap")) info.SetHeightWithoutBootstrap(Convert.ToUInt64(val));
                else if (key.Equals("incoming_connections_count")) info.SetNumIncomingConnections(Convert.ToUInt32(val));
                else if (key.Equals("offline")) info.SetIsOffline((bool)val);
                else if (key.Equals("outgoing_connections_count")) info.SetNumOutgoingConnections(Convert.ToUInt32(val));
                else if (key.Equals("rpc_connections_count")) info.SetNumRpcConnections(Convert.ToUInt32(val));
                else if (key.Equals("start_time")) info.SetStartTimestamp(Convert.ToUInt64(val));
                else if (key.Equals("adjusted_time")) info.SetAdjustedTimestamp(Convert.ToUInt64(val));
                else if (key.Equals("status")) { }  // handled elsewhere
                else if (key.Equals("target")) info.SetTarget(Convert.ToUInt64(val));
                else if (key.Equals("target_height")) info.SetTargetHeight(Convert.ToUInt64(val));
                else if (key.Equals("tx_count")) info.SetNumTxs(Convert.ToUInt32(val));
                else if (key.Equals("tx_pool_size")) info.SetNumTxsPool(Convert.ToUInt32(val));
                else if (key.Equals("untrusted")) { } // handled elsewhere
                else if (key.Equals("was_bootstrap_ever_used")) info.SetWasBootstrapEverUsed((bool)val);
                else if (key.Equals("white_peerlist_size")) info.SetNumOnlinePeers(Convert.ToUInt32(val));
                else if (key.Equals("update_available")) info.SetUpdateAvailable((bool)val);
                else if (key.Equals("nettype")) info.SetNetworkType(GenUtils.Reconcile(info.GetNetworkType(), MoneroNetwork.Parse((string)val)));
                else if (key.Equals("mainnet")) { if ((bool)val) info.SetNetworkType(GenUtils.Reconcile(info.GetNetworkType(), MoneroNetworkType.Mainnet)); }
                else if (key.Equals("testnet")) { if ((bool)val) info.SetNetworkType(GenUtils.Reconcile(info.GetNetworkType(), MoneroNetworkType.Testnet)); }
                else if (key.Equals("stagenet")) { if ((bool)val) info.SetNetworkType(GenUtils.Reconcile(info.GetNetworkType(), MoneroNetworkType.Stagenet)); }
                else if (key.Equals("credits")) info.SetCredits(Convert.ToUInt64(val));
                else if (key.Equals("top_block_hash") || key.Equals("top_hash")) info.SetTopBlockHash(GenUtils.Reconcile(info.GetTopBlockHash(), "".Equals(val) ? null : (string)val));  // TODO monero-wallet-rpc: daemon info top_hash is redundant with top_block_hash, only returned if pay-for-service enabled
                else if (key.Equals("busy_syncing")) info.SetIsBusySyncing((bool)val);
                else if (key.Equals("synchronized")) info.SetIsSynchronized((bool)val);
                else if (key.Equals("restricted")) info.SetIsRestricted((bool)val);
                else MoneroUtils.Log(0, "Ignoring unexpected info field: " + key + ": " + val);
            }
            return info;
        }

        private static MoneroDaemonSyncInfo ConvertRpcSyncInfo(Dictionary<string, object> rpcSyncInfo)
        {
            var syncInfo = new MoneroDaemonSyncInfo();
            foreach (string key in rpcSyncInfo.Keys)
            {
                object val = rpcSyncInfo[key];
                if (key.Equals("height")) syncInfo.SetHeight(Convert.ToUInt64(val));
                else if (key.Equals("peers"))
                {
                    syncInfo.SetPeers([]);
                    var rpcConnections = ((JArray)val).ToObject<List<Dictionary<string, object>>>()!;
                    foreach (var rpcConnection in rpcConnections)
                    {
                        syncInfo.GetPeers()!.Add(ConvertRpcConnection(((JObject)rpcConnection["info"]).ToObject<Dictionary<string, object>> ()));
                    }
                }
                else if (key.Equals("spans"))
                {
                    syncInfo.SetSpans([]);
                    var rpcSpans = ((JArray)val).ToObject<List<Dictionary<string, object>>>()!;
                    foreach (var rpcSpan in rpcSpans)
                    {
                        syncInfo.GetSpans()!.Add(ConvertRpcConnectionSpan(rpcSpan));
                    }
                }
                else if (key.Equals("status")) { }   // handled elsewhere
                else if (key.Equals("target_height")) syncInfo.SetTargetHeight(Convert.ToUInt64(val));
                else if (key.Equals("next_needed_pruning_seed")) syncInfo.SetNextNeededPruningSeed(Convert.ToUInt32(val));
                else if (key.Equals("overview"))
                {  // this returns [] without pruning
                    try
                    {
                        List<object>? overview = JsonConvert.DeserializeObject<List<object>>((string)val);
                        if (overview != null && overview.Count > 0) MoneroUtils.Log(0, "ignoring non-empty 'overview' field (not implemented): " + overview); // TODO
                    }
                    catch (Exception e)
                    {
                        if (e.StackTrace != null) MoneroUtils.Log(1, e.StackTrace);
                        MoneroUtils.Log(0, "Failed to parse 'overview' field: " + val);
                    }
                }
                else if (key.Equals("credits")) syncInfo.SetCredits(Convert.ToUInt64(val));
                else if (key.Equals("top_hash")) syncInfo.SetTopBlockHash("".Equals(val) ? null : (string)val);
                else if (key.Equals("untrusted")) { }  // handled elsewhere
                else MoneroUtils.Log(0, "ignoring unexpected field in sync info: " + key + ": " + val);
            }
            return syncInfo;
        }

        private static MoneroHardForkInfo ConvertRpcHardForkInfo(Dictionary<string, object> rpcHardForkInfo)
        {
            var info = new MoneroHardForkInfo();
            foreach (string key in rpcHardForkInfo.Keys)
            {
                object val = rpcHardForkInfo[key];
                if (key.Equals("earliest_height")) info.SetEarliestHeight(Convert.ToUInt64(val));
                else if (key.Equals("enabled")) info.SetIsEnabled((bool)val);
                else if (key.Equals("state")) info.SetState(Convert.ToUInt32(val));
                else if (key.Equals("status")) { }     // handled elsewhere
                else if (key.Equals("untrusted")) { }  // handled elsewhere
                else if (key.Equals("threshold")) info.SetThreshold(Convert.ToUInt32(val));
                else if (key.Equals("version")) info.SetVersion(Convert.ToUInt32(val));
                else if (key.Equals("votes")) info.SetNumVotes(Convert.ToUInt32(val));
                else if (key.Equals("voting")) info.SetVoting(Convert.ToUInt32(val));
                else if (key.Equals("window")) info.SetWindow(Convert.ToUInt32(val));
                else if (key.Equals("credits")) info.SetCredits(Convert.ToUInt32(val));
                else if (key.Equals("top_hash")) info.SetTopBlockHash("".Equals(val) ? null : (string)val);
                else MoneroUtils.Log(0, "ignoring unexpected field in hard fork info: " + key + ": " + val);
            }
            return info;
        }

        private static MoneroConnectionSpan ConvertRpcConnectionSpan(Dictionary<string, object> rpcConnectionSpan)
        {
            MoneroConnectionSpan span = new MoneroConnectionSpan();
            foreach (string key in rpcConnectionSpan.Keys)
            {
                object val = rpcConnectionSpan[key];
                if (key.Equals("connection_id")) span.SetConnectionId((string)val);
                else if (key.Equals("nblocks")) span.SetNumBlocks(Convert.ToUInt64(val));
                else if (key.Equals("rate")) span.SetRate(Convert.ToUInt64(val));
                else if (key.Equals("remote_address")) { if (!"".Equals(val)) span.SetRemoteAddress((string)val); }
                else if (key.Equals("size")) span.SetSize(Convert.ToUInt64(val));
                else if (key.Equals("speed")) span.SetSpeed(Convert.ToUInt64(val));
                else if (key.Equals("start_block_height")) span.SetStartHeight(Convert.ToUInt64(val));
                else MoneroUtils.Log(0, "ignoring unexpected field in daemon connection span: " + key + ": " + val);
            }
            return span;
        }

        private static Dictionary<string, object?> ConvertToRpcBan(MoneroBan ban)
        {
            Dictionary<string, object?> rpcBan = [];
            rpcBan.Add("host", ban.GetHost());
            rpcBan.Add("ip", ban.GetIp());
            rpcBan.Add("ban", ban.IsBanned());
            rpcBan.Add("seconds", ban.GetSeconds());
            return rpcBan;
        }

        private static MoneroMiningStatus ConvertRpcMiningStatus(Dictionary<string, object> rpcStatus)
        {
            var status = new MoneroMiningStatus();
            status.SetIsActive((bool)rpcStatus["active"]);
            status.SetSpeed(Convert.ToUInt64(rpcStatus["speed"]));
            status.SetNumThreads(Convert.ToUInt32(rpcStatus["threads_count"]));
            if (status.IsActive() == true)
            {
                status.SetAddress((string)rpcStatus["address"]);
                status.SetIsBackground((bool)rpcStatus["is_background_mining_enabled"]);
            }
            return status;
        }

        private static MoneroAltChain ConvertRpcAltChain(Dictionary<string, object> rpcChain)
        {
            var chain = new MoneroAltChain();
            foreach (string key in rpcChain.Keys)
            {
                object val = rpcChain[key];
                if (key.Equals("block_hash")) { }  // using block_hashes instead
                else if (key.Equals("difficulty")) { } // handled by wide_difficulty
                else if (key.Equals("difficulty_top64")) { }  // handled by wide_difficulty
                else if (key.Equals("wide_difficulty")) chain.SetDifficulty(GenUtils.Reconcile(Convert.ToUInt64(chain.GetDifficulty()), PrefixedHexToUulong((string)val)));
                else if (key.Equals("height")) chain.SetHeight(Convert.ToUInt64(val));
                else if (key.Equals("length")) chain.SetLength(Convert.ToUInt64(val));
                else if (key.Equals("block_hashes")) chain.SetBlockHashes(((JArray)val).ToObject<List<string>>());
                else if (key.Equals("main_chain_parent_block")) chain.SetMainChainParentBlockHash((string)val);
                else MoneroUtils.Log(0, "ignoring unexpected field in alternative chain: " + key + ": " + val);
            }
            return chain;
        }

        public static ulong PrefixedHexToUulong(string hex)
        {
            if (!hex.StartsWith("0x"))
                throw new ArgumentException("Given hex does not start with \"0x\": " + hex);

            return Convert.ToUInt64(hex.Substring(2), 16);
        }

        private void RefreshListening()
        {
            if (_daemonPoller == null && _listeners.Count > 0) _daemonPoller = new DaemonPoller(this);
            if (_daemonPoller != null) _daemonPoller.SetIsPolling(_listeners.Count > 0);
        }

        #endregion
    }

    internal class NextBlockListener : MoneroDaemonListener {
        private MoneroDaemonRpc _daemon;
        private readonly object _syncLock;

        public NextBlockListener(MoneroDaemonRpc daemon, object syncLock) {
            // nothing to construct
            this._daemon = daemon;
            this._syncLock = syncLock;
        }

        public override void OnBlockHeader(MoneroBlockHeader header) {
            base.OnBlockHeader(header);
            lock (_syncLock) {
                // notify waiting thread
                Monitor.Pulse(_syncLock);
            }
        }
    }

    internal class DaemonPoller {
    
        private static readonly ulong DefaultPollPeriodInMs = 10000; // Poll every X ms
    
        private readonly MoneroDaemonRpc _daemon;
        private readonly TaskLooper _looper;
        private MoneroBlockHeader? _lastHeader;
    
        public DaemonPoller(MoneroDaemonRpc daemon) {
            this._daemon = daemon;
            _looper = new TaskLooper(() => { Poll(); });
        }
    
        public void SetIsPolling(bool isPolling) {
            if (isPolling) _looper.Start(DefaultPollPeriodInMs); // TODO: allow configurable Poll period
            else _looper.Stop();
        }
    
        private void Poll() {
            try {
                // get first header for comparison
                if (_lastHeader == null) {
                    _lastHeader = _daemon.GetLastBlockHeader();
                    return;
                }
            
                // fetch and compare latest block header
                MoneroBlockHeader header = _daemon.GetLastBlockHeader();
                if (!header.GetHash()!.Equals(_lastHeader.GetHash())) {
                    _lastHeader = header;
                    lock(_daemon.GetListeners()) {
                        AnnounceBlockHeader(header);
                    }
                }
            } catch (Exception e)
            {
                MoneroUtils.Log(1, e.Message);
                if (e.StackTrace != null) MoneroUtils.Log(1, e.StackTrace);
            }
        }

        private void AnnounceBlockHeader(MoneroBlockHeader header) {
            foreach (MoneroDaemonListener listener in _daemon.GetListeners()) {
                try {
                    listener.OnBlockHeader(header);
                } catch (Exception e) {
                    MoneroUtils.Log(0, "Error calling listener on new block header: " + e.Message);
                    if (e.StackTrace != null) MoneroUtils.Log(1, e.StackTrace);
                }
            }
        }
    }
}
