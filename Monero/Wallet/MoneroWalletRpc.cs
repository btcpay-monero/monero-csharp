﻿using Monero.Common;
using Monero.Wallet.Common;
using System.Diagnostics;
using System.Text;
using MoneroJsonRpcParams = System.Collections.Generic.Dictionary<string, object>;

namespace Monero.Wallet
{
    public class MoneroWalletRpc : MoneroWalletDefault
    {
        // class variables
        //private static final Logger LOGGER = Logger.GetLogger(MoneroWalletRpc.class.GetName());
        //private static readonly MoneroTxHeightComparer TX_HEIGHT_COMPARATOR = new();
        private static readonly int ERROR_CODE_INVALID_PAYMENT_ID = -5; // invalid payment id error code
        private static readonly ulong DEFAULT_SYNC_PERIOD_IN_MS = 20000; // default period between syncs in ms (defined by DEFAULT_AUTO_REFRESH_PERIOD in wallet_rpc_server.cpp)

        // instance variables
        private readonly object SYNC_LOCK = new(); // lock for synchronizing sync requests
        private string? path;                                     // wallet's path identifier
        private MoneroRpcConnection rpc;                         // rpc connection to monero-wallet-rpc
        private MoneroRpcConnection? daemonConnection;            // current daemon connection (unknown/null until explicitly set)
        private MoneroWalletPoller? walletPoller;                       // listener which polls monero-wallet-rpc
        //private WalletRpcZmqListener zmqListener;                // listener which processes zmq notifications from monero-wallet-rpc
        private readonly Dictionary<uint, Dictionary<uint, string>> addressCache = []; // cache static addresses to reduce requests
        private Process? process;                                 // process running monero-wallet-rpc if applicable
        private ulong syncPeriodInMs = DEFAULT_SYNC_PERIOD_IN_MS; // period between syncs in ms (default 20000)

        public override MoneroWalletType GetWalletType()
        {
            return MoneroWalletType.RPC;
        }

        public MoneroWalletRpc(MoneroRpcConnection connection)
        {
            rpc = connection;
            CheckRpcConnection();
        }

        public MoneroWalletRpc(string url, string? username = null, string? password = null)
        {
            rpc = new MoneroRpcConnection(url, username, password);
            CheckRpcConnection();
        }

        public MoneroWalletRpc(List<string> cmd)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = cmd[0],
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (cmd.Count > 1)
            {
                startInfo.Arguments = string.Join(" ", cmd.Skip(1));
                Console.WriteLine("Starting monero wallet rpc with arguments: " + startInfo.Arguments); // debug log
            }

            startInfo.Environment["LANG"] = "en_US.UTF-8";

            process = new Process { StartInfo = startInfo };
            process.Start();

            var sb = new StringBuilder();
            string uri = null;
            bool success = false;

            using (var reader = process.StandardOutput)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(line); // debug log

                    sb.AppendLine(line);

                    const string uriLineContains = "Binding on ";
                    int idx = line.IndexOf(uriLineContains, StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        string host = line.Substring(idx + uriLineContains.Length,
                                                     line.LastIndexOf(' ') - (idx + uriLineContains.Length));
                        string port = line.Substring(line.LastIndexOf(':') + 1);
                        bool sslEnabled = false;
                        int sslIdx = cmd.IndexOf("--rpc-ssl");
                        if (sslIdx >= 0 && sslIdx + 1 < cmd.Count)
                        {
                            sslEnabled = string.Equals(cmd[sslIdx + 1], "enabled", StringComparison.OrdinalIgnoreCase);
                        }
                        uri = (sslEnabled ? "https" : "http") + "://" + host + ":" + port;
                    }

                    if (line.Contains("Starting wallet RPC server"))
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                string l;
                                while ((l = reader.ReadLine()) != null)
                                {
                                    Console.WriteLine(l);
                                }
                            }
                            catch (IOException)
                            {
                            }
                        });

                        success = true;
                        break;
                    }
                }
            }

            if (!success)
            {
                throw new MoneroError("Failed to start monero-wallet-rpc server:\n\n" + sb.ToString().Trim());
            }

            string username = null;
            string password = null;
            int userPassIdx = cmd.IndexOf("--rpc-login");
            if (userPassIdx >= 0 && userPassIdx + 1 < cmd.Count)
            {
                string userPass = cmd[userPassIdx + 1];
                int sep = userPass.IndexOf(':');
                if (sep >= 0)
                {
                    username = userPass.Substring(0, sep);
                    password = userPass.Substring(sep + 1);
                }
            }

            string zmqUri = null;
            int zmqUriIdx = cmd.IndexOf("--zmq-pub");
            if (zmqUriIdx >= 0 && zmqUriIdx + 1 < cmd.Count)
            {
                zmqUri = cmd[zmqUriIdx + 1];
            }

            rpc = new MoneroRpcConnection(uri, username, password, zmqUri);
        }

        #region RPC Wallet Methods

        public MoneroWalletRpc OpenWallet(MoneroWalletConfig config)
        {

            // validate config
            if (config == null) throw new MoneroError("Must provide configuration of wallet to open");
            if (config.GetPath() == null || config.GetPath().Length == 0) throw new MoneroError("Filename is not initialized");
            // TODO: ensure other fields are uninitialized?

            // open wallet on rpc server
            Dictionary<string, object> parameters = [];
            parameters.Add("filename", config.GetPath());
            parameters.Add("password", config.GetPassword() == null ? "" : config.GetPassword());
            MoneroJsonRpcRequest request = new MoneroJsonRpcRequest("open_wallet", parameters);
            rpc.SendJsonRequest(request);
            Clear();
            path = config.GetPath();

            // set connection manager or server
            if (config.GetConnectionManager() != null)
            {
                if (config.GetServer() != null) throw new MoneroError("Wallet can be opened with a server or connection manager but not both");
                SetConnectionManager(config.GetConnectionManager());
            }
            else if (config.GetServer() != null)
            {
                SetDaemonConnection(config.GetServer());
            }

            return this;
        }

        public MoneroWalletRpc OpenWallet(string path, string? password = null)
        {
            return OpenWallet(new MoneroWalletConfig().SetPath(path).SetPassword(password));
        }

        public MoneroWalletRpc CreateWallet(MoneroWalletConfig config)
        {
            // validate config
            if (config == null) throw new MoneroError("Must specify config to create wallet");
            if (config.GetNetworkType() != null) throw new MoneroError("Cannot specify network type when creating RPC wallet");
            if (config.GetSeed() != null && (config.GetPrimaryAddress() != null || config.GetPrivateViewKey() != null || config.GetPrivateSpendKey() != null))
            {
                throw new MoneroError("Wallet can be initialized with a seed or keys but not both");
            }
            if (config.GetAccountLookahead() != null || config.GetSubaddressLookahead() != null) throw new MoneroError("monero-wallet-rpc does not support creating wallets with subaddress lookahead over rpc");

            // set server from connection manager if provided
            if (config.GetConnectionManager() != null)
            {
                if (config.GetServer() != null) throw new MoneroError("Wallet can be created with a server or connection manager but not both");
                config.SetServer((MoneroRpcConnection)config.GetConnectionManager().GetConnection());
            }

            // create wallet
            if (config.GetSeed() != null) CreateWalletFromSeed(config);
            else if (config.GetPrivateSpendKey() != null || config.GetPrimaryAddress() != null) CreateWalletFromKeys(config);
            else CreateWalletRandom(config);

            // set connection manager or server
            if (config.GetConnectionManager() != null)
            {
                SetConnectionManager(config.GetConnectionManager());
            }
            else if (config.GetServer() != null)
            {
                SetDaemonConnection(config.GetServer());
            }

            return this;
        }

        private MoneroWalletRpc CreateWalletRandom(MoneroWalletConfig config)
        {

            // validate and normalize config
            config = config.Clone();
            if (config.GetSeedOffset() != null) throw new MoneroError("Cannot specify seed offset when creating random wallet");
            if (config.GetRestoreHeight() != null) throw new MoneroError("Cannot specify restore height when creating random wallet");
            if (config.GetSaveCurrent() == false) throw new MoneroError("Current wallet is saved automatically when creating random wallet");
            if (config.GetPath() == null || config.GetPath().Length == 0) throw new MoneroError("Wallet name is not initialized");
            if (config.GetLanguage() == null || config.GetLanguage().Length == 0) config.SetLanguage(MoneroWallet.DEFAULT_LANGUAGE);

            // send request
            Dictionary<string, object> parameters = [];
            parameters.Add("filename", config.GetPath());
            parameters.Add("password", config.GetPassword());
            parameters.Add("language", config.GetLanguage());
            MoneroJsonRpcRequest request = new MoneroJsonRpcRequest("create_wallet", parameters);
            try { rpc.SendJsonRequest(request); }
            catch (MoneroRpcError e) { HandleCreateWalletError(config.GetPath(), e); }
            Clear();
            path = config.GetPath();
            return this;
        }

        private MoneroWalletRpc CreateWalletFromSeed(MoneroWalletConfig config)
        {
            config = config.Clone();
            if (config.GetLanguage() == null || config.GetLanguage().Length == 0) config.SetLanguage(MoneroWallet.DEFAULT_LANGUAGE);
            Dictionary<string, object> parameters = [];
            parameters.Add("filename", config.GetPath());
            parameters.Add("password", config.GetPassword());
            parameters.Add("seed", config.GetSeed());
            parameters.Add("seed_offset", config.GetSeedOffset());
            parameters.Add("restore_height", config.GetRestoreHeight());
            parameters.Add("language", config.GetLanguage());
            parameters.Add("autosave_current", config.GetSaveCurrent());
            parameters.Add("enable_multisig_experimental", config.IsMultisig());

            MoneroJsonRpcRequest request = new MoneroJsonRpcRequest("restore_deterministic_wallet", parameters);
            try { rpc.SendJsonRequest(request); }
            catch (MoneroRpcError e) { HandleCreateWalletError(config.GetPath(), e); }
            Clear();
            path = config.GetPath();
            return this;
        }

        private MoneroWalletRpc CreateWalletFromKeys(MoneroWalletConfig config)
        {
            config = config.Clone();
            if (config.GetSeedOffset() != null) throw new MoneroError("Cannot specify seed offset when creating wallet from keys");
            if (config.GetRestoreHeight() == null) config.SetRestoreHeight(0);
            Dictionary<string, object> parameters = [];

            parameters.Add("filename", config.GetPath());
            parameters.Add("password", config.GetPassword());
            parameters.Add("address", config.GetPrimaryAddress());
            parameters.Add("viewkey", config.GetPrivateViewKey());
            parameters.Add("spendkey", config.GetPrivateSpendKey());
            parameters.Add("restore_height", config.GetRestoreHeight());
            parameters.Add("autosave_current", config.GetSaveCurrent());

            MoneroJsonRpcRequest request = new MoneroJsonRpcRequest("generate_from_keys", parameters);
            try { rpc.SendJsonRequest(request); }
            catch (MoneroRpcError e) { HandleCreateWalletError(config.GetPath(), e); }
            Clear();
            path = config.GetPath();
            return this;
        }

        private static void HandleCreateWalletError(string name, MoneroRpcError e)
        {
            if (e.Message.Equals("Cannot create wallet. Already exists.")) throw new MoneroRpcError("Wallet already exists: " + name, e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());
            if (e.Message.Equals("Electrum-style word list failed verification")) throw new MoneroRpcError("Invalid mnemonic", e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());
            throw e;
        }

        public Process? GetProcess() { return process; }

        public int StopProcess(bool force = false)
        {
            if (process == null)
            {
                throw new MoneroError("MoneroWalletRpc instance not created from new process");
            }

            Clear();

            if (force)
            {
                try
                {
                    process.Kill(true);
                }
                catch (Exception e)
                {
                    throw new MoneroError(e);
                }
            }
            else
            {
                try
                {
                    process.Close();
                }
                catch (Exception e)
                {
                    throw new MoneroError(e);
                }
            }

            try
            {
                process.WaitForExit();
                return process.ExitCode;
            }
            catch (Exception e)
            {
                throw new MoneroError(e);
            }
        }

        public MoneroRpcConnection GetRpcConnection() { return rpc; }

        private void CheckRpcConnection()
        {
            if (rpc == null || rpc.IsConnected() == true) return;

            rpc.CheckConnection(2000);
        }

        #endregion

        #region Common Wallet Methods

        public override void AddListener(MoneroWalletListener listener)
        {
            base.AddListener(listener);
            RefreshListening();
        }

        public override void RemoveListener(MoneroWalletListener listener)
        {
            base.RemoveListener(listener);
            RefreshListening();
        }

        public override bool IsViewOnly()
        {
            try
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("key_type", "mnemonic");
                rpc.SendJsonRequest("query_key", parameters);
                return false; // key retrieval succeeds if not view-only
            }
            catch (MoneroError e)
            {
                if (-29 == e.GetCode()) return true;  // wallet is view-only
                if (-1 == e.GetCode()) return false;  // wallet is offline but not view-only
                throw e;
            }
        }

        public override void SetDaemonConnection(MoneroRpcConnection? connection)
        {
            SetDaemonConnection(connection, null, null);
        }

        public void SetDaemonConnection(MoneroRpcConnection? connection, bool? isTrusted, SslOptions? sslOptions)
        {
            if (sslOptions == null) sslOptions = new SslOptions();
            MoneroJsonRpcParams parameters = [];
            parameters.Add("address", connection == null ? "placeholder" : connection.GetUri());
            parameters.Add("username", connection == null ? "" : connection.GetUsername());
            parameters.Add("password", connection == null ? "" : connection.GetPassword());
            parameters.Add("trusted", isTrusted);
            parameters.Add("ssl_support", "autodetect");
            parameters.Add("ssl_private_key_path", sslOptions.GetPrivateKeyPath());
            parameters.Add("ssl_certificate_path", sslOptions.GetCertificatePath());
            parameters.Add("ssl_ca_file", sslOptions.GetCertificateAuthorityFile());
            parameters.Add("ssl_allowed_fingerprints", sslOptions.GetAllowedFingerprints());
            parameters.Add("ssl_allow_any_cert", sslOptions.GetAllowAnyCert());
            rpc.SendJsonRequest("set_daemon", parameters);
            this.daemonConnection = connection == null || connection.GetUri() == null || connection.GetUri().Length == 0 ? null : new MoneroRpcConnection(connection);
        }

        public override void SetProxyUri(string? uri)
        {
            throw new MoneroError("MoneroWalletRpc.SetProxyUri() not supported. Start monero-wallet-rpc with --proxy instead.");
        }

        public override MoneroRpcConnection? GetDaemonConnection()
        {
            return daemonConnection;
        }

        public override bool IsConnectedToDaemon()
        {
            try
            {
                CheckReserveProof(GetPrimaryAddress(), "", ""); // TODO (monero-project): provide better way to know if wallet rpc is connected to daemon
                throw new Exception("check reserve expected to fail");
            }
            catch (MoneroError e)
            {
                return !e.Message.Contains("Failed to connect to daemon");
            }
        }

        public override MoneroVersion GetVersion()
        {
            var resp = rpc.SendJsonRequest("get_version");
            var result = resp.Result;
            return new MoneroVersion(((int)result["version"]), (bool)result["release"]);
        }

        public override string? GetPath()
        {
            return path;
        }

        public override string GetSeed()
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_type", "mnemonic");
            var resp = rpc.SendJsonRequest("query_key", parameters);
            var result = resp.Result;
            return (string)result["key"];
        }

        public override string GetSeedLanguage()
        {
            throw new MoneroError("MoneroWalletRpc.GetSeedLanguage() not supported");
        }

        public override string GetPrivateViewKey()
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_type", "view_key");
            var resp = rpc.SendJsonRequest("query_key", parameters);
            var result = resp.Result;
            return (string)result["key"];
        }

        public override string GetPublicViewKey()
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_type", "public_view_key");
            var resp = rpc.SendJsonRequest("query_key", parameters);
            var result = resp.Result;
            return (string)result["key"];
        }

        public override string GetPublicSpendKey()
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_type", "public_spend_key");
            var resp = rpc.SendJsonRequest("query_key", parameters);
            var result = resp.Result;
            return (string)result["key"];
        }

        public override string GetPrivateSpendKey()
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_type", "spend_key");
            var resp = rpc.SendJsonRequest("query_key", parameters);
            var result = resp.Result;
            return (string)result["key"];
        }

        public override string GetAddress(uint accountIdx, uint subaddressIdx)
        {
            Dictionary<uint, string>? subaddressMap = addressCache.GetValueOrDefault(accountIdx);
            if (subaddressMap == null)
            {
                GetSubaddresses(accountIdx, null, true);      // cache's all addresses at this account
                return GetAddress(accountIdx, subaddressIdx); // uses cache
            }
            string? address = subaddressMap[subaddressIdx];
            if (address == null)
            {
                GetSubaddresses(accountIdx, null, true);      // cache's all addresses at this account
                return addressCache[accountIdx][subaddressIdx];
            }
            return address;
        }

        public override MoneroSubaddress GetAddressIndex(string address)
        {
            // fetch result and normalize error if address does not belong to the wallet
            Dictionary<string, object>? result;
            try
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("address", address);
                var resp = rpc.SendJsonRequest("get_address_index", parameters);
                result = resp.Result;
            }
            catch (MoneroRpcError e)
            {
                MoneroUtils.Log(0, e.Message);
                if (-2 == e.GetCode()) throw new MoneroError(e.Message, e.GetCode());
                throw e;
            }

            // convert rpc response
            var rpcIndices = (Dictionary<string, uint>)result["index"];
            MoneroSubaddress subaddress = new MoneroSubaddress(address);
            subaddress.SetAccountIndex((uint)rpcIndices["major"]);
            subaddress.SetIndex((uint)rpcIndices["minor"]);
            return subaddress;
        }

        public override MoneroIntegratedAddress GetIntegratedAddress(string? standardAddress = null, string? paymentId = null)
        {
            try
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("standard_address", standardAddress);
                parameters.Add("payment_id", paymentId);
                var resp = rpc.SendJsonRequest("make_integrated_address", parameters);
                var result = resp.Result;
                string integratedAddressStr = (string)result["integrated_address"];
                return DecodeIntegratedAddress(integratedAddressStr);
            }
            catch (MoneroRpcError e)
            {
                if (e.Message.Contains("Invalid payment ID")) throw new MoneroError("Invalid payment ID: " + paymentId, ERROR_CODE_INVALID_PAYMENT_ID);
                throw e;
            }
        }

        public override MoneroIntegratedAddress DecodeIntegratedAddress(string integratedAddress)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("integrated_address", integratedAddress);
            var resp = rpc.SendJsonRequest("split_integrated_address", parameters);
            var result = resp.Result;
            return new MoneroIntegratedAddress((string)result["standard_address"], (string)result["payment_id"], integratedAddress);
        }

        public override ulong GetHeight()
        {
            var resp = rpc.SendJsonRequest("get_height");
            var result = resp.Result;
            return ((ulong)result["height"]);
        }

        public override ulong GetDaemonHeight()
        {
            throw new MoneroError("monero-wallet-rpc does not support getting the chain height");
        }

        public override ulong GetHeightByDate(int year, int month, int day)
        {
            throw new MoneroError("monero-wallet-rpc does not support getting a height by date");
        }

        public override MoneroSyncResult Sync(ulong? startHeight = null, MoneroWalletListener? listener = null)
        {
            if (listener != null) throw new MoneroError("Monero Wallet RPC does not support reporting sync progress");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("start_height", startHeight);
            lock(SYNC_LOCK) {  // TODO (monero-project): monero-wallet-rpc hangs at 100% cpu utilization if refresh called concurrently
                try
                {
                    var resp = rpc.SendJsonRequest("refresh", parameters);
                    Poll();
                    var result = resp.Result;
                    return new MoneroSyncResult(((ulong)result["blocks_fetched"]), (bool)result["received_money"]);
                }
                catch (MoneroError err)
                {
                    if (err.Message.Equals("no connection to daemon")) throw new MoneroError("Wallet is not connected to daemon");
                    throw err;
                }
            }
        }

        public override void StartSyncing(ulong? syncPeriodInMs = null)
        {
            // convert ms to seconds for rpc parameter
            ulong syncPeriodInSeconds = (syncPeriodInMs == null ? DEFAULT_SYNC_PERIOD_IN_MS : (ulong)syncPeriodInMs) / 1000;

            // send rpc request
            MoneroJsonRpcParams parameters = [];
            parameters.Add("enable", true);
            parameters.Add("period", syncPeriodInSeconds);
            rpc.SendJsonRequest("auto_refresh", parameters);

            // update sync period for poller
            this.syncPeriodInMs = syncPeriodInSeconds * 1000;
            if (walletPoller != null) walletPoller.SetPeriodInMs(this.syncPeriodInMs);

            // poll if listening
            Poll();
        }

        public override void StopSyncing()
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("enable", false);
            rpc.SendJsonRequest("auto_refresh", parameters);
        }

        public override void ScanTxs(List<string> txHashes)
        {
            if (txHashes == null || txHashes.Count == 0) throw new MoneroError("No tx hashes given to scan");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txids", txHashes);
            rpc.SendJsonRequest("scan_tx", parameters);
            Poll(); // notify of changes
        }

        public override void RescanSpent()
        {
            rpc.SendJsonRequest("rescan_spent");
        }

        public override void RescanBlockchain()
        {
            rpc.SendJsonRequest("rescan_blockchain");
        }

        public override ulong GetBalance(uint? accountIdx = null, uint? subaddressIdx = null)
        {
            return GetBalances(accountIdx, subaddressIdx)[0];
        }

        public override ulong GetUnlockedBalance(uint? accountIdx = null, uint? subaddressIdx = null)
        {
            return GetBalances(accountIdx, subaddressIdx)[1];
        }

        public override List<MoneroAccount> GetAccounts(bool includeSubaddresses = false, string? tag = null)
        {
            return GetAccounts(includeSubaddresses, tag, false);
        }

        public List<MoneroAccount> GetAccounts(bool includeSubaddresses, string? tag, bool skipBalances)
        {
            // fetch accounts from rpc
            MoneroJsonRpcParams parameters = [];
            parameters.Add("tag", tag);
            var resp = rpc.SendJsonRequest("get_accounts", parameters);
            var result = resp.Result;

            // build account objects and fetch subaddresses per account using get_address
            // TODO monero-wallet-rpc: get_address should support all_accounts so not called once per account
            List<MoneroAccount> accounts = [];
            foreach (var rpcAccount in (List<Dictionary<string, object>>)result["subaddress_accounts"])
            {
                MoneroAccount account = ConvertRpcAccount(rpcAccount);
                if (includeSubaddresses) account.SetSubaddresses(GetSubaddresses((uint)account.GetIndex(), null, true));
                accounts.Add(account);
            }

            // fetch and merge fields from get_balance across all accounts
            if (includeSubaddresses && !skipBalances)
            {

                // these fields are not initialized if subaddress is unused and therefore not returned from `get_balance`
                foreach (MoneroAccount account in accounts)
                {
                    foreach (MoneroSubaddress subaddress in account.GetSubaddresses())
                    {
                        subaddress.SetBalance(0);
                        subaddress.SetUnlockedBalance(0);
                        subaddress.SetNumUnspentOutputs(0);
                        subaddress.SetNumBlocksToUnlock(0);
                    }
                }
      
                // fetch and merge info from get_balance
                parameters.Clear();
                parameters.Add("all_accounts", true);
                resp = rpc.SendJsonRequest("get_balance", parameters);
                result = resp.Result;
                if (result.ContainsKey("per_subaddress"))
                {
                    foreach (var rpcSubaddress in (List<Dictionary<string, object>>)result["per_subaddress"])
                    {
                        MoneroSubaddress subaddress = ConvertRpcSubaddress(rpcSubaddress);

                        // merge info
                        MoneroAccount account = accounts[(int)subaddress.GetAccountIndex()];
                        if(account.GetIndex() != subaddress.GetAccountIndex()) throw new MoneroError("RPC accounts are out of order");  // would need to switch lookup to loop
                        MoneroSubaddress tgtSubaddress = account.GetSubaddresses()[(int)subaddress.GetIndex()];
                        if (tgtSubaddress.GetIndex() != subaddress.GetIndex()) throw new MoneroError("RPC subaddresses are out of order");
                        if (subaddress.GetBalance() != null) tgtSubaddress.SetBalance(subaddress.GetBalance());
                        if (subaddress.GetUnlockedBalance() != null) tgtSubaddress.SetUnlockedBalance(subaddress.GetUnlockedBalance());
                        if (subaddress.GetNumUnspentOutputs() != null) tgtSubaddress.SetNumUnspentOutputs(subaddress.GetNumUnspentOutputs());
                        if (subaddress.GetNumBlocksToUnlock() != null) tgtSubaddress.SetNumBlocksToUnlock(subaddress.GetNumBlocksToUnlock());
                    }
                }
            }

            // return accounts
            return accounts;
        }

        public override MoneroAccount GetAccount(uint accountIdx, bool includeSubaddresses = false)
        {
            return GetAccount(accountIdx, includeSubaddresses, false);
        }

        public MoneroAccount GetAccount(uint accountIdx, bool includeSubaddresses, bool skipBalances)
        {
            if (accountIdx < 0) throw new MoneroError("Account index must be greater than or equal to 0");
            foreach (MoneroAccount account in GetAccounts())
            {
                if (account.GetIndex() == accountIdx)
                {
                    if (includeSubaddresses) account.SetSubaddresses(GetSubaddresses(accountIdx, null, skipBalances));
                    return account;
                }
            }
            throw new MoneroError("Account with index " + accountIdx + " does not exist");
        }

        public override MoneroAccount CreateAccount(string? label = null)
        {
            label = label == null || label.Length == 0 ? null : label;
            MoneroJsonRpcParams parameters = [];
            parameters.Add("label", label);
            var resp = rpc.SendJsonRequest("create_account", parameters);
            var result = resp.Result;
            return new MoneroAccount(((uint)result["account_index"]), (string)result["address"], 0, 0, null);
        }

        public override List<MoneroSubaddress> GetSubaddresses(uint accountIdx, List<uint>? subaddressIndices = null)
        {
            return GetSubaddresses(accountIdx, subaddressIndices, false);
        }

        public List<MoneroSubaddress> GetSubaddresses(uint accountIdx, List<uint>? subaddressIndices, bool skipBalances)
        {
            // fetch subaddresses
            MoneroJsonRpcParams parameters = [];
            parameters.Add("account_index", accountIdx);
            if (subaddressIndices != null && subaddressIndices.Count > 0) parameters.Add("address_index", subaddressIndices);
            var resp = rpc.SendJsonRequest("get_address", parameters);
            var result = resp.Result;

            // initialize subaddresses
            List<MoneroSubaddress> subaddresses = [];
            foreach (var rpcSubaddress in (List<Dictionary<string, object>>)result["addresses"])
            {
                MoneroSubaddress subaddress = ConvertRpcSubaddress(rpcSubaddress);
                subaddress.SetAccountIndex(accountIdx);
                subaddresses.Add(subaddress);
            }

            // fetch and initialize subaddress balances
            if (!skipBalances)
            {

                // these fields are not initialized if subaddress is unused and therefore not returned from `get_balance`
                foreach (MoneroSubaddress subaddress in subaddresses)
                {
                    subaddress.SetBalance(0);
                    subaddress.SetUnlockedBalance(0);
                    subaddress.SetNumUnspentOutputs(0);
                    subaddress.SetNumBlocksToUnlock(0);
                }

                // fetch and initialize balances
                resp = rpc.SendJsonRequest("get_balance", parameters);
                result = resp.Result;
                if (result.ContainsKey("per_subaddress"))
                {
                    foreach (var rpcSubaddress in (List<Dictionary<string, object>>)result["per_subaddress"])
                    {
                        MoneroSubaddress subaddress = ConvertRpcSubaddress(rpcSubaddress);

                        // transfer info to existing subaddress object
                        foreach (MoneroSubaddress tgtSubaddress in subaddresses)
                        {
                            if (tgtSubaddress.GetIndex() != subaddress.GetIndex()) continue; // skip to subaddress with same index
                            if (subaddress.GetBalance() != null) tgtSubaddress.SetBalance(subaddress.GetBalance());
                            if (subaddress.GetUnlockedBalance() != null) tgtSubaddress.SetUnlockedBalance(subaddress.GetUnlockedBalance());
                            if (subaddress.GetNumUnspentOutputs() != null) tgtSubaddress.SetNumUnspentOutputs(subaddress.GetNumUnspentOutputs());
                            if (subaddress.GetNumBlocksToUnlock() != null) tgtSubaddress.SetNumBlocksToUnlock(subaddress.GetNumBlocksToUnlock());
                        }
                    }
                }
            }

            // cache addresses
            var subaddressMap = addressCache[accountIdx];
            if (subaddressMap == null)
            {
                subaddressMap = new Dictionary<uint, string>();
                addressCache.Add(accountIdx, subaddressMap);
            }
            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                subaddressMap.Add((uint)subaddress.GetIndex(), subaddress.GetAddress());
            }

            // return results
            return subaddresses;
        }

        public override MoneroSubaddress CreateSubaddress(uint accountIdx, string? label = null)
        {
            // send request
            MoneroJsonRpcParams parameters = [];
            parameters.Add("account_index", accountIdx);
            parameters.Add("label", label);
            var resp = rpc.SendJsonRequest("create_address", parameters);
            var result = resp.Result;

            // build subaddress object
            MoneroSubaddress subaddress = new MoneroSubaddress();
            subaddress.SetAccountIndex(accountIdx);
            subaddress.SetIndex(((uint)result["address_index"]));
            subaddress.SetAddress((string)result["address"]);
            subaddress.SetLabel(label);
            subaddress.SetBalance(0);
            subaddress.SetUnlockedBalance(0);
            subaddress.SetNumUnspentOutputs(0);
            subaddress.SetIsUsed(false);
            subaddress.SetNumBlocksToUnlock(0);
            return subaddress;
        }

        public override void SetSubaddressLabel(uint accountIdx, uint subaddressIdx, string label)
        {
            Dictionary<string, object> parameters = [];
            Dictionary<string, uint> idx = [];
            idx.Add("major", accountIdx);
            idx.Add("minor", subaddressIdx);
            parameters.Add("index", idx);
            parameters.Add("label", label);
            rpc.SendJsonRequest("label_address", parameters);
        }

        public override List<MoneroTxWallet> GetTxs(MoneroTxQuery query)
        {
            // copy and normalize query
            query = query == null ? new MoneroTxQuery() : query.Clone();
            if (query.GetInputQuery() != null) query.GetInputQuery().SetTxQuery(query);
            if (query.GetOutputQuery() != null) query.GetOutputQuery().SetTxQuery(query);

            // temporarily disable transfer and output queries in order to collect all tx information
            MoneroTransferQuery transferQuery = query.GetTransferQuery();
            MoneroOutputQuery inputQuery = query.GetInputQuery();
            MoneroOutputQuery outputQuery = query.GetOutputQuery();
            query.SetTransferQuery(null);
            query.SetInputQuery(null);
            query.SetOutputQuery(null);

            // fetch all transfers that meet tx query
            List<MoneroTransfer> transfers = GetTransfersAux(new MoneroTransferQuery().SetTxQuery(Decontextualize(query.Clone())));

            // collect unique txs from transfers while retaining order
            List<MoneroTxWallet> txs = [];
            HashSet<MoneroTxWallet> txsSet = new HashSet<MoneroTxWallet>();
            foreach (MoneroTransfer transfer in transfers)
            {
                if (!txsSet.Contains(transfer.GetTx()))
                {
                    txs.Add(transfer.GetTx());
                    txsSet.Add(transfer.GetTx());
                }
            }

            // cache types into maps for merging and lookup
            Dictionary<string, MoneroTxWallet> txMap = [];
            Dictionary<ulong, MoneroBlock> blockMap = [];
            foreach (MoneroTxWallet tx in txs)
            {
                MergeTx(tx, txMap, blockMap);
            }

            // fetch and merge outputs if queried
            if (query.GetIncludeOutputs() == true || outputQuery != null)
            {

                // fetch outputs
                MoneroOutputQuery outputQueryAux = (outputQuery != null ? outputQuery.Clone() : new MoneroOutputQuery()).SetTxQuery(Decontextualize(query.Clone()));
                List<MoneroOutputWallet> outputs = GetOutputsAux(outputQueryAux);

                // merge output txs one time while retaining order
                HashSet<MoneroTxWallet> outputTxs = new HashSet<MoneroTxWallet>();
                foreach (MoneroOutputWallet output in outputs)
                {
                    if (!outputTxs.Contains(output.GetTx()))
                    {
                        MergeTx(output.GetTx(), txMap, blockMap);
                        outputTxs.Add(output.GetTx());
                    }
                }
            }

            // restore transfer and output queries
            query.SetTransferQuery(transferQuery);
            query.SetInputQuery(inputQuery);
            query.SetOutputQuery(outputQuery);

            // filter txs that don't meet transfer and output queries
            List<MoneroTxWallet> txsQueried = [];
            foreach (MoneroTxWallet tx in txs)
            {
                if (query.MeetsCriteria(tx)) txsQueried.Add(tx);
                else if (tx.GetBlock() != null) tx.GetBlock().GetTxs().Remove(tx);
            }
            txs = txsQueried;

            // special case: re-fetch txs if inconsistency caused by needing to make multiple rpc calls
            foreach (MoneroTxWallet tx in txs)
            {
                if (tx.IsConfirmed() == true && tx.GetBlock() == null || tx.IsConfirmed() != true && tx.GetBlock() != null)
                {
                    MoneroUtils.Log(1, "Inconsistency detected building txs from multiple rpc calls, re-fetching");
                    return GetTxs(query);
                }
            }

            // order txs if tx hashes given
            if (query.GetHashes() != null && query.GetHashes().Count > 0)
            {
                Dictionary<string, MoneroTxWallet> txsById = [];  // store txs in temporary map for sorting
                foreach (MoneroTxWallet tx in txs) txsById.Add(tx.GetHash(), tx);
                List<MoneroTxWallet> orderedTxs = [];
                foreach (string txHash in query.GetHashes()) if (txsById[txHash] != null) orderedTxs.Add(txsById[txHash]);
                txs = orderedTxs;
            }
            return txs;
        }

        public override List<MoneroTransfer> GetTransfers(MoneroTransferQuery query)
        {

            // copy and normalize query up to block
            query = NormalizeTransferQuery(query);

            // get transfers directly if query does not require tx context (other transfers, outputs)
            if (!IsContextual(query)) return GetTransfersAux(query);

            // otherwise get txs with full models to fulfill query
            List<MoneroTransfer> transfers = [];
            query.GetTxQuery().SetTransferQuery(query);
            foreach (MoneroTxWallet tx in GetTxs(query.GetTxQuery())) transfers.AddRange(tx.FilterTransfers(query));
            return transfers;
        }

        public override List<MoneroOutputWallet> GetOutputs(MoneroOutputQuery query)
        {

            // get outputs directly if query does not require tx context (other outputs, transfers)
            if (!IsContextual(query)) return GetOutputsAux(query);

            // otherwise get txs with full models to fulfill query
            List<MoneroOutputWallet> outputs = [];
            foreach (MoneroTxWallet tx in GetTxs(query.GetTxQuery())) outputs.AddRange(tx.filterOutputsWallet(query));
            return outputs;
        }

        public override string ExportOutputs(bool all = false)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("all", all);
            var resp = rpc.SendJsonRequest("export_outputs", parameters);
            var result = resp.Result;
            return (string)result["outputs_data_hex"];
        }

        public override int ImportOutputs(string outputsHex)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("outputs_data_hex", outputsHex);
            var resp = rpc.SendJsonRequest("import_outputs", parameters);
            var result = resp.Result;
            return ((int)result["num_imported"]);
        }

        public override List<MoneroKeyImage> ExportKeyImages(bool all)
        {
            return RpcExportKeyImages(all);
        }

        public override MoneroKeyImageImportResult ImportKeyImages(List<MoneroKeyImage> keyImages)
        {

            // convert key images to rpc parameter
            List<Dictionary<string, object>> rpcKeyImages = [];
            foreach (MoneroKeyImage keyImage in keyImages)
            {
                Dictionary<string, object> rpcKeyImage = [];
                rpcKeyImage.Add("key_image", keyImage.GetHex());
                rpcKeyImage.Add("signature", keyImage.GetSignature());
                rpcKeyImages.Add(rpcKeyImage);
            }

            // send rpc request
            MoneroJsonRpcParams parameters = [];
            parameters.Add("signed_key_images", rpcKeyImages);
            var resp = rpc.SendJsonRequest("import_key_images", parameters);
            var result = resp.Result;

            // build and return result
            MoneroKeyImageImportResult importResult = new MoneroKeyImageImportResult();
            importResult.SetHeight(((ulong)result["height"]));
            importResult.SetSpentAmount((ulong)result["spent"]);
            importResult.SetUnspentAmount((ulong)result["unspent"]);
            return importResult;
        }

        public override List<MoneroKeyImage> GetNewKeyImagesFromLastImport()
        {
            return RpcExportKeyImages(false);
        }

        public override void FreezeOutput(string keyImage)
        {
            if (keyImage == null) throw new MoneroError("Must specify key image to freeze");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_image", keyImage);
            rpc.SendJsonRequest("freeze", parameters);
        }

        public override void ThawOutput(string keyImage)
        {
            if (keyImage == null) throw new MoneroError("Must specify key image to thaw");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_image", keyImage);
            rpc.SendJsonRequest("thaw", parameters);
        }

        public override bool IsOutputFrozen(string keyImage)
        {
            if (keyImage == null) throw new MoneroError("Must specify key image to check if frozen");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_image", keyImage);
            var resp = rpc.SendJsonRequest("frozen", parameters);
            var result = resp.Result;
            return (bool)result["frozen"] == true;
        }

        public override MoneroTxPriority GetDefaultFeePriority()
        {
            var resp = rpc.SendJsonRequest("get_default_fee_priority");
            var result = resp.Result;
            int priority = ((int)result["priority"]);
            
            return (MoneroTxPriority)priority;
        }

        public override List<MoneroTxWallet> CreateTxs(MoneroTxConfig config)
        {

            // validate, copy, and normalize request
            if (config == null) throw new MoneroError("Send request cannot be null");
            if (config.GetDestinations() == null) throw new MoneroError("Must specify destinations to send to");
            if (config.GetSweepEachSubaddress() != null) throw new MoneroError("Sweep each subaddress expected to ben null");
            if (config.GetBelowAmount() != null) throw new MoneroError("Below amount expected to be null"); 

            if (config.GetCanSplit() == null)
            {
                config = config.Clone();
                config.SetCanSplit(true);
            }
            if (config.GetRelay() == true && IsMultisig()) throw new MoneroError("Cannot relay multisig transaction until co-signed");

            // determine account and subaddresses to send from
            uint accountIdx = config.GetAccountIndex();
            if (accountIdx == null) throw new MoneroError("Must specify the account index to send from");
            List<uint> subaddressIndices = config.GetSubaddressIndices() == null ? null : [.. config.GetSubaddressIndices()]; // fetch all or copy given indices

            // build request parameters
            MoneroJsonRpcParams parameters = [];
            List<Dictionary<string, object>> destinationMaps = [];
            parameters.Add("destinations", destinationMaps);
            foreach (MoneroDestination destination in config.GetDestinations())
            {
                if (destination.GetAddress() == null) throw new Exception("Destination address is not defined");
                if (destination.GetAmount() == null) throw new Exception("Destination amount is not defined");
                Dictionary<string, object> destinationMap = [];
                destinationMap.Add("address", destination.GetAddress());
                destinationMap.Add("amount", destination.GetAmount().ToString());
                destinationMaps.Add(destinationMap);
            }
            if (config.GetSubtractFeeFrom() != null) parameters.Add("subtract_fee_from_outputs", config.GetSubtractFeeFrom());
            parameters.Add("account_index", accountIdx);
            parameters.Add("subaddr_indices", subaddressIndices);
            parameters.Add("payment_id", config.GetPaymentId());
            parameters.Add("do_not_relay", config.GetRelay() != true);
            parameters.Add("priority", config.GetPriority() == null ? null : config.GetPriority());
            parameters.Add("get_tx_hex", true);
            parameters.Add("get_tx_metadata", true);
            if (config.GetCanSplit()) parameters.Add("get_tx_keys", true); // param to get tx key(s) depends if split
            else parameters.Add("get_tx_key", true);

            // cannot apply subtractFeeFrom with `transfer_split` call
            if (config.GetCanSplit() && config.GetSubtractFeeFrom() != null && config.GetSubtractFeeFrom().Count > 0)
            {
                throw new MoneroError("subtractfeefrom transfers cannot be split over multiple transactions yet");
            }

            // send request
            Dictionary<string, object>? result = null;
            try
            {
                var resp = rpc.SendJsonRequest(config.GetCanSplit() ? "transfer_split" : "transfer", parameters);
                result = resp.Result;
            }
            catch (MoneroRpcError err)
            {
                if (err.Message.IndexOf("WALLET_RPC_ERROR_CODE_WRONG_ADDRESS") > -1) throw new MoneroError("Invalid destination address");
                throw err;
            }

            // pre-initialize txs iff present. multisig and view-only wallets will have tx set without transactions
            List<MoneroTxWallet>? txs = null;
            int numTxs = config.GetCanSplit() ? (result.ContainsKey("fee_list") ? ((List<string>)result["fee_list"]).Count : 0) : (result.ContainsKey("fee") ? 1 : 0);
            if (numTxs > 0) txs = [];
            bool copyDestinations = numTxs == 1;
            for (int i = 0; i < numTxs; i++)
            {
                MoneroTxWallet tx = new MoneroTxWallet();
                InitSentTxWallet(config, tx, copyDestinations);
                tx.GetOutgoingTransfer().SetAccountIndex(accountIdx);
                if (subaddressIndices != null && subaddressIndices.Count == 1) tx.GetOutgoingTransfer().SetSubaddressIndices(subaddressIndices);
                txs.Add(tx);
            }

            // notify of changes
            if (config.GetRelay() == true) Poll();

            // initialize tx set from rpc response with pre-initialized txs
            if (config.GetCanSplit()) return ConvertRpcSentTxsToTxSet(result, txs, config).GetTxs();
            else return ConvertRpcTxToTxSet(result, txs == null ? null : txs[0], true, config).GetTxs();
        }

        public override MoneroTxWallet SweepOutput(MoneroTxConfig config)
        {
            // validate request
            if (config.GetSweepEachSubaddress() != null) throw new Exception("Expected sweep each subaddress to be null");
            if (config.GetBelowAmount() != null) throw new Exception("Expected below amount to be null");
            if (config.GetCanSplit() != null) throw new Exception("Splitting is not applicable when sweeping output");
            if (config.GetDestinations() == null || config.GetDestinations().Count != 1 || config.GetDestinations()[0].GetAddress() == null || config.GetDestinations()[0].GetAddress().Length == 0) throw new MoneroError("Must provide exactly one destination address to sweep output to");
            if (config.GetSubtractFeeFrom() != null && config.GetSubtractFeeFrom().Count > 0) throw new MoneroError("Sweep transactions do not support subtracting fees from destinations");

            // build request parameters
            MoneroJsonRpcParams parameters = [];
            parameters.Add("address", config.GetDestinations()[0].GetAddress());
            parameters.Add("account_index", config.GetAccountIndex());
            parameters.Add("subaddr_indices", config.GetSubaddressIndices());
            parameters.Add("key_image", config.GetKeyImage());
            parameters.Add("do_not_relay", config.GetRelay() != true);
            parameters.Add("priority", config.GetPriority() == null ? null : config.GetPriority());
            parameters.Add("payment_id", config.GetPaymentId());
            parameters.Add("get_tx_key", true);
            parameters.Add("get_tx_hex", true);
            parameters.Add("get_tx_metadata", true);

            // send request
            var resp = rpc.SendJsonRequest("sweep_single", parameters);
            var result = resp.Result;

            // notify of changes
            if (config.GetRelay() == true) Poll();

            // build and return tx
            MoneroTxWallet tx = InitSentTxWallet(config, null, true);
            ConvertRpcTxToTxSet(result, tx, true, config);
            tx.GetOutgoingTransfer().GetDestinations()[0].SetAmount(tx.GetOutgoingTransfer().GetAmount()); // initialize destination amount
            return tx;
        }

        public override List<MoneroTxWallet> SweepUnlocked(MoneroTxConfig config)
        {
            // validate request
            if (config == null) throw new MoneroError("Sweep request cannot be null");
            if (config.GetDestinations() == null || config.GetDestinations().Count != 1) throw new MoneroError("Must specify exactly one destination to sweep to");
            if (config.GetDestinations()[0].GetAddress() == null) throw new MoneroError("Must specify destination address to sweep to");
            if (config.GetDestinations()[0].GetAmount() != null) throw new MoneroError("Cannot specify amount in sweep request");
            if (config.GetKeyImage() != null) throw new MoneroError("Key image defined; use sweepOutput() to sweep an output by its key image");
            if (config.GetSubaddressIndices() != null && config.GetSubaddressIndices().Count == 0) config.SetSubaddressIndices((List<uint>)null);
            if (config.GetAccountIndex() == null && config.GetSubaddressIndices() != null) throw new MoneroError("Must specify account index if subaddress indices are specified");
            if (config.GetSubtractFeeFrom() != null && config.GetSubtractFeeFrom().Count > 0) throw new MoneroError("Sweep transactions do not support subtracting fees from destinations");

            // determine account and subaddress indices to sweep; default to all with unlocked balance if not specified
            Dictionary<uint, List<uint>> indices = [];  // java type preserves insertion order
            if (config.GetAccountIndex() != null)
            {
                if (config.GetSubaddressIndices() != null)
                {
                    indices.Add(config.GetAccountIndex(), config.GetSubaddressIndices());
                }
                else
                {
                    List<uint> subaddressIndices = [];
                    indices.Add(config.GetAccountIndex(), subaddressIndices);
                    foreach (MoneroSubaddress subaddress in GetSubaddresses(config.GetAccountIndex()))
                    { // TODO: wallet rpc sweep_all now supports req.subaddr_indices_all
                        if (((ulong)subaddress.GetUnlockedBalance()).CompareTo(0) > 0) subaddressIndices.Add((uint)subaddress.GetIndex());
                    }
                }
            }
            else
            {
                List<MoneroAccount> accounts = GetAccounts(true);
                foreach (MoneroAccount account in accounts)
                {
                    if (account.GetUnlockedBalance().CompareTo(0) > 0)
                    {
                        List<uint> subaddressIndices = [];
                        indices.Add((uint)account.GetIndex(), subaddressIndices);
                        foreach (MoneroSubaddress subaddress in account.GetSubaddresses())
                        {
                            if (((ulong)subaddress.GetUnlockedBalance()).CompareTo(0) > 0) subaddressIndices.Add((uint)subaddress.GetIndex());
                        }
                    }
                }
            }

            // sweep from each account and collect resulting tx sets
            List<MoneroTxWallet> txs = [];
            foreach (uint accountIdx in indices.Keys)
            {

                // copy and modify the original request
                MoneroTxConfig copy = config.Clone();
                copy.SetAccountIndex(accountIdx);
                copy.SetSweepEachSubaddress(false);

                // sweep all subaddresses together // TODO monero-project: can this reveal outputs belong to same wallet?
                if (true != copy.GetSweepEachSubaddress())
                {
                    copy.SetSubaddressIndices(indices[accountIdx]);
                    txs.AddRange(RpcSweepAccount(copy));
                }

                // otherwise sweep each subaddress individually
                else
                {
                    foreach (uint subaddressIdx in indices[accountIdx])
                    {
                        copy.SetSubaddressIndices(subaddressIdx);
                        txs.AddRange(RpcSweepAccount(copy));
                    }
                }
            }

            // notify of changes
            if (config.GetRelay() == true) Poll();
            return txs;
        }

        public override List<MoneroTxWallet> SweepDust(bool relay)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("do_not_relay", !relay);
            var resp = rpc.SendJsonRequest("sweep_dust", parameters);
            if (relay) Poll();
            var result = resp.Result;
            MoneroTxSet txSet = ConvertRpcSentTxsToTxSet(result, null, null);
            if (txSet.GetTxs() == null) return [];
            foreach (MoneroTxWallet tx in txSet.GetTxs())
            {
                tx.SetIsRelayed(relay);
                tx.SetInTxPool(relay);
            }
            return txSet.GetTxs();
        }

        public override List<string> RelayTxs(List<string> txMetadatas)
        {
            if (txMetadatas == null || txMetadatas.Count == 0) throw new MoneroError("Must provide an array of tx metadata to relay");
            List<string> txHashes = [];
            foreach (string txMetadata in txMetadatas)
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("hex", txMetadata);
                var resp = rpc.SendJsonRequest("relay_tx", parameters);
                var result = resp.Result;
                txHashes.Add((string)result["tx_hash"]);
            }
            Poll(); // notify of changes
            return txHashes;
        }

        public override MoneroTxSet DescribeTxSet(MoneroTxSet txSet)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("unsigned_txset", txSet.GetUnsignedTxHex());
            parameters.Add("multisig_txset", txSet.GetMultisigTxHex());
            var resp = rpc.SendJsonRequest("describe_transfer", parameters);
            return ConvertRpcDescribeTransfer(resp.Result);
        }

        public override MoneroTxSet SignTxs(string unsignedTxHex)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("unsigned_txset", unsignedTxHex);
            var resp = rpc.SendJsonRequest("sign_transfer", parameters);
            var result = resp.Result;
            return ConvertRpcSentTxsToTxSet(result, null, null);
        }

        public override List<string> SubmitTxs(string signedTxHex)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("tx_data_hex", signedTxHex);
            var resp = rpc.SendJsonRequest("submit_transfer", parameters);
            Poll();
            var result = resp.Result;
            return (List<string>)result["tx_hash_list"];
        }

        public override string SignMessage(string msg, MoneroMessageSignatureType signatureType, uint accountIdx, uint subaddressIdx)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("data", msg);
            parameters.Add("signature_type", signatureType == MoneroMessageSignatureType.SIGN_WITH_SPEND_KEY ? "spend" : "view");
            parameters.Add("account_index", accountIdx);
            parameters.Add("address_index", subaddressIdx);
            var resp = rpc.SendJsonRequest("sign", parameters);
            var result = resp.Result;
            return (string)result["signature"];
        }

        public override MoneroMessageSignatureResult VerifyMessage(string msg, string address, string signature)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("data", msg);
            parameters.Add("address", address);
            parameters.Add("signature", signature);
            try
            {
                var resp = rpc.SendJsonRequest("verify", parameters);
                var result = resp.Result;
                bool isGood = (bool)result["good"];
                return new MoneroMessageSignatureResult(
                    isGood,
                    !isGood ? null : (bool)result["old"],
                    !isGood || !result.ContainsKey("signature_type") ? null : "view".Equals(result["signature_type"]) ? MoneroMessageSignatureType.SIGN_WITH_VIEW_KEY : MoneroMessageSignatureType.SIGN_WITH_SPEND_KEY,
                    !isGood ? null : ((int)result["version"]));
            }
            catch (MoneroRpcError e)
            {
                if (-2 == e.GetCode()) return new MoneroMessageSignatureResult(false, null, null, null);
                throw e;
            }
        }

        public override string GetTxKey(string txHash)
        {
            try
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("txid", txHash);
                var resp = rpc.SendJsonRequest("get_tx_key", parameters);
                var result = resp.Result;
                return (string)result["tx_key"];
            }
            catch (MoneroRpcError e)
            {
                if (-8 == e.GetCode() && e.Message.IndexOf("TX ID has invalid format") != -1) e = new MoneroRpcError("TX hash has invalid format", e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());  // normalize error message
                throw e;
            }
        }

        public override MoneroCheckTx CheckTxKey(string txHash, string txKey, string address)
        {
            try
            {
                // send request
                MoneroJsonRpcParams parameters = [];
                parameters.Add("txid", txHash);
                parameters.Add("tx_key", txKey);
                parameters.Add("address", address);
                var resp = rpc.SendJsonRequest("check_tx_key", parameters);

                // interpret result
                var result = resp.Result;
                MoneroCheckTx check = new MoneroCheckTx();
                check.SetIsGood(true);
                check.SetNumConfirmations(((ulong)result["confirmations"]));
                check.SetInTxPool((bool)result["in_pool"]);
                check.SetReceivedAmount((ulong)result["received"]);
                return check;
            }
            catch (MoneroRpcError e)
            {
                if (-8 == e.GetCode() && e.Message.IndexOf("TX ID has invalid format") != -1) e = new MoneroRpcError("TX hash has invalid format", e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());  // normalize error message
                throw e;
            }
        }

        public override string GetTxProof(string txHash, string address, string? message = null)
        {
            try
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("txid", txHash);
                parameters.Add("address", address);
                parameters.Add("message", message);
                var resp = rpc.SendJsonRequest("get_tx_proof", parameters);
                var result = resp.Result;
                return (string)result["signature"];
            }
            catch (MoneroRpcError e)
            {
                if (-8 == e.GetCode() && e.Message.IndexOf("TX ID has invalid format") != -1) e = new MoneroRpcError("TX hash has invalid format", e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());  // normalize error message
                throw e;
            }
        }

        public override MoneroCheckTx CheckTxProof(string txHash, string address, string message, string signature)
        {
            try
            {
                // send request
                MoneroJsonRpcParams parameters = [];
                parameters.Add("txid", txHash);
                parameters.Add("address", address);
                parameters.Add("message", message);
                parameters.Add("signature", signature);
                var resp = rpc.SendJsonRequest("check_tx_proof", parameters);

                // interpret response
                var result = resp.Result;
                bool isGood = (bool)result["good"];
                MoneroCheckTx check = new MoneroCheckTx();
                check.SetIsGood(isGood);
                if (isGood)
                {
                    check.SetNumConfirmations(((ulong)result["confirmations"]));
                    check.SetInTxPool((bool)result["in_pool"]);
                    check.SetReceivedAmount((ulong)result["received"]);
                }
                return check;
            }
            catch (MoneroRpcError e)
            {
                if (-1 == e.GetCode() && e.Message.Equals("basic_string")) e = new MoneroRpcError("Must provide signature to check tx proof", -1, null, null);
                if (-8 == e.GetCode() && e.Message.IndexOf("TX ID has invalid format") != -1) e = new MoneroRpcError("TX hash has invalid format", e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());
                throw e;
            }
        }

        public override string GetSpendProof(string txHash, string? message = null)
        {
            try
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("txid", txHash);
                parameters.Add("message", message);
                var resp = rpc.SendJsonRequest("get_spend_proof", parameters);
                var result = resp.Result;
                return (string)result["signature"];
            }
            catch (MoneroRpcError e)
            {
                if (-8 == e.GetCode() && e.Message.IndexOf("TX ID has invalid format") != -1) e = new MoneroRpcError("TX hash has invalid format", e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());  // normalize error message
                throw e;
            }
        }

        public override bool CheckSpendProof(string txHash, string message, string signature)
        {
            try
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("txid", txHash);
                parameters.Add("message", message);
                parameters.Add("signature", signature);
                var resp = rpc.SendJsonRequest("check_spend_proof", parameters);
                var result = resp.Result;
                return (bool)result["good"];
            }
            catch (MoneroRpcError e)
            {
                if (-8 == e.GetCode() && e.Message.IndexOf("TX ID has invalid format") != -1) e = new MoneroRpcError("TX hash has invalid format", e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());  // normalize error message
                throw e;
            }
        }

        public override string GetReserveProofWallet(string message)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("all", true);
            parameters.Add("message", message);
            var resp = rpc.SendJsonRequest("get_reserve_proof", parameters);
            var result = resp.Result;
            return (string)result["signature"];
        }

        public override string GetReserveProofAccount(uint accountIdx, ulong amount, string message)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("account_index", accountIdx);
            parameters.Add("amount", amount.ToString());
            parameters.Add("message", message);
            var resp = rpc.SendJsonRequest("get_reserve_proof", parameters);
            var result = resp.Result;
            return (string)result["signature"];
        }

        public override MoneroCheckReserve CheckReserveProof(string address, string message, string signature)
        {

            // send request
            MoneroJsonRpcParams parameters = [];
            parameters.Add("address", address);
            parameters.Add("message", message);
            parameters.Add("signature", signature);
            var resp = rpc.SendJsonRequest("check_reserve_proof", parameters);
            var result = resp.Result;

            // interpret results
            bool isGood = (bool)result["good"];
            MoneroCheckReserve check = new MoneroCheckReserve();
            check.SetIsGood(isGood);
            if (isGood)
            {
                check.SetTotalAmount((ulong)result["total"]);
                check.SetUnconfirmedSpentAmount((ulong)result["spent"]);
            }
            return check;
        }

        public override List<string> GetTxNotes(List<string> txHashes)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txids", txHashes);
            var resp = rpc.SendJsonRequest("get_tx_notes", parameters);
            var result = resp.Result;
            return (List<string>)result["notes"];
        }

        public override void SetTxNotes(List<string> txHashes, List<string> notes)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txids", txHashes);
            parameters.Add("notes", notes);
            rpc.SendJsonRequest("set_tx_notes", parameters);
        }

        public override List<MoneroAddressBookEntry> GetAddressBookEntries(List<uint>? entryIndices)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("entries", entryIndices);
            var respMap = rpc.SendJsonRequest("get_address_book", parameters);
            var resultMap = respMap.Result;
            List<MoneroAddressBookEntry> entries = [];
            if (!resultMap.ContainsKey("entries")) return entries;
            var entriesMap = (List<Dictionary<string, object>>)resultMap["entries"];
            foreach (var entryMap in entriesMap)
            {
                MoneroAddressBookEntry entry = new MoneroAddressBookEntry(
                        ((uint)entryMap["index"]),
                        (string)entryMap["address"],
                        (string)entryMap["description"],
                        (string)entryMap["payment_id"]
                );
                entries.Add(entry);
            }
            return entries;
        }

        public override uint AddAddressBookEntry(string address, string description)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("address", address);
            parameters.Add("description", description);
            var respMap = rpc.SendJsonRequest("add_address_book", parameters);
            var resultMap = respMap.Result;
            return ((uint)resultMap["index"]);
        }

        public override void EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription, string description)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("index", index);
            parameters.Add("set_address", setAddress);
            parameters.Add("address", address);
            parameters.Add("set_description", setDescription);
            parameters.Add("description", description);
            rpc.SendJsonRequest("edit_address_book", parameters);
        }

        public override void DeleteAddressBookEntry(uint entryIdx)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("index", entryIdx);
            rpc.SendJsonRequest("delete_address_book", parameters);
        }

        public override void TagAccounts(string tag, List<uint> accountIndices)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("tag", tag);
            parameters.Add("accounts", accountIndices);
            rpc.SendJsonRequest("tag_accounts", parameters);
        }

        public override void UntagAccounts(List<uint> accountIndices)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("accounts", accountIndices);
            rpc.SendJsonRequest("untag_accounts", parameters);
        }

        public override List<MoneroAccountTag> GetAccountTags()
        {
            List<MoneroAccountTag> tags = [];
            var respMap = rpc.SendJsonRequest("get_account_tags");
            var resultMap = respMap.Result;
            var accountTagMaps = (List<Dictionary<string, object>>)resultMap["account_tags"];
            if (accountTagMaps != null)
            {
                foreach (var accountTagMap in accountTagMaps)
                {
                    MoneroAccountTag tag = new MoneroAccountTag();
                    tags.Add(tag);
                    tag.SetTag((string)accountTagMap["tag"]);
                    tag.SetLabel((string)accountTagMap["label"]);
                    List<uint> accountIndicesBI = (List<uint>)accountTagMap["accounts"];
                    List<uint> accountIndices = [];
                    foreach (uint idx in accountIndicesBI) accountIndices.Add(idx);
                    tag.SetAccountIndices(accountIndices);
                }
            }
            return tags;
        }

        public override void SetAccountTagLabel(string tag, string label)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("tag", tag);
            parameters.Add("description", label);
            rpc.SendJsonRequest("set_account_tag_description", parameters);
        }

        public override string GetPaymentUri(MoneroTxConfig config)
        {
            if (config == null) throw new MoneroError("Must provide configuration to create a payment URI");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("address", config.GetDestinations()[0].GetAddress());
            parameters.Add("amount", config.GetDestinations()[0].GetAmount() != null ? config.GetDestinations()[0].GetAmount().ToString() : null);
            parameters.Add("payment_id", config.GetPaymentId());
            parameters.Add("recipient_name", config.GetRecipientName());
            parameters.Add("tx_description", config.GetNote());
            var resp = rpc.SendJsonRequest("make_uri", parameters);
            var result = resp.Result;
            return (string)result["uri"];
        }

        public override MoneroTxConfig ParsePaymentUri(string uri)
        {
            if (uri == null || uri.Length == 0) throw new MoneroError("Must provide URI to parse");
            MoneroJsonRpcParams parameters = [];
            parameters.Add("uri", uri);
            var resp = rpc.SendJsonRequest("parse_uri", parameters);
            var result = resp.Result;
            var rpcUri = (Dictionary<string, object>)result["uri"];
            MoneroTxConfig config = new MoneroTxConfig().SetAddress((string)rpcUri["address"]).SetAmount((ulong)rpcUri["amount"]);
            config.SetPaymentId((string)rpcUri["payment_id"]);
            config.SetRecipientName((string)rpcUri["recipient_name"]);
            config.SetNote((string)rpcUri["tx_description"]);
            if ("".Equals(config.GetDestinations()[0].GetAddress())) config.GetDestinations()[0].SetAddress(null);
            if ("".Equals(config.GetPaymentId())) config.SetPaymentId(null);
            if ("".Equals(config.GetRecipientName())) config.SetRecipientName(null);
            if ("".Equals(config.GetNote())) config.SetNote(null);
            return config;
        }

        public override string? GetAttribute(string key)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key", key);
            try
            {
                var resp = rpc.SendJsonRequest("get_attribute", parameters);
                var result = resp.Result;
                string value = (string)result["value"];
                return value.Length == 0 ? null : value;
            }
            catch (MoneroRpcError e)
            {
                if (-45 == e.GetCode()) return null;  // -45: attribute not found
                throw e;
            }
        }

        public override void SetAttribute(string key, string val)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key", key);
            parameters.Add("value", val);
            rpc.SendJsonRequest("set_attribute", parameters);
        }

        public override void StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("threads_count", numThreads);
            parameters.Add("do_background_mining", backgroundMining);
            parameters.Add("ignore_battery", ignoreBattery);
            rpc.SendJsonRequest("start_mining", parameters);
        }

        public override void StopMining()
        {
            rpc.SendJsonRequest("stop_mining");
        }

        public override bool IsMultisigImportNeeded()
        {
            var resp = rpc.SendJsonRequest("get_balance");
            var result = resp.Result;
            return true == (bool)result["multisig_import_needed"];
        }

        public override MoneroMultisigInfo GetMultisigInfo()
        {
            var resp = rpc.SendJsonRequest("is_multisig");
            var result = resp.Result;
            MoneroMultisigInfo info = new MoneroMultisigInfo();
            info.SetIsMultisig((bool)result["multisig"]);
            info.SetIsReady((bool)result["ready"]);
            info.SetThreshold(((int)result["threshold"]));
            info.SetNumParticipants(((int)result["total"]));
            return info;
        }

        public override string PrepareMultisig()
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("enable_multisig_experimental", true);
            var resp = rpc.SendJsonRequest("prepare_multisig", parameters);
            addressCache.Clear();
            var result = resp.Result;
            return (string)result["multisig_info"];
        }

        public override string MakeMultisig(List<string> multisigHexes, int threshold, string password)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("multisig_info", multisigHexes);
            parameters.Add("threshold", threshold);
            parameters.Add("password", password);
            var resp = rpc.SendJsonRequest("make_multisig", parameters);
            addressCache.Clear();
            var result = resp.Result;
            return (string)result["multisig_info"];
        }

        public override MoneroMultisigInitResult ExchangeMultisigKeys(List<string> multisigHexes, string password)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("multisig_info", multisigHexes);
            parameters.Add("password", password);
            var resp = rpc.SendJsonRequest("exchange_multisig_keys", parameters);
            addressCache.Clear();
            var result = resp.Result;
            MoneroMultisigInitResult msResult = new MoneroMultisigInitResult();
            msResult.SetAddress((string)result["address"]);
            msResult.SetMultisigHex((string)result["multisig_info"]);
            if (msResult.GetAddress().Length == 0) msResult.SetAddress(null);
            if (msResult.GetMultisigHex().Length == 0) msResult.SetMultisigHex(null);
            return msResult;
        }

        public override string ExportMultisigHex()
        {
            MoneroJsonRpcResponse resp = rpc.SendJsonRequest("export_multisig_info");
            var result = resp.Result;
            return (string)result["info"];
        }

        public override int ImportMultisigHex(List<string> multisigHexes)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("info", multisigHexes);
            var resp = rpc.SendJsonRequest("import_multisig_info", parameters);
            var result = resp.Result;
            return ((int)result["n_outputs"]);
        }

        public override MoneroMultisigSignResult SignMultisigTxHex(string multisigTxHex)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("tx_data_hex", multisigTxHex);
            var resp = rpc.SendJsonRequest("sign_multisig", parameters);
            var result = resp.Result;
            
            if (result == null) {
                throw new MoneroError("Invalid response from sign_multisig: " + resp.ToString());
            }

            MoneroMultisigSignResult signResult = new();
            signResult.SetSignedMultisigTxHex((string)result["tx_data_hex"]);
            signResult.SetTxHashes((List<string>)result["tx_hash_list"]);
            return signResult;
        }

        public override List<string> SubmitMultisigTxHex(string signedMultisigTxHex)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("tx_data_hex", signedMultisigTxHex);
            MoneroJsonRpcResponse resp = rpc.SendJsonRequest("submit_multisig", parameters);
            if (resp.Result == null || !resp.Result.ContainsKey("tx_hash_list"))
            {
                throw new MoneroError("Invalid response from submit_multisig: " + resp.ToString());
            }
            return (List<string>)resp.Result["tx_hash_list"];
        }

        public override void ChangePassword(string oldPassword, string newPassword)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("old_password", oldPassword);
            parameters.Add("new_password", newPassword);
            rpc.SendJsonRequest("change_wallet_password", parameters);
        }

        public override void Save()
        {
            rpc.SendJsonRequest("store");
        }

        public override void Close(bool save)
        {
            base.Close(save);
            Clear();
            MoneroJsonRpcParams parameters = [];
            parameters.Add("autosave_current", save);
            rpc.SendJsonRequest("close_wallet", parameters);
        }

        public override bool IsClosed()
        {
            try
            {
                GetPrimaryAddress();
            }
            catch (MoneroRpcError e) {
                return e.GetCode() == -8 && e.Message.Contains("No wallet file");
            }
            catch
            {
                return false;
            }
            return false;
        }

        #endregion

        #region Private Methods

        private void Clear()
        {
            listeners.Clear();
            RefreshListening();
            addressCache.Clear();
            path = null;
        }

        private void RefreshListening()
        {
            if (rpc.GetZmqUri() == null)
            {
                if (walletPoller == null && listeners.Count > 0) walletPoller = new MoneroWalletPoller(this, syncPeriodInMs);
                if (walletPoller != null) walletPoller.SetIsPolling(listeners.Count > 0);
            }
            //else
            //{
            //    if (zmqListener == null && listeners.size() > 0) zmqListener = new WalletRpcZmqListener();
            //    if (zmqListener != null) zmqListener.setIsPolling(listeners.size() > 0);
            //}
        }

        private void Poll() { throw new NotImplementedException(); }

        private Dictionary<uint, List<uint>> GetAccountIndices(bool getSubaddressIndices)
        {
            var indices = new Dictionary<uint, List<uint>>();
            foreach (MoneroAccount account in GetAccounts())
            {
                var accountIdx = account.GetIndex();
                if (accountIdx == null) continue;

                indices.Add((uint)accountIdx, getSubaddressIndices ? GetSubaddressIndices((uint)accountIdx) : null);
            }
            return indices;
        }

        private List<uint> GetSubaddressIndices(uint accountIdx)
        {
            List<uint> subaddressIndices = [];
            MoneroJsonRpcParams parameters = [];
            parameters.Add("account_index", accountIdx);
            var resp = rpc.SendJsonRequest("get_address", parameters);
            var result = resp.Result;
            foreach (var address in (List<Dictionary<string, object>>)result["addresses"])
            {
                subaddressIndices.Add(((uint)address["address_index"]));
            }
            return subaddressIndices;
        }

        private List<MoneroKeyImage> RpcExportKeyImages(bool all)
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("all", all);
            var resp = rpc.SendJsonRequest("export_key_images", parameters);
            var result = resp.Result;
            List<MoneroKeyImage> images = [];
            if (!result.ContainsKey("signed_key_images")) return images;
            foreach (var rpcImage in (List<Dictionary<string, object>>)result["signed_key_images"])
            {
                images.Add(new MoneroKeyImage((string)rpcImage["key_image"], (string)rpcImage["signature"]));
            }
            return images;
        }

        private List<ulong> GetBalances(uint? accountIdx, uint? subaddressIdx)
        {
            if (accountIdx == null)
            {
                if (subaddressIdx != null) throw new MoneroError("Must provide account index with subaddress index");
                ulong balance = 0;
                ulong unlockedBalance = 0;
                foreach (MoneroAccount account in GetAccounts())
                {
                    balance += (ulong)account.GetBalance();
                    unlockedBalance += (ulong)account.GetUnlockedBalance();
                }
                return new List<ulong> { balance, unlockedBalance };
            }
            else
            {
                MoneroJsonRpcParams parameters = [];
                parameters.Add("account_index", accountIdx);
                parameters.Add("address_indices", subaddressIdx == null ? null : new List<uint?> { subaddressIdx });
                var resp = rpc.SendJsonRequest("get_balance", parameters);
                var result = resp.Result;
                if (subaddressIdx == null) return new List<ulong> { (ulong)result["balance"], (ulong)result["unlocked_balance"] };
                else
                {
                    var rpcBalancesPerSubaddress = (List<Dictionary<string, object>>)result["per_subaddress"];
                    return new List<ulong> { (ulong)rpcBalancesPerSubaddress[0]["balance"], (ulong)rpcBalancesPerSubaddress[0]["unlocked_balance"] };
                }
            }
        }

        private List<MoneroTransfer> GetTransfersAux(MoneroTransferQuery query)
        {
            // copy and normalize query up to block
            if (query == null) query = new MoneroTransferQuery();
            else
            {
                if (query.GetTxQuery() == null) query = query.Clone();
                else
                {
                    MoneroTxQuery _txQuery = query.GetTxQuery().Clone();
                    if (query.GetTxQuery().GetTransferQuery() == query) query = _txQuery.GetTransferQuery();
                    else
                    {
                        if (query.GetTxQuery().GetTransferQuery() != null) throw new MoneroError("Transfer query's tx query must be circular reference or null");
                        query = query.Clone();
                        query.SetTxQuery(_txQuery);
                    }
                }
            }
            if (query.GetTxQuery() == null) query.SetTxQuery(new MoneroTxQuery());
            MoneroTxQuery txQuery = query.GetTxQuery();

            // build params for get_transfers rpc call
            MoneroJsonRpcParams parameters = [];
            bool canBeConfirmed = txQuery.IsConfirmed() != false && txQuery.InTxPool() != true && txQuery.IsFailed() != true && txQuery.IsRelayed() != false;
            bool canBeInTxPool = txQuery.IsConfirmed() != true && txQuery.InTxPool() != false && txQuery.IsFailed() != true && txQuery.GetHeight() == null && txQuery.GetMaxHeight() == null && txQuery.IsLocked() != false;
            bool canBeIncoming = query.IsIncoming() != false && query.IsOutgoing() != true && query.HasDestinations() != true;
            bool canBeOutgoing = query.IsOutgoing() != false && query.IsIncoming() != true;

            // check if fetching pool txs contradicted by configuration
            if (txQuery.InTxPool() == true && !canBeInTxPool)
            {
                throw new MoneroError("Cannot fetch pool transactions because it contradicts configuration");
            }

            parameters.Add("in", canBeIncoming && canBeConfirmed);
            parameters.Add("out", canBeOutgoing && canBeConfirmed);
            parameters.Add("pool", canBeIncoming && canBeInTxPool);
            parameters.Add("pending", canBeOutgoing && canBeInTxPool);
            parameters.Add("failed", txQuery.IsFailed() != false && txQuery.IsConfirmed() != true && txQuery.InTxPool() != true);
            if (txQuery.GetMinHeight() != null)
            {
                if (txQuery.GetMinHeight() > 0) parameters.Add("min_height", txQuery.GetMinHeight() - 1); // TODO monero-project: wallet2::get_payments() min_height is exclusive, so manually offset to match intended range (issues #5751, #5598)
            else parameters.Add("min_height", txQuery.GetMinHeight());
            }
            if (txQuery.GetMaxHeight() != null) parameters.Add("max_height", txQuery.GetMaxHeight());
            parameters.Add("filter_by_height", txQuery.GetMinHeight() != null || txQuery.GetMaxHeight() != null);
            if (query.GetAccountIndex() == null)
            {
                if (!(query.GetSubaddressIndex() == null && query.GetSubaddressIndices() == null)) throw new MoneroError("Filter specifies a subaddress index but not an account index");
                parameters.Add("all_accounts", true);
            }
            else
            {
                parameters.Add("account_index", query.GetAccountIndex());

                // set subaddress indices param
                HashSet<uint> subaddressIndices = new HashSet<uint>();
                if (query.GetSubaddressIndex() != null) subaddressIndices.Add((uint)query.GetSubaddressIndex());
                if (query.GetSubaddressIndices() != null)
                {
                    foreach (uint subaddressIdx in query.GetSubaddressIndices()) subaddressIndices.Add(subaddressIdx);
                }
                if (subaddressIndices.Count > 0) parameters.Add("subaddr_indices", subaddressIndices);
            }

            // cache unique txs and blocks
            var txMap = new Dictionary<string, MoneroTxWallet>();
            var blockMap = new Dictionary<ulong, MoneroBlock>();

            // build txs using `get_transfers`
            var resp = rpc.SendJsonRequest("get_transfers", parameters);
            var result = resp.Result;
            foreach (string key in result.Keys)
            {
                foreach (var rpcTx in ((List<Dictionary<string, object>>)result[key]))
                {
                    MoneroTxWallet tx = ConvertRpcTxWithTransfer(rpcTx, null, null, null);
                    if (tx.IsConfirmed() == true) if (tx.GetBlock().GetTxs().Contains(tx) != true) throw new MoneroError("Tx not in block");
                    //        if (tx.GetId().equals("38436c710dfbebfb24a14cddfd430d422e7282bbe94da5e080643a1bd2880b44")) {
                    //          System.out.println(rpcTx);
                    //          System.out.println(tx.GetOutgoingAmount().compareTo(BigInteger.valueOf(0)) == 0);
                    //        }

                    // replace transfer amount with destination sum
                    // TODO monero-wallet-rpc: confirmed tx from/to same account has amount 0 but cached transfers
                    if (tx.GetOutgoingTransfer() != null && tx.IsRelayed() == true && tx.IsFailed() != true &&
                        tx.GetOutgoingTransfer().GetDestinations() != null && tx.GetOutgoingAmount() == 0)
                    {
                        MoneroOutgoingTransfer outgoingTransfer = tx.GetOutgoingTransfer();
                        ulong transferTotal = 0;
                        foreach (MoneroDestination destination in outgoingTransfer.GetDestinations()) transferTotal += (ulong)destination.GetAmount();
                        tx.GetOutgoingTransfer().SetAmount(transferTotal);
                    }

                    // merge tx
                    MergeTx(tx, txMap, blockMap);
                }
            }

            // sort txs by block height
            List<MoneroTxWallet> txs = [.. txMap.Values];
            txs.Sort(new MoneroTxHeightComparer());

            // filter and return transfers
            List<MoneroTransfer> transfers = [];
            foreach (MoneroTxWallet tx in txs)
            {

                // tx is not incoming/outgoing unless already set
                if (tx.IsIncoming() == null) tx.SetIsIncoming(false);
                if (tx.IsOutgoing() == null) tx.SetIsOutgoing(false);

                // sort incoming transfers
                if (tx.GetIncomingTransfers() != null) tx.GetIncomingTransfers().Sort(new MoneroIncomingTransferComparer());

                // collect queried transfers, erase if excluded
                transfers.AddRange(tx.FilterTransfers(query));

                // remove excluded txs from block
                if (tx.GetBlock() != null && tx.GetOutgoingTransfer() == null && tx.GetIncomingTransfers() == null)
                {
                    tx.GetBlock().GetTxs().Remove(tx);
                }
            }
            return transfers;
        }

        private List<MoneroOutputWallet> GetOutputsAux(MoneroOutputQuery query)
        {
            // copy and normalize query up to block
            if (query == null) query = new MoneroOutputQuery();
            else
            {
                if (query.GetTxQuery() == null) query = query.Clone();
                else
                {
                    MoneroTxQuery txQuery = query.GetTxQuery().Clone();
                    if (query.GetTxQuery().GetOutputQuery() == query) query = txQuery.GetOutputQuery();
                    else
                    {
                        if (query.GetTxQuery().GetOutputQuery() != null) throw new MoneroError("Output request's tx request must be circular reference or null");
                        query = query.Clone();
                        query.SetTxQuery(txQuery);
                    }
                }
            }
            if (query.GetTxQuery() == null) query.SetTxQuery(new MoneroTxQuery());

            // determine account and subaddress indices to be queried
            Dictionary<uint, List<uint>> indices = [];
            if (query.GetAccountIndex() != null)
            {
                HashSet<uint> subaddressIndices = new HashSet<uint>();
                if (query.GetSubaddressIndex() != null) subaddressIndices.Add((uint)query.GetSubaddressIndex());
                if (query.GetSubaddressIndices() != null) foreach (uint subaddressIdx in query.GetSubaddressIndices()) subaddressIndices.Add(subaddressIdx);
                indices.Add((uint)query.GetAccountIndex(), subaddressIndices.Count == 0 ? null : [.. subaddressIndices]);  // null will fetch from all subaddresses
            }
            else
            {
                if (query.GetSubaddressIndex() != null) throw new MoneroError("Request specifies a subaddress index but not an account index");
                if (!(query.GetSubaddressIndices() == null || query.GetSubaddressIndices().Count == 0)) throw new MoneroError("Request specifies subaddress indices but not an account index");
                indices = GetAccountIndices(false);  // fetch all account indices without subaddresses
            }

            // cache unique txs and blocks
            Dictionary<string, MoneroTxWallet> txMap = [];
            Dictionary<ulong, MoneroBlock> blockMap = [];

            // collect txs with outputs for each indicated account using `incoming_transfers` rpc call
            MoneroJsonRpcParams parameters = [];
            string transferType;
            if (true == query.IsSpent()) transferType = "unavailable";
            else if (false == query.IsSpent()) transferType = "available";
            else transferType = "all";
            parameters.Add("transfer_type", transferType);
            parameters.Add("verbose", true);
            foreach (uint accountIdx in indices.Keys)
            {
    
                // send request
                parameters.Add("account_index", accountIdx);
                parameters.Add("subaddr_indices", indices[accountIdx]);
                var resp = rpc.SendJsonRequest("incoming_transfers", parameters);
                var result = resp.Result;

                // convert response to txs with outputs and merge
                if (!result.ContainsKey("transfers")) continue;
                foreach (var rpcOutput in (List<Dictionary<string, object>>)result["transfers"])
                {
                    MoneroTxWallet tx = ConvertRpcTxWithOutput(rpcOutput);
                    MergeTx(tx, txMap, blockMap);
                }
            }

            // sort txs by block height
            List<MoneroTxWallet> txs = [.. txMap.Values];
            txs.Sort(new MoneroTxHeightComparer());

            // collect queried outputs
            List<MoneroOutputWallet> outputs = [];
            foreach (MoneroTxWallet tx in txs)
            {

                // sort outputs
                if (tx.GetOutputs() != null) tx.GetOutputs().Sort(new MoneroOutputComparer());

                // collect queried outputs, erase if excluded
                outputs.AddRange(tx.filterOutputsWallet(query));

                // remove excluded txs from block
                if (tx.GetOutputs() == null && tx.GetBlock() != null) tx.GetBlock().GetTxs().Remove(tx);
            }
            return outputs;
        }

        private List<MoneroTxWallet> RpcSweepAccount(MoneroTxConfig config)
        {
            // validate request
            if (config == null) throw new MoneroError("Sweep request cannot be null");
            if (config.GetAccountIndex() == null) throw new MoneroError("Must specify an account index to sweep from");
            if (config.GetDestinations() == null || config.GetDestinations().Count != 1) throw new MoneroError("Must specify exactly one destination to sweep to");
            if (config.GetDestinations()[0].GetAddress() == null) throw new MoneroError("Must specify destination address to sweep to");
            if (config.GetDestinations()[0].GetAmount() != null) throw new MoneroError("Cannot specify amount in sweep request");
            if (config.GetKeyImage() != null) throw new MoneroError("Key image defined; use sweepOutput() to sweep an output by its key image");
            if (config.GetSubaddressIndices() != null && config.GetSubaddressIndices().Count == 0) throw new MoneroError("Empty list given for subaddresses indices to sweep");
            if (true == config.GetSweepEachSubaddress()) throw new MoneroError("Cannot sweep each subaddress with RPC `sweep_all`");
            if (config.GetSubtractFeeFrom() != null && config.GetSubtractFeeFrom().Count > 0) throw new MoneroError("Sweeping output does not support subtracting fees from destinations");

            // sweep from all subaddresses if not otherwise defined
            if (config.GetSubaddressIndices() == null)
            {
                config.SetSubaddressIndices([]);
                foreach (MoneroSubaddress subaddress in GetSubaddresses(config.GetAccountIndex()))
                {
                    config.GetSubaddressIndices().Add((uint)subaddress.GetIndex());
                }
            }
            if (config.GetSubaddressIndices().Count == 0) throw new MoneroError("No subaddresses to sweep from");

            // common request params
            bool relay = config.GetRelay() == true;
            MoneroJsonRpcParams parameters = [];
            parameters.Add("account_index", config.GetAccountIndex());
            parameters.Add("subaddr_indices", config.GetSubaddressIndices());
            parameters.Add("address", config.GetDestinations()[0].GetAddress());
            parameters.Add("priority", config.GetPriority() == null ? null : config.GetPriority());
            parameters.Add("payment_id", config.GetPaymentId());
            parameters.Add("do_not_relay", !relay);
            parameters.Add("below_amount", config.GetBelowAmount());
            parameters.Add("get_tx_keys", true);
            parameters.Add("get_tx_hex", true);
            parameters.Add("get_tx_metadata", true);

            // invoke wallet rpc `sweep_all`
            var resp = rpc.SendJsonRequest("sweep_all", parameters);
            var result = resp.Result;

            // initialize txs from response
            MoneroTxSet txSet = ConvertRpcSentTxsToTxSet(result, null, config);

            // initialize remaining known fields
            foreach (MoneroTxWallet tx in txSet.GetTxs())
            {
                tx.SetIsLocked(true);
                tx.SetIsConfirmed(false);
                tx.SetNumConfirmations(0l);
                tx.SetRelay(relay);
                tx.SetInTxPool(relay);
                tx.SetIsRelayed(relay);
                tx.SetIsMinerTx(false);
                tx.SetIsFailed(false);
                tx.SetRingSize(MoneroUtils.RING_SIZE);
                MoneroOutgoingTransfer transfer = tx.GetOutgoingTransfer();
                transfer.SetAccountIndex(config.GetAccountIndex());
                if (config.GetSubaddressIndices().Count == 1) transfer.SetSubaddressIndices([.. config.GetSubaddressIndices()]);
                MoneroDestination destination = new MoneroDestination(config.GetDestinations()[0].GetAddress(), transfer.GetAmount());
                transfer.SetDestinations([destination]);
                tx.SetPaymentId(config.GetPaymentId());
                if (tx.GetUnlockTime() == null) tx.SetUnlockTime(0);
                if (tx.GetRelay() == true)
                {
                    if (tx.GetLastRelayedTimestamp() == null) tx.SetLastRelayedTimestamp((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());  // TODO (monero-wallet-rpc): provide timestamp on response; unconfirmed timestamps vary
                    if (tx.IsDoubleSpendSeen() == null) tx.SetIsDoubleSpendSeen(false);
                }
            }
            return txSet.GetTxs();
        }

        #endregion

        #region Private Static

        private static MoneroTxQuery Decontextualize(MoneroTxQuery query)
        {
            query.SetIsIncoming(null);
            query.SetIsOutgoing(null);
            query.SetTransferQuery(null);
            query.SetInputQuery(null);
            query.SetOutputQuery(null);
            return query;
        }

        private static bool IsContextual(MoneroTransferQuery query)
        {
            if (query == null) return false;
            if (query.GetTxQuery() == null) return false;
            if (query.GetTxQuery().IsIncoming() != null) return true;       // requires context of all transfers
            if (query.GetTxQuery().IsOutgoing() != null) return true;
            if (query.GetTxQuery().GetInputQuery() != null) return true;    // requires context of inputs
            if (query.GetTxQuery().GetOutputQuery() != null) return true;   // requires context of outputs
            return false;
        }

        private static bool IsContextual(MoneroOutputQuery query)
        {
            if (query == null) return false;
            if (query.GetTxQuery() == null) return false;
            if (query.GetTxQuery().IsIncoming() != null) return true;       // requires context of all transfers
            if (query.GetTxQuery().IsOutgoing() != null) return true;
            if (query.GetTxQuery().GetTransferQuery() != null) return true; // requires context of transfers
            return false;
        }

        private static MoneroAccount ConvertRpcAccount(Dictionary<string, object> rpcAccount)
        {
            MoneroAccount account = new MoneroAccount();
            foreach (string key in rpcAccount.Keys)
            {
                Object val = rpcAccount[key];
                if (key.Equals("account_index")) account.SetIndex(((uint)val));
                else if (key.Equals("balance")) account.SetBalance((ulong)val);
                else if (key.Equals("unlocked_balance")) account.SetUnlockedBalance((ulong)val);
                else if (key.Equals("base_address")) account.SetPrimaryAddress((string)val);
                else if (key.Equals("tag")) account.SetTag((string)val);
                else if (key.Equals("label")) { } // label belongs to first subaddress
                //else LOGGER.warning("ignoring unexpected account field: " + key + ": " + val);
            }
            if ("".Equals(account.GetTag())) account.SetTag(null);
            return account;
        }

        private static MoneroSubaddress ConvertRpcSubaddress(Dictionary<string, object> rpcSubaddress)
        {
            MoneroSubaddress subaddress = new MoneroSubaddress();
            foreach (string key in rpcSubaddress.Keys)
            {
                Object val = rpcSubaddress[key];
                if (key.Equals("account_index")) subaddress.SetAccountIndex(((uint)val));
                else if (key.Equals("address_index")) subaddress.SetIndex(((uint)val));
                else if (key.Equals("address")) subaddress.SetAddress((string)val);
                else if (key.Equals("balance")) subaddress.SetBalance((ulong)val);
                else if (key.Equals("unlocked_balance")) subaddress.SetUnlockedBalance((ulong)val);
                else if (key.Equals("num_unspent_outputs")) subaddress.SetNumUnspentOutputs(((ulong)val));
                else if (key.Equals("label")) { if (!"".Equals(val)) subaddress.SetLabel((string)val); }
                else if (key.Equals("used")) subaddress.SetIsUsed((bool)val);
                else if (key.Equals("blocks_to_unlock")) subaddress.SetNumBlocksToUnlock(((ulong)val));
                else if (key.Equals("time_to_unlock")) { } // ignoring
                //else LOGGER.warning("ignoring unexpected subaddress field: " + key + ": " + val);
            }
            return subaddress;
        }

        private static MoneroTxWallet InitSentTxWallet(MoneroTxConfig config, MoneroTxWallet tx, bool copyDestinations)
        {
            if (tx == null) tx = new MoneroTxWallet();
            bool relay = config.GetRelay() == true;
            tx.SetIsOutgoing(true);
            tx.SetIsConfirmed(false);
            tx.SetNumConfirmations(0);
            tx.SetInTxPool(relay);
            tx.SetRelay(relay);
            tx.SetIsRelayed(relay);
            tx.SetIsMinerTx(false);
            tx.SetIsFailed(false);
            tx.SetIsLocked(true);
            tx.SetRingSize(MoneroUtils.RING_SIZE);
            MoneroOutgoingTransfer transfer = new MoneroOutgoingTransfer().SetTx(tx);
            if (config.GetSubaddressIndices() != null && config.GetSubaddressIndices().Count == 1) transfer.SetSubaddressIndices([.. config.GetSubaddressIndices()]); // we know src subaddress indices iff request specifies 1
            if (copyDestinations)
            {
                List<MoneroDestination> destCopies = [];
                foreach (MoneroDestination dest in config.GetDestinations()) destCopies.Add(dest.Clone());
                transfer.SetDestinations(destCopies);
            }
            tx.SetOutgoingTransfer(transfer);
            tx.SetPaymentId(config.GetPaymentId());
            if (tx.GetUnlockTime() == null) tx.SetUnlockTime(0);
            if (tx.GetRelay() == true)
            {
                if (tx.GetLastRelayedTimestamp() == null) tx.SetLastRelayedTimestamp((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());  // TODO (monero-wallet-rpc): provide timestamp on response; unconfirmed timestamps vary
                if (tx.IsDoubleSpendSeen() == null) tx.SetIsDoubleSpendSeen(false);
            }
            return tx;
        }

        private static MoneroTxSet ConvertRpcTxSet(Dictionary<string, object> rpcMap)
        {
            MoneroTxSet txSet = new MoneroTxSet();
            txSet.SetMultisigTxHex((string)rpcMap["multisig_txset"]);
            txSet.SetUnsignedTxHex((string)rpcMap["unsigned_txset"]);
            txSet.SetSignedTxHex((string)rpcMap["signed_txset"]);
            if (txSet.GetMultisigTxHex() != null && txSet.GetMultisigTxHex().Length == 0) txSet.SetMultisigTxHex(null);
            if (txSet.GetUnsignedTxHex() != null && txSet.GetUnsignedTxHex().Length == 0) txSet.SetUnsignedTxHex(null);
            if (txSet.GetSignedTxHex() != null && txSet.GetSignedTxHex().Length == 0) txSet.SetSignedTxHex(null);
            return txSet;
        }

        private static MoneroTxSet ConvertRpcSentTxsToTxSet(Dictionary<string, object> rpcTxs, List<MoneroTxWallet>? txs, MoneroTxConfig config)
        {
            // build shared tx set
            MoneroTxSet txSet = ConvertRpcTxSet(rpcTxs);

            // get number of txs
            string? numTxsKey = rpcTxs.ContainsKey("fee_list") ? "fee_list" : rpcTxs.ContainsKey("tx_hash_list") ? "tx_hash_list" : null;
            int numTxs = numTxsKey == null ? 0 : ((List<object>) rpcTxs[numTxsKey]).Count;

            // done if rpc response contains no txs
            if (numTxs == 0)
            {
                if (txs != null) throw new Exception("Cannot provide txs when rpc response contains no transactions");
                return txSet;
            }

            // initialize txs if none given
            if (txs != null) txSet.SetTxs(txs);
            else
            {
                txs = [];
                for (int i = 0; i < numTxs; i++) txs.Add(new MoneroTxWallet());
            }
            foreach (MoneroTxWallet tx in txs)
            {
                tx.SetTxSet(txSet);
                tx.SetIsOutgoing(true);
            }
            txSet.SetTxs(txs);

            // initialize txs from rpc lists
            foreach (string key in rpcTxs.Keys)
            {
                Object val = rpcTxs[key];
                if (key.Equals("tx_hash_list"))
                {
                    List<string> hashes = (List<string>)val;
                    for (int i = 0; i < hashes.Count; i++) txs[i].SetHash(hashes[i]);
                }
                else if (key.Equals("tx_key_list"))
                {
                    List<string> keys = (List<string>)val;
                    for (int i = 0; i < keys.Count; i++) txs[i].SetKey(keys[i]);
                }
                else if (key.Equals("tx_blob_list"))
                {
                    List<string> blobs = (List<string>)val;
                    for (int i = 0; i < blobs.Count; i++) txs[i].SetFullHex(blobs[i]);
                }
                else if (key.Equals("tx_metadata_list"))
                {
                    List<string> metadatas = (List<string>)val;
                    for (int i = 0; i < metadatas.Count; i++) txs[i].SetMetadata(metadatas[i]);
                }
                else if (key.Equals("fee_list"))
                {
                    List<ulong> fees = (List<ulong>)val;
                    for (int i = 0; i < fees.Count; i++) txs[i].SetFee(fees[i]);
                }
                else if (key.Equals("amount_list"))
                {
                    List<ulong> amounts = (List<ulong>)val;
                    for (int i = 0; i < amounts.Count; i++)
                    {
                        if (txs[i].GetOutgoingTransfer() == null) txs[i].SetOutgoingTransfer(new MoneroOutgoingTransfer().SetTx(txs[i]));
                        txs[i].GetOutgoingTransfer().SetAmount(amounts[i]);
                    }
                }
                else if (key.Equals("weight_list"))
                {
                    List<ulong> weights = (List<ulong>)val;
                    for (int i = 0; i < weights.Count; i++) txs[i].SetWeight((ulong)weights[i]);
                }
                else if (key.Equals("multisig_txset") || key.Equals("unsigned_txset") || key.Equals("signed_txset"))
                {
                    // handled elsewhere
                }
                else if (key.Equals("spent_key_images_list"))
                {
                    var inputKeyImagesList = (List<Dictionary<string, object>>)val;
                    for (int i = 0; i < inputKeyImagesList.Count; i++)
                    {
                        if(txs[i].GetInputs() != null) throw new Exception("Expected null inputs");
                        txs[i].SetInputsWallet([]);
                        foreach (string inputKeyImage in (List<string>)inputKeyImagesList[i]["key_images"])
                        {
                            txs[i].GetInputs().Add(new MoneroOutputWallet().SetKeyImage(new MoneroKeyImage().SetHex(inputKeyImage)).SetTx(txs[i]));
                        }
                    }
                }
                else if (key.Equals("amounts_by_dest_list"))
                {
                    var amountsByDestList = (List<Dictionary<string, object>>)val;
                    int destinationIdx = 0;
                    for (int txIdx = 0; txIdx < amountsByDestList.Count; txIdx++)
                    {
                        List<ulong> amountsByDest = (List<ulong>)amountsByDestList[txIdx]["amounts"];
                        if (txs[txIdx].GetOutgoingTransfer() == null) txs[txIdx].SetOutgoingTransfer(new MoneroOutgoingTransfer().SetTx(txs[txIdx]));
                        txs[txIdx].GetOutgoingTransfer().SetDestinations([]);
                        foreach (ulong amount in amountsByDest)
                        {
                            if (config.GetDestinations().Count == 1) txs[txIdx].GetOutgoingTransfer().GetDestinations().Add(new MoneroDestination(config.GetDestinations()[0].GetAddress(), amount)); // sweeping can create multiple withone address
                            else txs[txIdx].GetOutgoingTransfer().GetDestinations().Add(new MoneroDestination(config.GetDestinations()[destinationIdx++].GetAddress(), amount));
                        }
                    }
                }
                else
                {
                    MoneroUtils.Log(0, "ignoring unexpected transaction list field: " + key + ": " + val);
                }
            }

            return txSet;
        }

        private static MoneroTxSet ConvertRpcTxToTxSet(Dictionary<string, object> rpcTx, MoneroTxWallet tx, bool isOutgoing, MoneroTxConfig config)
        {
            MoneroTxSet txSet = ConvertRpcTxSet(rpcTx);
            txSet.SetTxs([ConvertRpcTxWithTransfer(rpcTx, tx, isOutgoing, config).SetTxSet(txSet)]);
            return txSet;
        }

        private static MoneroTxWallet ConvertRpcTxWithTransfer(Dictionary<string, object> rpcTx, MoneroTxWallet tx, bool? isOutgoing, MoneroTxConfig config)
        {  // TODO: change everything to safe set

            // initialize tx to return
            if (tx == null) tx = new MoneroTxWallet();

            // initialize tx state from rpc type
            if (rpcTx.ContainsKey("type")) isOutgoing = DecodeRpcType((string)rpcTx["type"], tx);
            else if (isOutgoing == null) throw new MoneroError("Must indicate if tx is outgoing (true) xor incoming (false) since unknown");

            // TODO: safe set
            // initialize remaining fields  TODO: seems this should be part of common function with DaemonRpc._convertRpcTx
            MoneroBlockHeader? header = null;
            MoneroTransfer? transfer = null;
            foreach (string key in rpcTx.Keys)
            {
                Object val = rpcTx[key];
                if (key.Equals("txid")) tx.SetHash((string)val);
                else if (key.Equals("tx_hash")) tx.SetHash((string)val);
                else if (key.Equals("fee")) tx.SetFee((ulong)val);
                else if (key.Equals("note")) { if (!"".Equals(val)) tx.SetNote((string)val); }
                else if (key.Equals("tx_key")) tx.SetKey((string)val);
                else if (key.Equals("type")) { } // type already handled
                else if (key.Equals("tx_size")) tx.SetSize(((ulong)val));
                else if (key.Equals("unlock_time")) tx.SetUnlockTime(((ulong)val));
                else if (key.Equals("weight")) tx.SetWeight(((ulong)val));
                else if (key.Equals("locked")) tx.SetIsLocked((bool)val);
                else if (key.Equals("tx_blob")) tx.SetFullHex((string)val);
                else if (key.Equals("tx_metadata")) tx.SetMetadata((string)val);
                else if (key.Equals("double_spend_seen")) tx.SetIsDoubleSpendSeen((bool)val);
                else if (key.Equals("block_height") || key.Equals("height"))
                {
                    if (tx.IsConfirmed() == true)
                    {
                        if (header == null) header = new MoneroBlockHeader();
                        header.SetHeight(((ulong)val));
                    }
                }
                else if (key.Equals("timestamp"))
                {
                    if (tx.IsConfirmed() == true)
                    {
                        if (header == null) header = new MoneroBlockHeader();
                        header.SetTimestamp(((ulong)val));
                    }
                    else
                    {
                        // timestamp of unconfirmed tx is current request time
                    }
                }
                else if (key.Equals("confirmations")) tx.SetNumConfirmations(((ulong)val));
                else if (key.Equals("suggested_confirmations_threshold"))
                {
                    if (transfer == null)
                    {
                        transfer = (isOutgoing == true ? new MoneroOutgoingTransfer() : new MoneroIncomingTransfer());
                        transfer.SetTx(tx);
                    }
                    if (isOutgoing != true) ((MoneroIncomingTransfer)transfer).SetNumSuggestedConfirmations(((ulong)val));
                }
                else if (key.Equals("amount"))
                {
                    if (transfer == null)
                    {
                        transfer = (isOutgoing == true ? new MoneroOutgoingTransfer() : new MoneroIncomingTransfer());
                        transfer.SetTx(tx);
                    }
                    transfer.SetAmount((ulong)val);
                }
                else if (key.Equals("amounts")) { }  // ignoring, amounts sum to amount
                else if (key.Equals("address"))
                {
                    if (isOutgoing != true)
                    {
                        if (transfer == null) transfer = new MoneroIncomingTransfer().SetTx(tx);
                        ((MoneroIncomingTransfer)transfer).SetAddress((string)val);
                    }
                }
                else if (key.Equals("payment_id"))
                {
                    if (!"".Equals(val) && !MoneroTxWallet.DEFAULT_PAYMENT_ID.Equals(val)) tx.SetPaymentId((string)val);  // default is undefined
                }
                else if (key.Equals("subaddr_index"))
                {
                    if (!rpcTx.ContainsKey("subaddr_indices")) throw new MoneroError("subaddress indices not found"); // handled by subaddr_indices
                }
                else if (key.Equals("subaddr_indices"))
                {
                    if (transfer == null)
                    {
                        transfer = (isOutgoing == true ? new MoneroOutgoingTransfer() : new MoneroIncomingTransfer());
                        transfer.SetTx(tx);
                    }
                    List<Dictionary<string, uint>> rpcIndices = (List<Dictionary<string, uint>>)val;
                    transfer.SetAccountIndex(rpcIndices[0]["major"]);
                    if (isOutgoing == true)
                    {
                        List<uint> subaddressIndices = [];
                        foreach (Dictionary<string, uint> rpcIndex in rpcIndices) subaddressIndices.Add(rpcIndex["minor"]);
                        ((MoneroOutgoingTransfer)transfer).SetSubaddressIndices(subaddressIndices);
                    }
                    else
                    {
                        if (rpcIndices.Count != 1) throw new MoneroError("Expected exactly one subaddress index for incoming transfer, but got " + rpcIndices.Count);
                        ((MoneroIncomingTransfer)transfer).SetSubaddressIndex(rpcIndices[0]["minor"]);
                    }
                }
                else if (key.Equals("destinations") || key.Equals("recipients"))
                {
                    if (isOutgoing != true) throw new MoneroError("Expected outgoing transfer, but got incoming transfer");
                    List<MoneroDestination> destinations = [];
                    foreach (var rpcDestination in (List<Dictionary<string, object>>)val)
                    {
                        MoneroDestination destination = new MoneroDestination();
                        destinations.Add(destination);
                        foreach (string destinationKey in rpcDestination.Keys)
                        {
                            if (destinationKey.Equals("address")) destination.SetAddress((string)rpcDestination[destinationKey]);
                            else if (destinationKey.Equals("amount")) destination.SetAmount((ulong)rpcDestination[destinationKey]);
                            else throw new MoneroError("Unrecognized transaction destination field: " + destinationKey);
                        }
                    }
                    if (transfer == null) transfer = new MoneroOutgoingTransfer().SetTx(tx);
                    ((MoneroOutgoingTransfer)transfer).SetDestinations(destinations);
                }
                else if (key.Equals("multisig_txset") && val != null) { }  // handled elsewhere; this method only builds a tx wallet
                else if (key.Equals("unsigned_txset") && val != null) { }  // handled elsewhere; this method only builds a tx wallet
                else if (key.Equals("amount_in")) tx.SetInputSum((ulong)val);
                else if (key.Equals("amount_out")) tx.SetOutputSum((ulong)val);
                else if (key.Equals("change_address")) tx.SetChangeAddress("".Equals(val) ? null : (string)val);
                else if (key.Equals("change_amount")) tx.SetChangeAmount((ulong)val);
                else if (key.Equals("dummy_outputs")) tx.SetNumDummyOutputs(((uint)val));
                else if (key.Equals("extra")) tx.SetExtraHex((string)val);
                else if (key.Equals("ring_size")) tx.SetRingSize(((uint)val));
                else if (key.Equals("spent_key_images"))
                {
                    List<string> inputKeyImages = (List<string>)((Dictionary<string, object>)val)["key_images"];
                    if (tx.GetInputs() != null) throw new MoneroError("Expected null inputs");
                    tx.SetInputs([]);
                    foreach (string inputKeyImage in inputKeyImages)
                    {
                        tx.GetInputs().Add(new MoneroOutputWallet().SetKeyImage(new MoneroKeyImage().SetHex(inputKeyImage)).SetTx(tx));
                    }
                }
                else if (key.Equals("amounts_by_dest"))
                {
                    if (isOutgoing != true) throw new MoneroError("Expected outgoing transfer, but got incoming transfer");
                    List<ulong> amountsByDest = (List<ulong>)((Dictionary<string, object>)val)["amounts"];
                    if (config.GetDestinations().Count != amountsByDest.Count) throw new MoneroError("Expected " + config.GetDestinations().Count + " destinations, but got " + amountsByDest.Count);
                    if (transfer == null) transfer = new MoneroOutgoingTransfer().SetTx(tx);
                    ((MoneroOutgoingTransfer)transfer).SetDestinations([]);
                    for (int i = 0; i < config.GetDestinations().Count; i++)
                    {
                        ((MoneroOutgoingTransfer)transfer).GetDestinations().Add(new MoneroDestination(config.GetDestinations()[i].GetAddress(), amountsByDest[i]));
                    }
                }
                else MoneroUtils.Log(0, "ignoring unexpected transaction field with transfer: " + key + ": " + val);
            }

            // link block and tx
            if (header != null) tx.SetBlock(new MoneroBlock(header).SetTxs([tx]));

            // initialize final fields
            if (transfer != null)
            {
                if (tx.IsConfirmed() == null) tx.SetIsConfirmed(false);
                if (transfer.GetTx().IsConfirmed() == false) tx.SetNumConfirmations(0l);
                if (isOutgoing == true)
                {
                    tx.SetIsOutgoing(true);
                    if (tx.GetOutgoingTransfer() != null)
                    {
                        if (((MoneroOutgoingTransfer)transfer).GetDestinations() != null) tx.GetOutgoingTransfer().SetDestinations(null); // overwrite to avoid Reconcile error TODO: remove after >18.3.1 when amounts_by_dest supported
                        tx.GetOutgoingTransfer().Merge(transfer);
                    }
                    else tx.SetOutgoingTransfer((MoneroOutgoingTransfer)transfer);
                }
                else
                {
                    tx.SetIsIncoming(true);
                    tx.SetIncomingTransfers([(MoneroIncomingTransfer)transfer]);
                }
            }

            // return initialized transaction
            return tx;
        }

        private static MoneroTxWallet ConvertRpcTxWithOutput(Dictionary<string, object> rpcOutput)
        {

            // initialize tx
            MoneroTxWallet tx = new MoneroTxWallet();
            tx.SetIsConfirmed(true);
            tx.SetIsRelayed(true);
            tx.SetIsFailed(false);

            // initialize output
            MoneroOutputWallet output = new MoneroOutputWallet().SetTx(tx);
            foreach (string key in rpcOutput.Keys)
            {
                Object val = rpcOutput[key];
                if (key.Equals("amount")) output.SetAmount((ulong)val);
                else if (key.Equals("spent")) output.SetIsSpent((bool)val);
                else if (key.Equals("key_image")) { if (!"".Equals(val)) output.SetKeyImage(new MoneroKeyImage((string)val)); }
                else if (key.Equals("global_index")) output.SetIndex(((ulong)val));
                else if (key.Equals("tx_hash")) tx.SetHash((string)val);
                else if (key.Equals("unlocked")) tx.SetIsLocked(!(bool)val);
                else if (key.Equals("frozen")) output.SetIsFrozen((bool)val);
                else if (key.Equals("pubkey")) output.SetStealthPublicKey((string)val);
                else if (key.Equals("subaddr_index"))
                {
                    var rpcIndices = (Dictionary<string, uint>)val;
                    output.SetAccountIndex(rpcIndices["major"]);
                    output.SetSubaddressIndex(rpcIndices["minor"]);
                }
                else if (key.Equals("block_height"))
                {
                    ulong height = ((ulong)val);
                    tx.SetBlock(new MoneroBlock().SetHeight(height).SetTxs([tx]));
                }
                else MoneroUtils.Log(0, "ignoring unexpected transaction field with output: " + key + ": " + val);
            }

            // initialize tx with output
            List<MoneroOutput> outputs = [];
            outputs.Add(output);
            tx.SetOutputs(outputs);
            return tx;
        }

        private static MoneroTxSet ConvertRpcDescribeTransfer(Dictionary<string, object> rpcDescribeTransferResult)
        {
            MoneroTxSet txSet = new MoneroTxSet();
            foreach (string key in rpcDescribeTransferResult.Keys)
            {
                Object val = rpcDescribeTransferResult[key];
                if (key.Equals("desc"))
                {
                    txSet.SetTxs([]);
                    foreach (var txMap in (List<Dictionary<string, object>>)val)
                    {
                        MoneroTxWallet tx = ConvertRpcTxWithTransfer(txMap, null, true, null);
                        tx.SetTxSet(txSet);
                        txSet.GetTxs().Add(tx);
                    }
                }
                else if (key.Equals("summary")) { } // TODO: support tx set summary fields?
                else MoneroUtils.Log(0, "ignoring unexpected describe transfer field: " + key + ": " + val);
            }
            return txSet;
        }

        private static bool DecodeRpcType(string rpcType, MoneroTxWallet tx)
        {
            bool isOutgoing;
            if (rpcType.Equals("in"))
            {
                isOutgoing = false;
                tx.SetIsConfirmed(true);
                tx.SetInTxPool(false);
                tx.SetIsRelayed(true);
                tx.SetRelay(true);
                tx.SetIsFailed(false);
                tx.SetIsMinerTx(false);
            }
            else if (rpcType.Equals("out"))
            {
                isOutgoing = true;
                tx.SetIsConfirmed(true);
                tx.SetInTxPool(false);
                tx.SetIsRelayed(true);
                tx.SetRelay(true);
                tx.SetIsFailed(false);
                tx.SetIsMinerTx(false);
            }
            else if (rpcType.Equals("pool"))
            {
                isOutgoing = false;
                tx.SetIsConfirmed(false);
                tx.SetInTxPool(true);
                tx.SetIsRelayed(true);
                tx.SetRelay(true);
                tx.SetIsFailed(false);
                tx.SetIsMinerTx(false);  // TODO: but could it be?
            }
            else if (rpcType.Equals("pending"))
            {
                isOutgoing = true;
                tx.SetIsConfirmed(false);
                tx.SetInTxPool(true);
                tx.SetIsRelayed(true);
                tx.SetRelay(true);
                tx.SetIsFailed(false);
                tx.SetIsMinerTx(false);
            }
            else if (rpcType.Equals("block"))
            {
                isOutgoing = false;
                tx.SetIsConfirmed(true);
                tx.SetInTxPool(false);
                tx.SetIsRelayed(true);
                tx.SetRelay(true);
                tx.SetIsFailed(false);
                tx.SetIsMinerTx(true);
            }
            else if (rpcType.Equals("failed"))
            {
                isOutgoing = true;
                tx.SetIsConfirmed(false);
                tx.SetInTxPool(false);
                tx.SetIsRelayed(true);
                tx.SetRelay(true);
                tx.SetIsFailed(true);
                tx.SetIsMinerTx(false);
            }
            else
            {
                throw new MoneroError("Unrecognized transfer type: " + rpcType);
            }
            return isOutgoing;
        }

        private static void MergeTx(MoneroTxWallet tx, Dictionary<string, MoneroTxWallet> txMap, Dictionary<ulong, MoneroBlock> blockMap)
        {
            if (tx.GetHash() == null) throw new MoneroError("Cannot merge transaction without hash");
            if (tx.GetHeight() == null) throw new MoneroError("Cannot merge transaction without height");
            var txHash = (string)tx.GetHash();
            var txHeight = (ulong)tx.GetHeight();
            // merge tx
            MoneroTxWallet aTx = txMap[txHash];
            if (aTx == null) txMap.Add(txHash, tx); // cache new tx
            else aTx.Merge(tx); // merge with existing tx

            // merge tx's block if confirmed
            if (tx.GetHeight() != null)
            {
                MoneroBlock aBlock = blockMap[txHeight];
                if (aBlock == null) blockMap.Add(txHeight, tx.GetBlock()); // cache new block
                else aBlock.Merge(tx.GetBlock()); // merge with existing block
            }
        }

        public override MoneroNetworkType GetNetworkType()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}