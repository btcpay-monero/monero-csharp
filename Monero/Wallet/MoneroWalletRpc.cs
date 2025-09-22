using System.Diagnostics;

using Monero.Common;
using Monero.Wallet.Common;

using Newtonsoft.Json.Linq;

using MoneroJsonRpcParams = System.Collections.Generic.Dictionary<string, object?>;

namespace Monero.Wallet;

public class MoneroWalletRpc : IMoneroWallet
{
    // class variables
    private const int ErrorCodeInvalidPaymentId = -5; // invalid payment id error code

    private const ulong
        DefaultSyncPeriodInMs =
            20000; // default period between syncs in ms (defined by DEFAULT_AUTO_REFRESH_PERIOD in wallet_rpc_server.cpp)

    private readonly Dictionary<uint, Dictionary<uint, string?>?>
        _addressCache = []; // cache static addresses to reduce requests

    private readonly List<MoneroWalletListener> _listeners = [];

    private readonly Process? _process = null; // process running monero-wallet-rpc if applicable
    private readonly MoneroRpcConnection _rpc; // rpc connection to monero-wallet-rpc

    // instance variables
    private readonly SemaphoreSlim _syncSem = new(1, 1); // lock for synchronizing sync requests
    private MoneroRpcConnection? _daemonConnection; // current daemon connection (unknown/null until explicitly set)
    private string? _path; // wallet's path identifier
    private ulong _syncPeriodInMs = DefaultSyncPeriodInMs; // period between syncs in ms (default 20000)

    private MoneroWalletPoller? _walletPoller; // listener which polls monero-wallet-rpc

    public MoneroWalletRpc(MoneroRpcConnection connection)
    {
        _rpc = connection;
        CheckRpcConnection();
    }

    public MoneroWalletRpc(string url, string? username = null, string? password = null)
    {
        _rpc = new MoneroRpcConnection(url, username, password);
        CheckRpcConnection();
    }

    public List<MoneroWalletListener> GetListeners()
    {
        return [.. _listeners];
    }

    public Process? GetProcess()
    {
        return _process;
    }

    #region RPC Wallet Methods

    public async Task<MoneroWalletRpc> OpenWallet(MoneroWalletConfig config)
    {
        // validate config
        if (config == null)
        {
            throw new MoneroError("Must provide configuration of wallet to open");
        }

        if (string.IsNullOrEmpty(config.GetPath()))
        {
            throw new MoneroError("Filename is not initialized");
        }
        // TODO: ensure other fields are uninitialized?

        // open wallet on rpc server
        Dictionary<string, object?> parameters = [];
        parameters.Add("filename", config.GetPath());
        parameters.Add("password", config.GetPassword() == null ? "" : config.GetPassword());
        await _rpc.SendJsonRequest("open_wallet", parameters);
        Clear();
        _path = config.GetPath();

        if (config.GetServer() != null)
        {
            await SetDaemonConnection(config.GetServer(), true, null);
        }

        return this;
    }

    public async Task<MoneroWalletRpc> OpenWallet(string path, string? password = null)
    {
        return await OpenWallet(new MoneroWalletConfig().SetPath(path).SetPassword(password));
    }

    public async Task<MoneroWalletRpc> CreateWallet(MoneroWalletConfig config)
    {
        // validate config
        if (config == null)
        {
            throw new MoneroError("Must specify config to create wallet");
        }

        if (config.GetNetworkType() != null)
        {
            throw new MoneroError("Cannot specify network type when creating RPC wallet");
        }

        if (config.GetSeed() != null && (config.GetPrimaryAddress() != null || config.GetPrivateViewKey() != null ||
                                         config.GetPrivateSpendKey() != null))
        {
            throw new MoneroError("Wallet can be initialized with a seed or keys but not both");
        }

        if (config.GetAccountLookahead() != null || config.GetSubaddressLookahead() != null)
        {
            throw new MoneroError(
                "monero-wallet-rpc does not support creating wallets with subaddress lookahead over rpc");
        }

        // create wallet
        if (config.GetSeed() != null)
        {
            await CreateWalletFromSeed(config);
        }
        else if (config.GetPrivateSpendKey() != null || config.GetPrimaryAddress() != null)
        {
            await CreateWalletFromKeys(config);
        }
        else
        {
            await CreateWalletRandom(config);
        }

        if (config.GetServer() != null)
        {
            await SetDaemonConnection(config.GetServer(), true, null);
        }

        return this;
    }

    private async Task<MoneroWalletRpc> CreateWalletRandom(MoneroWalletConfig? config)
    {
        if (config == null)
        {
            throw new MoneroError("Config is null");
        }

        // validate and normalize config
        config = config.Clone();
        if (config.GetSeedOffset() != null)
        {
            throw new MoneroError("Cannot specify seed offset when creating random wallet");
        }

        if (config.GetRestoreHeight() != null)
        {
            throw new MoneroError("Cannot specify restore height when creating random wallet");
        }

        if (config.GetSaveCurrent() == false)
        {
            throw new MoneroError("Current wallet is saved automatically when creating random wallet");
        }

        if (string.IsNullOrEmpty(config.GetPath()))
        {
            throw new MoneroError("Wallet name is not initialized");
        }

        if (string.IsNullOrEmpty(config.GetLanguage()))
        {
            config.SetLanguage(IMoneroWallet.DefaultLanguage);
        }

        // send request
        Dictionary<string, object?> parameters = [];
        parameters.Add("filename", config.GetPath());
        parameters.Add("password", config.GetPassword());
        parameters.Add("language", config.GetLanguage());
        try { await _rpc.SendJsonRequest("create_wallet", parameters); }
        catch (MoneroRpcError e) { HandleCreateWalletError(config.GetPath(), e); }

        Clear();
        _path = config.GetPath();
        return this;
    }

    private async Task CreateWalletFromSeed(MoneroWalletConfig config)
    {
        config = config.Clone();
        if (string.IsNullOrEmpty(config.GetLanguage()))
        {
            config.SetLanguage(IMoneroWallet.DefaultLanguage);
        }

        Dictionary<string, object?> parameters = [];
        parameters.Add("filename", config.GetPath());
        parameters.Add("password", config.GetPassword());
        parameters.Add("seed", config.GetSeed());
        parameters.Add("seed_offset", config.GetSeedOffset());
        parameters.Add("restore_height", config.GetRestoreHeight());
        parameters.Add("language", config.GetLanguage());
        parameters.Add("autosave_current", config.GetSaveCurrent());
        parameters.Add("enable_multisig_experimental", config.IsMultisig());

        try { await _rpc.SendJsonRequest("restore_deterministic_wallet", parameters); }
        catch (MoneroRpcError e) { HandleCreateWalletError(config.GetPath(), e); }

        Clear();
        _path = config.GetPath();
    }

    private async Task CreateWalletFromKeys(MoneroWalletConfig config)
    {
        config = config.Clone();
        if (config.GetSeedOffset() != null)
        {
            throw new MoneroError("Cannot specify seed offset when creating wallet from keys");
        }

        if (config.GetRestoreHeight() == null)
        {
            config.SetRestoreHeight(0);
        }

        Dictionary<string, object?> parameters = [];

        parameters.Add("filename", config.GetPath());
        parameters.Add("password", config.GetPassword());
        parameters.Add("address", config.GetPrimaryAddress());
        parameters.Add("viewkey", config.GetPrivateViewKey());
        parameters.Add("spendkey", config.GetPrivateSpendKey());
        parameters.Add("restore_height", config.GetRestoreHeight());
        parameters.Add("autosave_current", config.GetSaveCurrent());

        try { await _rpc.SendJsonRequest("generate_from_keys", parameters); }
        catch (MoneroRpcError e) { HandleCreateWalletError(config.GetPath(), e); }

        Clear();
        _path = config.GetPath();
    }

    private static void HandleCreateWalletError(string? name, MoneroRpcError? e)
    {
        if (e == null)
        {
            throw new MoneroRpcError("Cannot create wallet due an unknown error");
        }

        if (e.Message.Equals("Cannot create wallet. Already exists."))
        {
            throw new MoneroRpcError("Wallet already exists: " + (name ?? "unkown"), e.GetCode(), e.GetRpcMethod(),
                e.GetRpcParams());
        }

        if (e.Message.Equals("Electrum-style word list failed verification"))
        {
            throw new MoneroRpcError("Invalid mnemonic", e.GetCode(), e.GetRpcMethod(), e.GetRpcParams());
        }

        throw e;
    }

    private static void HandleInvalidTxIdError(MoneroRpcError? e)
    {
        if (e == null)
        {
            throw new MoneroError("Cannot parse invalid tx id error");
        }

        if (-8 == e.GetCode() && e.Message.Contains("TX ID has invalid format"))
        {
            e = new MoneroRpcError("TX hash has invalid format", e.GetCode(), e.GetRpcMethod(),
                e.GetRpcParams()); // normalize error message
        }

        throw new MoneroRpcError(e);
    }

    public MoneroRpcConnection GetRpcConnection() { return _rpc; }

    private void CheckRpcConnection()
    {
        if (_rpc.IsConnected() == true)
        {
            return;
        }

        _rpc.CheckConnection(2000).GetAwaiter().GetResult();
    }

    #endregion

    #region Common Wallet Methods

    public void AddListener(MoneroWalletListener listener)
    {
        lock (_listeners)
        {
            if (listener == null)
            {
                throw new MoneroError("Cannot add null listener");
            }

            _listeners.Add(listener);
            RefreshListening();
        }
    }

    public void RemoveListener(MoneroWalletListener listener)
    {
        lock (_listeners)
        {
            _listeners.Remove(listener);
            RefreshListening();
        }
    }

    public async Task<bool> IsViewOnly()
    {
        try
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("key_type", "mnemonic");
            await _rpc.SendJsonRequest("query_key", parameters);
            return false; // key retrieval succeeds if not view-only
        }
        catch (MoneroError e)
        {
            if (-29 == e.GetCode())
            {
                return true; // wallet is view-only
            }

            if (-1 == e.GetCode())
            {
                return false; // wallet is offline but not view-only
            }

            throw;
        }
    }

    public async Task SetDaemonConnection(MoneroRpcConnection? connection, bool? isTrusted, SslOptions? sslOptions)
    {
        if (sslOptions == null)
        {
            sslOptions = new SslOptions();
        }

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
        await _rpc.SendJsonRequest("set_daemon", parameters);
        if (connection != null && !string.IsNullOrEmpty(connection.GetUri()))
        {
            _daemonConnection = new MoneroRpcConnection(connection);
        }
        else
        {
            _daemonConnection = null;
        }
    }

    public Task SetProxyUri(string? uri)
    {
        throw new MoneroError(
            "MoneroWalletRpc.SetProxyUri() not supported. Start monero-wallet-rpc with --proxy instead.");
    }

    public Task<MoneroRpcConnection?> GetDaemonConnection()
    {
        return Task.FromResult(_daemonConnection);
    }

    public async Task<bool> IsConnectedToDaemon()
    {
        try
        {
            await CheckReserveProof(await GetPrimaryAddress(), "",
                ""); // TODO (monero-project): provide better way to know if wallet rpc is connected to daemon
            throw new Exception("check reserve expected to fail");
        }
        catch (MoneroError e)
        {
            return !e.Message.Contains("Failed to connect to daemon");
        }
    }

    public async Task<MoneroVersion> GetVersion()
    {
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_version");
        MoneroJsonRpcParams result = resp.Result!;
        return new MoneroVersion((long?)result["version"], (bool?)result["release"]);
    }

    public Task<string> GetPath()
    {
        return Task.FromResult(_path ?? "");
    }

    public async Task<string> GetSeed()
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_type", "mnemonic");
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("query_key", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["key"]!;
    }

    public Task<string> GetSeedLanguage()
    {
        throw new MoneroError("MoneroWalletRpc.GetSeedLanguage() not supported");
    }

    public async Task<string> GetPrivateViewKey()
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_type", "view_key");
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("query_key", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["key"]!;
    }

    public async Task<string> GetPublicViewKey()
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_type", "public_view_key");
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("query_key", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["key"]!;
    }

    public async Task<string> GetPublicSpendKey()
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_type", "public_spend_key");
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("query_key", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["key"]!;
    }

    public async Task<string> GetPrivateSpendKey()
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_type", "spend_key");
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("query_key", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["key"]!;
    }

    public async Task<string?> GetAddress(uint accountIdx, uint subaddressIdx)
    {
        Dictionary<uint, string?>? subaddressMap = _addressCache.GetValueOrDefault(accountIdx);
        if (subaddressMap == null)
        {
            await GetSubaddresses(accountIdx, true, null); // cache's all addresses at this account
            return await GetAddress(accountIdx, subaddressIdx); // uses cache
        }

        string? address = subaddressMap.GetValueOrDefault(subaddressIdx);
        if (address == null)
        {
            await GetSubaddresses(accountIdx, true, null); // cache's all addresses at this account
            Dictionary<uint, string?>? map = _addressCache.GetValueOrDefault(accountIdx);
            return map?.GetValueOrDefault(subaddressIdx);
        }

        return address;
    }

    public async Task<MoneroSubaddress> GetAddressIndex(string address)
    {
        // fetch result and normalize error if address does not belong to the wallet
        Dictionary<string, object>? result;
        try
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("address", address);
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp =
                await _rpc.SendJsonRequest("get_address_index", parameters);
            result = resp.Result!;
        }
        catch (MoneroRpcError e)
        {
            MoneroUtils.Log(0, e.Message);
            if (-2 == e.GetCode())
            {
                throw new MoneroError(e.Message, e.GetCode());
            }

            throw;
        }

        // convert rpc response
        Dictionary<string, uint?> rpcIndices = ((JObject)result["index"]).ToObject<Dictionary<string, uint?>>()!;
        MoneroSubaddress subaddress = new(address);
        subaddress.SetAccountIndex(rpcIndices["major"]);
        subaddress.SetIndex(rpcIndices["minor"]);
        return subaddress;
    }

    public async Task<MoneroIntegratedAddress> GetIntegratedAddress(string? standardAddress, string? paymentId)
    {
        try
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("standard_address", standardAddress);
            parameters.Add("payment_id", paymentId);
            MoneroJsonRpcResponse<MoneroJsonRpcParams>
                resp = await _rpc.SendJsonRequest("make_integrated_address", parameters);
            MoneroJsonRpcParams result = resp.Result!;
            string integratedAddressStr = (string?)result["integrated_address"]!;
            return await DecodeIntegratedAddress(integratedAddressStr);
        }
        catch (MoneroRpcError e)
        {
            if (e.Message.Contains("Invalid payment ID"))
            {
                throw new MoneroError("Invalid payment ID: " + paymentId, ErrorCodeInvalidPaymentId);
            }

            throw;
        }
    }

    public async Task<MoneroIntegratedAddress> DecodeIntegratedAddress(string integratedAddress)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("integrated_address", integratedAddress);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp =
            await _rpc.SendJsonRequest("split_integrated_address", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return new MoneroIntegratedAddress((string?)result["standard_address"], (string?)result["payment_id"],
            integratedAddress);
    }

    public async Task<ulong> GetHeight()
    {
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_height");
        MoneroJsonRpcParams result = resp.Result!;
        return Convert.ToUInt64(result["height"]);
    }

    public Task<ulong> GetDaemonHeight()
    {
        throw new MoneroError("monero-wallet-rpc does not support getting the chain height");
    }

    public Task<ulong> GetHeightByDate(int year, int month, int day)
    {
        throw new MoneroError("monero-wallet-rpc does not support getting a height by date");
    }

    public async Task<MoneroSyncResult> Sync(ulong? startHeight, MoneroWalletListener? listener)
    {
        if (listener != null)
        {
            throw new MoneroError("Monero Wallet RPC does not support reporting sync progress");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("start_height", startHeight);
        // TODO (monero-project): monero-wallet-rpc hangs at 100% cpu utilization if refresh called concurrently
        await _syncSem.WaitAsync();

        try
        {
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("refresh", parameters);
            await Poll();
            MoneroJsonRpcParams result = resp.Result!;
            _syncSem.Release();

            return new MoneroSyncResult(Convert.ToUInt64(result["blocks_fetched"]), (bool?)result["received_money"]);
        }
        catch (MoneroError err)
        {
            _syncSem.Release();

            if (err.Message.Equals("no connection to daemon"))
            {
                throw new MoneroError("Wallet is not connected to daemon");
            }

            throw;
        }
    }

    public async Task StartSyncing(ulong? syncPeriodInMs)
    {
        // convert ms to seconds for rpc parameter
        ulong syncPeriodInSeconds = (syncPeriodInMs == null ? DefaultSyncPeriodInMs : (ulong)syncPeriodInMs) / 1000;

        // send rpc request
        MoneroJsonRpcParams parameters = [];
        parameters.Add("enable", true);
        parameters.Add("period", syncPeriodInSeconds);
        await _rpc.SendJsonRequest("auto_refresh", parameters);

        // update sync period for poller
        _syncPeriodInMs = syncPeriodInSeconds * 1000;
        if (_walletPoller != null)
        {
            _walletPoller.SetPeriodInMs(_syncPeriodInMs);
        }

        // poll if listening
        await Poll();
    }

    public async Task StopSyncing()
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("enable", false);
        await _rpc.SendJsonRequest("auto_refresh", parameters);
    }

    public async Task ScanTxs(List<string> txHashes)
    {
        if (txHashes == null || txHashes.Count == 0)
        {
            throw new MoneroError("No tx hashes given to scan");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("txids", txHashes);
        await _rpc.SendJsonRequest("scan_tx", parameters);
        await Poll(); // notify of changes
    }

    public async Task RescanSpent()
    {
        await _rpc.SendJsonRequest("rescan_spent");
    }

    public async Task RescanBlockchain()
    {
        await _rpc.SendJsonRequest("rescan_blockchain");
    }

    public async Task<ulong> GetBalance(uint? accountIdx, uint? subaddressIdx)
    {
        return (await GetBalances(accountIdx, subaddressIdx))[0];
    }

    public async Task<ulong> GetUnlockedBalance(uint? accountIdx, uint? subaddressIdx)
    {
        return (await GetBalances(accountIdx, subaddressIdx))[1];
    }

    private static void InitSubaddress(MoneroSubaddress? tgtSubaddress, MoneroSubaddress? subaddress)
    {
        if (tgtSubaddress == null || subaddress == null)
        {
            throw new MoneroError("Cannot initialize null subaddress");
        }

        if (subaddress.GetBalance() != null)
        {
            tgtSubaddress.SetBalance(subaddress.GetBalance());
        }

        if (subaddress.GetUnlockedBalance() != null)
        {
            tgtSubaddress.SetUnlockedBalance(subaddress.GetUnlockedBalance());
        }

        if (subaddress.GetNumUnspentOutputs() != null)
        {
            tgtSubaddress.SetNumUnspentOutputs(subaddress.GetNumUnspentOutputs());
        }

        if (subaddress.GetNumBlocksToUnlock() != null)
        {
            tgtSubaddress.SetNumBlocksToUnlock(subaddress.GetNumBlocksToUnlock());
        }
    }

    public async Task<List<MoneroAccount>> GetAccounts(bool includeSubaddresses, bool skipBalances, string? tag)
    {
        // fetch accounts from rpc
        MoneroJsonRpcParams parameters = [];
        parameters.Add("tag", tag);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_accounts", parameters);
        MoneroJsonRpcParams result = resp.Result!;

        // build account objects and fetch subaddresses per account using get_address
        // TODO monero-wallet-rpc: get_address should support all_accounts so not called once per account
        List<MoneroAccount> accounts = [];
        foreach (MoneroJsonRpcParams rpcAccount in ((JArray)result["subaddress_accounts"]!)
                 .ToObject<List<Dictionary<string, object?>>>()!)
        {
            MoneroAccount account = ConvertRpcAccount(rpcAccount);
            if (includeSubaddresses)
            {
                account.SetSubaddresses(await GetSubaddresses((uint)account.GetIndex()!, true, null));
            }

            accounts.Add(account);
        }

        // fetch and merge fields from get_balance across all accounts
        if (includeSubaddresses && !skipBalances)
        {
            // these fields are not initialized if the subaddress is unused and therefore not returned from `get_balance`
            foreach (MoneroAccount account in accounts)
            {
                foreach (MoneroSubaddress subaddress in account.GetSubaddresses()!)
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
            resp = await _rpc.SendJsonRequest("get_balance", parameters);
            result = resp.Result!;
            if (result.ContainsKey("per_subaddress"))
            {
                foreach (MoneroJsonRpcParams rpcSubaddress in
                         ((JArray)result["per_subaddress"]!).ToObject<List<Dictionary<string, object?>>>()!)
                {
                    MoneroSubaddress subaddress = ConvertRpcSubaddress(rpcSubaddress);

                    // merge info
                    MoneroAccount account = accounts[(int)subaddress.GetAccountIndex()!];
                    if (account.GetIndex() != subaddress.GetAccountIndex())
                    {
                        throw new MoneroError("RPC accounts are out of order"); // would need to switch lookup to loop
                    }

                    MoneroSubaddress tgtSubaddress = account.GetSubaddresses()![(int)subaddress.GetIndex()!];
                    if (tgtSubaddress.GetIndex() != subaddress.GetIndex())
                    {
                        throw new MoneroError("RPC subaddresses are out of order");
                    }

                    InitSubaddress(tgtSubaddress, subaddress);
                }
            }
        }

        // return accounts
        return accounts;
    }

    public async Task<MoneroAccount> CreateAccount(string? label)
    {
        label = string.IsNullOrEmpty(label) ? null : label;
        MoneroJsonRpcParams parameters = [];
        parameters.Add("label", label);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("create_account", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return result == null
            ? throw new InvalidOperationException("RPC response result was null.")
            : new MoneroAccount(Convert.ToUInt32(result["account_index"]), (string?)result["address"], 0, 0, null);
    }

    public async Task<List<MoneroSubaddress>> GetSubaddresses(uint accountIdx,
        bool skipBalances, List<uint>? subaddressIndices)
    {
        // fetch subaddresses
        MoneroJsonRpcParams parameters = [];
        parameters.Add("account_index", accountIdx);
        if (subaddressIndices != null && subaddressIndices.Count > 0)
        {
            parameters.Add("address_index", subaddressIndices);
        }

        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_address", parameters);
        MoneroJsonRpcParams result = resp.Result!;

        // initialize subaddresses
        List<MoneroSubaddress> subaddresses = [];
        foreach (MoneroJsonRpcParams rpcSubaddress in ((JArray)result["addresses"]!)
                 .ToObject<List<Dictionary<string, object?>>>()!)
        {
            MoneroSubaddress subaddress = ConvertRpcSubaddress(rpcSubaddress);
            subaddress.SetAccountIndex(accountIdx);
            subaddresses.Add(subaddress);
        }

        // fetch and initialize subaddress balances
        if (!skipBalances)
        {
            // these fields are not initialized if the subaddress is unused and therefore not returned from `get_balance`
            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                subaddress.SetBalance(0);
                subaddress.SetUnlockedBalance(0);
                subaddress.SetNumUnspentOutputs(0);
                subaddress.SetNumBlocksToUnlock(0);
            }

            // fetch and initialize balances
            resp = await _rpc.SendJsonRequest("get_balance", parameters);
            result = resp.Result!;
            if (result.ContainsKey("per_subaddress"))
            {
                foreach (MoneroJsonRpcParams rpcSubaddress in
                         ((JArray)result["per_subaddress"]!).ToObject<List<Dictionary<string, object?>>>()!)
                {
                    MoneroSubaddress subaddress = ConvertRpcSubaddress(rpcSubaddress);

                    // transfer info to existing subaddress object
                    foreach (MoneroSubaddress tgtSubaddress in subaddresses)
                    {
                        if (tgtSubaddress.GetIndex() != subaddress.GetIndex())
                        {
                            continue; // skip to subaddress with same index
                        }

                        InitSubaddress(tgtSubaddress, subaddress);
                    }
                }
            }
        }

        // cache addresses
        Dictionary<uint, string?>? subaddressMap = _addressCache.GetValueOrDefault(accountIdx);
        if (subaddressMap == null)
        {
            subaddressMap = [];
            _addressCache.Add(accountIdx, subaddressMap);
        }

        foreach (MoneroSubaddress subaddress in subaddresses)
        {
            subaddressMap[(uint)subaddress.GetIndex()!] = subaddress.GetAddress();
        }

        // return results
        return subaddresses;
    }

    public async Task<MoneroSubaddress> CreateSubaddress(uint accountIdx, string? label)
    {
        // send request
        MoneroJsonRpcParams parameters = [];
        parameters.Add("account_index", accountIdx);
        parameters.Add("label", label);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("create_address", parameters);
        MoneroJsonRpcParams result = resp.Result!;

        // build subaddress object
        MoneroSubaddress subaddress = new();
        subaddress.SetAccountIndex(accountIdx);
        subaddress.SetIndex(Convert.ToUInt32(result["address_index"]));
        subaddress.SetAddress((string?)result["address"]);
        subaddress.SetLabel(label);
        subaddress.SetBalance(0);
        subaddress.SetUnlockedBalance(0);
        subaddress.SetNumUnspentOutputs(0);
        subaddress.SetIsUsed(false);
        subaddress.SetNumBlocksToUnlock(0);
        return subaddress;
    }

    public async Task SetAccountLabel(uint accountIdx, uint subaddressIdx, string label)
    {
        MoneroJsonRpcParams parameters = [];
        Dictionary<string, uint> idx = [];
        idx.Add("major", accountIdx);
        idx.Add("minor", subaddressIdx);
        parameters.Add("index", idx);
        parameters.Add("label", label);
        await _rpc.SendJsonRequest("label_address", parameters);
    }

    public async Task<List<MoneroTxWallet>> GetTxs(MoneroTxQuery? query)
    {
        // copy and normalize a query
        query = query == null ? new MoneroTxQuery() : query.Clone();
        if (query.GetInputQuery() != null)
        {
            query.GetInputQuery()!.SetTxQuery(query);
        }

        if (query.GetOutputQuery() != null)
        {
            query.GetOutputQuery()!.SetTxQuery(query);
        }

        // temporarily disable transfer and output queries in order to collect all tx information
        MoneroTransferQuery? transferQuery = query.GetTransferQuery();
        MoneroOutputQuery? inputQuery = query.GetInputQuery();
        MoneroOutputQuery? outputQuery = query.GetOutputQuery();
        query.SetTransferQuery(null);
        query.SetInputQuery(null);
        query.SetOutputQuery(null);

        // fetch all transfers that meet tx query
        List<MoneroTransfer> transfers =
            await GetTransfersAux(new MoneroTransferQuery().SetTxQuery(Decontextualize(query.Clone())));

        // collect unique txs from transfers while retaining order
        List<MoneroTxWallet> txs = [];
        HashSet<MoneroTxWallet> txsSet = [];
        foreach (MoneroTransfer transfer in transfers)
        {
            if (!txsSet.Contains(transfer.GetTx()!))
            {
                txs.Add(transfer.GetTx()!);
                txsSet.Add(transfer.GetTx()!);
            }
        }

        // cache types into maps for merging and lookup
        Dictionary<string, MoneroTxWallet> txMap = [];
        Dictionary<ulong, MoneroBlock?> blockMap = [];
        foreach (MoneroTxWallet tx in txs)
        {
            MergeTx(tx, txMap, blockMap);
        }

        // fetch and merge outputs if queried
        if (query.GetIncludeOutputs() == true || outputQuery != null)
        {
            // fetch outputs
            MoneroOutputQuery outputQueryAux =
                (outputQuery != null ? outputQuery.Clone() : new MoneroOutputQuery()).SetTxQuery(
                    Decontextualize(query.Clone()));
            List<MoneroOutputWallet> outputs = await GetOutputsAux(outputQueryAux);

            // merge output txs one time while retaining order
            HashSet<MoneroTxWallet> outputTxs = [];
            foreach (MoneroOutputWallet output in outputs)
            {
                if (!outputTxs.Contains(output.GetTx()!))
                {
                    MergeTx(output.GetTx()!, txMap, blockMap);
                    outputTxs.Add(output.GetTx()!);
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
            if (query.MeetsCriteria(tx))
            {
                txsQueried.Add(tx);
            }
            else if (tx.GetBlock() != null)
            {
                tx.GetBlock()!.GetTxs()!.Remove(tx);
            }
        }

        txs = txsQueried;

        // special case: re-fetch txs if inconsistency caused by needing to make multiple rpc calls
        foreach (MoneroTxWallet tx in txs)
        {
            if ((tx.IsConfirmed() == true && tx.GetBlock() == null) ||
                (tx.IsConfirmed() != true && tx.GetBlock() != null))
            {
                MoneroUtils.Log(1, "Inconsistency detected building txs from multiple rpc calls, re-fetching");
                return await GetTxs(query);
            }
        }

        // order txs if tx hashes given
        if (query.GetHashes() != null && query.GetHashes()!.Count > 0)
        {
            Dictionary<string, MoneroTxWallet?> txsById = []; // store txs in temporary map for sorting
            foreach (MoneroTxWallet tx in txs)
            {
                txsById.Add(tx.GetHash()!, tx);
            }

            List<MoneroTxWallet> orderedTxs = [];
            foreach (string txHash in query.GetHashes()!)
            {
                MoneroTxWallet? value = txsById.GetValueOrDefault(txHash);
                if (value != null)
                {
                    orderedTxs.Add(value);
                }
            }

            txs = orderedTxs;
        }

        return txs;
    }

    public async Task<List<MoneroTransfer>> GetTransfers(MoneroTransferQuery? query)
    {
        // copy and normalize query up to block
        query = NormalizeTransferQuery(query);

        // get transfers directly if query does not require tx context (other transfers, outputs)
        if (!IsContextual(query))
        {
            return await GetTransfersAux(query);
        }

        // otherwise get txs with full models to fulfill a query
        List<MoneroTransfer> transfers = [];
        query.GetTxQuery()!.SetTransferQuery(query);
        foreach (MoneroTxWallet tx in await GetTxs(query.GetTxQuery()))
        {
            transfers.AddRange(tx.FilterTransfers(query));
        }

        return transfers;
    }

    public async Task<List<MoneroOutputWallet>> GetOutputs(MoneroOutputQuery? query)
    {
        // get outputs directly if query does not require tx context (other outputs, transfers)
        if (!IsContextual(query))
        {
            return await GetOutputsAux(query);
        }

        // otherwise get txs with full models to fulfill a query
        List<MoneroOutputWallet> outputs = [];

        if (query == null)
        {
            throw new MoneroError("Query is null");
        }

        foreach (MoneroTxWallet tx in await GetTxs(query.GetTxQuery()))
        {
            outputs.AddRange(tx.FilterOutputsWallet(query));
        }

        return outputs;
    }

    public async Task<string> ExportOutputs(bool all)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("all", all);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("export_outputs", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["outputs_data_hex"]!;
    }

    public async Task<int> ImportOutputs(string outputsHex)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("outputs_data_hex", outputsHex);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("import_outputs", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (int)result["num_imported"]!;
    }

    public async Task<List<MoneroKeyImage>> ExportKeyImages(bool all)
    {
        List<MoneroKeyImage> ret;
        MoneroJsonRpcParams parameters = [];
        parameters.Add("all", all);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("export_key_images", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        List<MoneroKeyImage> images = [];
        if (!result.TryGetValue("signed_key_images", out object? value))
        {
            ret = images;
        }
        else
        {
            foreach (MoneroJsonRpcParams rpcImage in ((JArray)value!)
                     .ToObject<List<Dictionary<string, object?>>>()!)
            {
                images.Add(new MoneroKeyImage((string?)rpcImage["key_image"], (string?)rpcImage["signature"]));
            }

            ret = images;
        }

        return ret;
    }

    public async Task<MoneroKeyImageImportResult> ImportKeyImages(List<MoneroKeyImage> keyImages)
    {
        // convert key images to rpc parameter
        List<Dictionary<string, object?>> rpcKeyImages = [];
        foreach (MoneroKeyImage keyImage in keyImages)
        {
            Dictionary<string, object?> rpcKeyImage = [];
            rpcKeyImage.Add("key_image", keyImage.GetHex());
            rpcKeyImage.Add("signature", keyImage.GetSignature());
            rpcKeyImages.Add(rpcKeyImage);
        }

        // send rpc request
        MoneroJsonRpcParams parameters = [];
        parameters.Add("signed_key_images", rpcKeyImages);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("import_key_images", parameters);
        MoneroJsonRpcParams result = resp.Result!;

        // build and return result
        MoneroKeyImageImportResult importResult = new();
        importResult.SetHeight((ulong?)result["height"]);
        importResult.SetSpentAmount((ulong?)result["spent"]);
        importResult.SetUnspentAmount((ulong?)result["unspent"]);
        return importResult;
    }

    public async Task FreezeOutput(string keyImage)
    {
        if (keyImage == null)
        {
            throw new MoneroError("Must specify key image to freeze");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_image", keyImage);
        await _rpc.SendJsonRequest("freeze", parameters);
    }

    public async Task ThawOutput(string keyImage)
    {
        if (keyImage == null)
        {
            throw new MoneroError("Must specify key image to thaw");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_image", keyImage);
        await _rpc.SendJsonRequest("thaw", parameters);
    }

    public async Task<bool> IsOutputFrozen(string keyImage)
    {
        if (keyImage == null)
        {
            throw new MoneroError("Must specify key image to check if frozen");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("key_image", keyImage);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("frozen", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (bool)result["frozen"]!;
    }

    public async Task<MoneroTxPriority> GetDefaultFeePriority()
    {
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_default_fee_priority");
        MoneroJsonRpcParams result = resp.Result!;
        int priority = (int)result["priority"]!;

        return (MoneroTxPriority)priority;
    }

    public async Task<List<MoneroTxWallet>> CreateTxs(MoneroTxConfig config)
    {
        // validate, copy, and normalize request
        if (config == null)
        {
            throw new MoneroError("Send request cannot be null");
        }

        if (config.GetDestinations() == null)
        {
            throw new MoneroError("Must specify destinations to send to");
        }

        if (config.GetSweepEachSubaddress() != null)
        {
            throw new MoneroError("Sweep each subaddress expected to ben null");
        }

        if (config.GetBelowAmount() != null)
        {
            throw new MoneroError("Below amount expected to be null");
        }

        if (config.GetCanSplit() == null)
        {
            config = config.Clone();
            config.SetCanSplit(true);
        }

        if (config.GetRelay() == true && await IsMultisig())
        {
            throw new MoneroError("Cannot relay multisig transaction until co-signed");
        }

        // determine account and subaddresses to send from
        uint? accountIdx = config.GetAccountIndex();
        if (accountIdx == null)
        {
            throw new MoneroError("Must specify the account index to send from");
        }

        List<uint>? subaddressIndices =
            config.GetSubaddressIndices() == null
                ? null
                : [.. config.GetSubaddressIndices()!]; // fetch all or copy given indices

        // build request parameters
        MoneroJsonRpcParams parameters = [];
        List<Dictionary<string, object?>> destinationMaps = [];
        parameters.Add("destinations", destinationMaps);
        foreach (MoneroDestination destination in config.GetDestinations()!)
        {
            if (destination.GetAddress() == null)
            {
                throw new Exception("Destination address is not defined");
            }

            if (destination.GetAmount() == null)
            {
                throw new Exception("Destination amount is not defined");
            }

            Dictionary<string, object?> destinationMap = [];
            destinationMap.Add("address", destination.GetAddress());
            destinationMap.Add("amount", destination.GetAmount().ToString());
            destinationMaps.Add(destinationMap);
        }

        if (config.GetSubtractFeeFrom() != null)
        {
            parameters.Add("subtract_fee_from_outputs", config.GetSubtractFeeFrom());
        }

        parameters.Add("account_index", accountIdx);
        parameters.Add("subaddr_indices", subaddressIndices);
        parameters.Add("payment_id", config.GetPaymentId());
        parameters.Add("do_not_relay", !config.GetRelay());
        parameters.Add("priority", config.GetPriority() == null ? null : config.GetPriority());
        parameters.Add("get_tx_hex", true);
        parameters.Add("get_tx_metadata", true);
        if (config.GetCanSplit() == true)
        {
            parameters.Add("get_tx_keys", true); // param to get tx key(s) depends if split
        }
        else
        {
            parameters.Add("get_tx_key", true);
        }

        // cannot apply subtractFeeFrom with `transfer_split` call
        if (config.GetCanSplit() == true && config.GetSubtractFeeFrom() != null &&
            config.GetSubtractFeeFrom()!.Count > 0)
        {
            throw new MoneroError("subtractfeefrom transfers cannot be split over multiple transactions yet");
        }

        // send request
        Dictionary<string, object?>? result;
        try
        {
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp =
                await _rpc.SendJsonRequest(config.GetCanSplit() == true ? "transfer_split" : "transfer", parameters);
            result = resp.Result!;
        }
        catch (MoneroRpcError err)
        {
            if (err.Message.IndexOf("WALLET_RPC_ERROR_CODE_WRONG_ADDRESS", StringComparison.Ordinal) > -1)
            {
                throw new MoneroError("Invalid destination address");
            }

            throw;
        }

        // pre-initialize txs iff present. multisig and view-only wallets will have tx set without transactions
        List<MoneroTxWallet> txs = [];
        int numTxs = config.GetCanSplit() == true
            ? result.ContainsKey("fee_list") ? ((JArray)result["fee_list"]!).Count : 0
            : result.ContainsKey("fee")
                ? 1
                : 0;
        if (numTxs > 0)
        {
            txs = [];
        }

        bool copyDestinations = numTxs == 1;
        for (int i = 0; i < numTxs; i++)
        {
            MoneroTxWallet tx = new();
            InitSentTxWallet(config, tx, copyDestinations);
            tx.GetOutgoingTransfer()!.SetAccountIndex(accountIdx);
            if (subaddressIndices != null && subaddressIndices.Count == 1)
            {
                tx.GetOutgoingTransfer()!.SetSubaddressIndices(subaddressIndices);
            }

            txs.Add(tx);
        }

        // notify of changes
        if (config.GetRelay() == true)
        {
            await Poll();
        }

        // initialize tx set from rpc response with pre-initialized txs
        if (config.GetCanSplit() == true)
        {
            return ConvertRpcSentTxsToTxSet(result, txs, config).GetTxs()!;
        }

        return ConvertRpcTxToTxSet(result, txs.Count == 0 ? null : txs[0], true, config).GetTxs()!;
    }

    public async Task<MoneroTxWallet> SweepOutput(MoneroTxConfig config)
    {
        // validate request
        if (config.GetSweepEachSubaddress() != null)
        {
            throw new Exception("Expected sweep each subaddress to be null");
        }

        if (config.GetBelowAmount() != null)
        {
            throw new Exception("Expected below amount to be null");
        }

        if (config.GetCanSplit() != null)
        {
            throw new Exception("Splitting is not applicable when sweeping output");
        }

        if (config.GetDestinations() == null || config.GetDestinations()!.Count != 1 ||
            string.IsNullOrEmpty(config.GetDestinations()![0].GetAddress()))
        {
            throw new MoneroError("Must provide exactly one destination address to sweep output to");
        }

        if (config.GetSubtractFeeFrom() != null && config.GetSubtractFeeFrom()!.Count > 0)
        {
            throw new MoneroError("Sweep transactions do not support subtracting fees from destinations");
        }

        // build request parameters
        MoneroJsonRpcParams parameters = [];
        parameters.Add("address", config.GetDestinations()![0].GetAddress());
        parameters.Add("account_index", config.GetAccountIndex());
        parameters.Add("subaddr_indices", config.GetSubaddressIndices());
        parameters.Add("key_image", config.GetKeyImage());
        parameters.Add("do_not_relay", !config.GetRelay());
        parameters.Add("priority", config.GetPriority() == null ? null : config.GetPriority());
        parameters.Add("payment_id", config.GetPaymentId());
        parameters.Add("get_tx_key", true);
        parameters.Add("get_tx_hex", true);
        parameters.Add("get_tx_metadata", true);

        // send request
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("sweep_single", parameters);
        MoneroJsonRpcParams result = resp.Result!;

        // notify of changes
        if (config.GetRelay() == true)
        {
            await Poll();
        }

        // build and return tx
        MoneroTxWallet tx = InitSentTxWallet(config, null, true);
        ConvertRpcTxToTxSet(result, tx, true, config);
        tx.GetOutgoingTransfer()!.GetDestinations()![0]
            .SetAmount(tx.GetOutgoingTransfer()!.GetAmount()); // initialize destination amount
        return tx;
    }

    public async Task<List<MoneroTxWallet>> SweepUnlocked(MoneroTxConfig config)
    {
        // validate request
        if (config == null)
        {
            throw new MoneroError("Sweep request cannot be null");
        }

        CheckSweepConfig(config, false);

        if (config.GetSubaddressIndices() != null && config.GetSubaddressIndices()!.Count == 0)
        {
            config.SetSubaddressIndices(null);
        }

        if (config.GetAccountIndex() == null && config.GetSubaddressIndices() != null)
        {
            throw new MoneroError("Must specify account index if subaddress indices are specified");
        }

        if (config.GetSubtractFeeFrom() != null && config.GetSubtractFeeFrom()!.Count > 0)
        {
            throw new MoneroError("Sweep transactions do not support subtracting fees from destinations");
        }

        // determine account and subaddress indices to sweep; default to all with unlocked balance if not specified
        Dictionary<uint, List<uint>> indices = []; // java type preserves insertion order
        if (config.GetAccountIndex() != null)
        {
            if (config.GetSubaddressIndices() != null)
            {
                indices.Add((uint)config.GetAccountIndex()!, config.GetSubaddressIndices()!);
            }
            else
            {
                List<uint> subaddressIndices = [];
                indices.Add((uint)config.GetAccountIndex()!, subaddressIndices);
                foreach (MoneroSubaddress subaddress in await GetSubaddresses((uint)config.GetAccountIndex()!, true,
                             null))
                {
                    // TODO: wallet rpc sweep_all now supports req.subaddr_indices_all
                    if (((ulong)subaddress.GetUnlockedBalance()!).CompareTo(0) > 0)
                    {
                        subaddressIndices.Add((uint)subaddress.GetIndex()!);
                    }
                }
            }
        }
        else
        {
            List<MoneroAccount> accounts = await GetAccounts(true, true, null);
            foreach (MoneroAccount account in accounts)
            {
                if (account.GetUnlockedBalance().CompareTo(0) > 0)
                {
                    List<uint> subaddressIndices = [];
                    indices.Add((uint)account.GetIndex()!, subaddressIndices);
                    foreach (MoneroSubaddress subaddress in account.GetSubaddresses()!)
                    {
                        if (((ulong)subaddress.GetUnlockedBalance()!).CompareTo(0) > 0)
                        {
                            subaddressIndices.Add((uint)subaddress.GetIndex()!);
                        }
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
            if (copy.GetSweepEachSubaddress() != true)
            {
                copy.SetSubaddressIndices(indices[accountIdx]);
                txs.AddRange(await RpcSweepAccount(copy));
            }

            // otherwise sweep each subaddress individually
            else
            {
                foreach (uint subaddressIdx in indices[accountIdx])
                {
                    copy.SetSubaddressIndices(subaddressIdx);
                    txs.AddRange(await RpcSweepAccount(copy));
                }
            }
        }

        // notify of changes
        if (config.GetRelay() == true)
        {
            await Poll();
        }

        return txs;
    }

    public async Task<List<MoneroTxWallet>> SweepDust(bool relay)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("do_not_relay", !relay);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("sweep_dust", parameters);
        if (relay)
        {
            await Poll();
        }

        MoneroJsonRpcParams result = resp.Result!;
        MoneroTxSet txSet = ConvertRpcSentTxsToTxSet(result, null, null);
        if (txSet.GetTxs() == null)
        {
            return [];
        }

        foreach (MoneroTxWallet tx in txSet.GetTxs()!)
        {
            tx.SetIsRelayed(relay);
            tx.SetInTxPool(relay);
        }

        return txSet.GetTxs()!;
    }

    public async Task<string> RelayTx(string txMetadata)
    {
        if (txMetadata == null)
        {
            throw new MoneroError("Must provide an array of tx metadata to relay");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("hex", txMetadata);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("relay_tx", parameters);
        MoneroJsonRpcParams result = resp.Result!;

        await Poll(); // notify of changes
        return (string)result["tx_hash"]!;
    }

    public async Task<MoneroTxSet> DescribeTxSet(MoneroTxSet txSet)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("unsigned_txset", txSet.GetUnsignedTxHex());
        parameters.Add("multisig_txset", txSet.GetMultisigTxHex());
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("describe_transfer", parameters);
        return ConvertRpcDescribeTransfer(resp.Result);
    }

    public async Task<MoneroTxSet> SignTxs(string unsignedTxHex)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("unsigned_txset", unsignedTxHex);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("sign_transfer", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return ConvertRpcSentTxsToTxSet(result, null, null);
    }

    public async Task<List<string>> SubmitTxs(string signedTxHex)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("tx_data_hex", signedTxHex);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("submit_transfer", parameters);
        await Poll();
        MoneroJsonRpcParams result = resp.Result!;
        return ((JArray)result["tx_hash_list"]!).ToObject<List<string>>()!;
    }

    public async Task<string> SignMessage(string msg, MoneroMessageSignatureType signatureType,
        uint accountIdx,
        uint subaddressIdx)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("data", msg);
        parameters.Add("signature_type",
            signatureType == MoneroMessageSignatureType.SignWithSpendKey ? "spend" : "view");
        parameters.Add("account_index", accountIdx);
        parameters.Add("address_index", subaddressIdx);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("sign", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["signature"]!;
    }

    public async Task<MoneroMessageSignatureResult> VerifyMessage(string msg, string address, string signature)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("data", msg);
        parameters.Add("address", address);
        parameters.Add("signature", signature);
        try
        {
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("verify", parameters);
            MoneroJsonRpcParams result = resp.Result!;
            bool isGood = (bool?)result["good"] == true;
            return new MoneroMessageSignatureResult(
                isGood,
                !isGood ? null : (bool?)result["old"],
                !isGood || !result.ContainsKey("signature_type") ? null :
                "view".Equals(result["signature_type"]) ? MoneroMessageSignatureType.SignWithViewKey :
                MoneroMessageSignatureType.SignWithSpendKey,
                !isGood ? null : (int?)result["version"]);
        }
        catch (MoneroRpcError e)
        {
            if (-2 == e.GetCode())
            {
                return new MoneroMessageSignatureResult();
            }

            throw;
        }
    }

    public async Task<string> GetTxKey(string txHash)
    {
        try
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txid", txHash);
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_tx_key", parameters);
            MoneroJsonRpcParams result = resp.Result!;
            return (string)result["tx_key"]!;
        }
        catch (MoneroRpcError e)
        {
            HandleInvalidTxIdError(e);
        }

        throw new MoneroError("Could not get tx key due an unknown error");
    }

    public async Task<MoneroCheckTx> CheckTxKey(string txHash, string txKey, string address)
    {
        try
        {
            // send request
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txid", txHash);
            parameters.Add("tx_key", txKey);
            parameters.Add("address", address);
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("check_tx_key", parameters);

            // interpret result
            MoneroJsonRpcParams result = resp.Result!;
            MoneroCheckTx check = new();
            check.SetIsGood(true);
            check.SetNumConfirmations((ulong?)result["confirmations"]);
            check.SetInTxPool((bool?)result["in_pool"]);
            check.SetReceivedAmount((ulong?)result["received"]);
            return check;
        }
        catch (MoneroRpcError e)
        {
            HandleInvalidTxIdError(e);
        }

        throw new MoneroError("Could not check tx key due an unknown error");
    }

    public async Task<string> GetTxProof(string txHash, string address, string? message)
    {
        try
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txid", txHash);
            parameters.Add("address", address);
            parameters.Add("message", message);
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_tx_proof", parameters);
            MoneroJsonRpcParams result = resp.Result!;
            return (string)result["signature"]!;
        }
        catch (MoneroRpcError e)
        {
            HandleInvalidTxIdError(e);
        }

        throw new MoneroError("Could not get tx proof due an unknown error");
    }

    public async Task<MoneroCheckTx> CheckTxProof(string txHash, string address, string message,
        string signature)
    {
        try
        {
            // send request
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txid", txHash);
            parameters.Add("address", address);
            parameters.Add("message", message);
            parameters.Add("signature", signature);
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("check_tx_proof", parameters);

            // interpret response
            MoneroJsonRpcParams result = resp.Result!;
            bool isGood = (bool?)result["good"] == true;
            MoneroCheckTx check = new();
            check.SetIsGood(isGood);
            if (isGood)
            {
                check.SetNumConfirmations((ulong?)result["confirmations"]);
                check.SetInTxPool((bool?)result["in_pool"]);
                check.SetReceivedAmount((ulong?)result["received"]);
            }

            return check;
        }
        catch (MoneroRpcError e)
        {
            if (-1 == e.GetCode() && e.Message.Equals("basic_string"))
            {
                e = new MoneroRpcError("Must provide signature to check tx proof", -1);
            }

            HandleInvalidTxIdError(e);
        }

        throw new MoneroError("Could not check tx proof due an unknown error");
    }

    public async Task<string> GetSpendProof(string txHash, string? message)
    {
        try
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txid", txHash);
            parameters.Add("message", message);
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_spend_proof", parameters);
            MoneroJsonRpcParams result = resp.Result!;
            return (string?)result["signature"]!;
        }
        catch (MoneroRpcError e)
        {
            HandleInvalidTxIdError(e);
        }

        throw new MoneroError("Could not get spend proof due an unknown error");
    }

    public async Task<bool> CheckSpendProof(string txHash, string message, string signature)
    {
        try
        {
            MoneroJsonRpcParams parameters = [];
            parameters.Add("txid", txHash);
            parameters.Add("message", message);
            parameters.Add("signature", signature);
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp =
                await _rpc.SendJsonRequest("check_spend_proof", parameters);
            MoneroJsonRpcParams result = resp.Result!;
            return (bool)result["good"]!;
        }
        catch (MoneroRpcError e)
        {
            HandleInvalidTxIdError(e);
        }

        return false;
    }

    public async Task<string> GetReserveProofWallet(string message)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("all", true);
        parameters.Add("message", message);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_reserve_proof", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["signature"]!;
    }

    public async Task<string> GetReserveProofAccount(uint accountIdx, ulong amount, string message)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("account_index", accountIdx);
        parameters.Add("amount", amount.ToString());
        parameters.Add("message", message);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_reserve_proof", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["signature"]!;
    }

    public async Task<MoneroCheckReserve> CheckReserveProof(string address, string message, string signature)
    {
        // send request
        MoneroJsonRpcParams parameters = [];
        parameters.Add("address", address);
        parameters.Add("message", message);
        parameters.Add("signature", signature);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("check_reserve_proof", parameters);
        MoneroJsonRpcParams result = resp.Result!;

        // interpret results
        bool isGood = (bool)result["good"]!;
        MoneroCheckReserve check = new();
        check.SetIsGood(isGood);
        if (isGood)
        {
            check.SetTotalAmount((ulong)result["total"]!);
            check.SetUnconfirmedSpentAmount((ulong)result["spent"]!);
        }

        return check;
    }

    public async Task<List<string>> GetTxNotes(List<string> txHashes)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("txids", txHashes);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_tx_notes", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return ((JArray)result["notes"]!).ToObject<List<string>>()!;
    }

    public async Task SetTxNotes(List<string> txHashes, List<string> notes)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("txids", txHashes);
        parameters.Add("notes", notes);
        await _rpc.SendJsonRequest("set_tx_notes", parameters);
    }

    public async Task<List<MoneroAddressBookEntry>> GetAddressBookEntries(List<uint>? entryIndices)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("entries", entryIndices);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> respMap = await _rpc.SendJsonRequest("get_address_book", parameters);
        MoneroJsonRpcParams resultMap = respMap.Result!;
        List<MoneroAddressBookEntry> entries = [];
        if (!resultMap.ContainsKey("entries"))
        {
            return entries;
        }

        List<MoneroJsonRpcParams> entriesMap =
            ((JArray)resultMap["entries"]!).ToObject<List<Dictionary<string, object?>>>()!;
        foreach (MoneroJsonRpcParams entryMap in entriesMap)
        {
            MoneroAddressBookEntry entry = new(
                (uint?)entryMap["index"],
                (string?)entryMap["address"],
                (string?)entryMap["description"],
                (string?)entryMap["payment_id"]
            );
            entries.Add(entry);
        }

        return entries;
    }

    public async Task<int> AddAddressBookEntry(string address, string description)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("address", address);
        parameters.Add("description", description);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> respMap = await _rpc.SendJsonRequest("add_address_book", parameters);
        MoneroJsonRpcParams resultMap = respMap.Result!;
        return (int)resultMap["index"]!;
    }

    public async Task EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription,
        string description)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("index", index);
        parameters.Add("set_address", setAddress);
        parameters.Add("address", address);
        parameters.Add("set_description", setDescription);
        parameters.Add("description", description);
        await _rpc.SendJsonRequest("edit_address_book", parameters);
    }

    public async Task DeleteAddressBookEntry(uint entryIdx)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("index", entryIdx);
        await _rpc.SendJsonRequest("delete_address_book", parameters);
    }

    public async Task TagAccounts(string tag, List<uint> accountIndices)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("tag", tag);
        parameters.Add("accounts", accountIndices);
        await _rpc.SendJsonRequest("tag_accounts", parameters);
    }

    public async Task UntagAccounts(List<uint> accountIndices)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("accounts", accountIndices);
        await _rpc.SendJsonRequest("untag_accounts", parameters);
    }

    public async Task<List<MoneroAccountTag>> GetAccountTags()
    {
        List<MoneroAccountTag> tags = [];
        MoneroJsonRpcResponse<MoneroJsonRpcParams> respMap = await _rpc.SendJsonRequest("get_account_tags");
        MoneroJsonRpcParams resultMap = respMap.Result!;
        List<MoneroJsonRpcParams>? accountTagMaps =
            ((JArray)resultMap["account_tags"]!).ToObject<List<Dictionary<string, object?>>>();
        if (accountTagMaps != null)
        {
            foreach (MoneroJsonRpcParams accountTagMap in accountTagMaps)
            {
                MoneroAccountTag tag = new();
                tags.Add(tag);
                tag.SetTag((string?)accountTagMap["tag"]);
                tag.SetLabel((string?)accountTagMap["label"]);
                List<uint> accountIndicesBI = ((JArray)accountTagMap["accounts"]!).ToObject<List<uint>>()!;
                List<uint> accountIndices = [];
                foreach (uint idx in accountIndicesBI)
                {
                    accountIndices.Add(idx);
                }

                tag.SetAccountIndices(accountIndices);
            }
        }

        return tags;
    }

    public async Task SetAccountTagLabel(string tag, string label)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("tag", tag);
        parameters.Add("description", label);
        await _rpc.SendJsonRequest("set_account_tag_description", parameters);
    }

    public async Task<string> GetPaymentUri(MoneroTxConfig config)
    {
        if (config == null)
        {
            throw new MoneroError("Must provide configuration to create a payment URI");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("address", config.GetDestinations()![0].GetAddress());
        parameters.Add("amount",
            config.GetDestinations()![0].GetAmount() != null
                ? config.GetDestinations()![0].GetAmount().ToString()
                : null);
        parameters.Add("payment_id", config.GetPaymentId());
        parameters.Add("recipient_name", config.GetRecipientName());
        parameters.Add("tx_description", config.GetNote());
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("make_uri", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["uri"]!;
    }

    public async Task<MoneroTxConfig> ParsePaymentUri(string uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            throw new MoneroError("Must provide URI to parse");
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("uri", uri);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("parse_uri", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        MoneroJsonRpcParams rpcUri = ((JObject)result["uri"]!).ToObject<Dictionary<string, object?>>()!;
        MoneroTxConfig config = new MoneroTxConfig().SetAddress((string?)rpcUri["address"])
            .SetAmount((ulong?)rpcUri["amount"]);
        config.SetPaymentId((string?)rpcUri["payment_id"]);
        config.SetRecipientName((string?)rpcUri["recipient_name"]);
        config.SetNote((string?)rpcUri["tx_description"]);
        if ("".Equals(config.GetDestinations()![0].GetAddress()))
        {
            config.GetDestinations()![0].SetAddress(null);
        }

        if ("".Equals(config.GetPaymentId()))
        {
            config.SetPaymentId(null);
        }

        if ("".Equals(config.GetRecipientName()))
        {
            config.SetRecipientName(null);
        }

        if ("".Equals(config.GetNote()))
        {
            config.SetNote(null);
        }

        return config;
    }

    public async Task<string?> GetAttribute(string key)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("key", key);
        try
        {
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_attribute", parameters);
            MoneroJsonRpcParams result = resp.Result!;
            string? value = (string?)result["value"];
            return string.IsNullOrEmpty(value) ? null : value;
        }
        catch (MoneroRpcError e)
        {
            if (-45 == e.GetCode())
            {
                return null; // -45: attribute not found
            }

            throw;
        }
    }

    public async Task SetAttribute(string key, string val)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("key", key);
        parameters.Add("value", val);
        await _rpc.SendJsonRequest("set_attribute", parameters);
    }

    public async Task StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("threads_count", numThreads);
        parameters.Add("do_background_mining", backgroundMining);
        parameters.Add("ignore_battery", ignoreBattery);
        await _rpc.SendJsonRequest("start_mining", parameters);
    }

    public async Task StopMining()
    {
        await _rpc.SendJsonRequest("stop_mining");
    }

    public async Task<bool> IsMultisigImportNeeded()
    {
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_balance");
        MoneroJsonRpcParams result = resp.Result!;
        return (bool)result["multisig_import_needed"]!;
    }

    public async Task<MoneroMultisigInfo> GetMultisigInfo()
    {
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("is_multisig");
        MoneroJsonRpcParams result = resp.Result!;
        MoneroMultisigInfo info = new();
        info.SetIsMultisig((bool?)result["multisig"]);
        info.SetIsReady((bool?)result["ready"]);
        info.SetThreshold((int?)result["threshold"]);
        info.SetNumParticipants((int?)result["total"]);
        return info;
    }

    public async Task<string> PrepareMultisig()
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("enable_multisig_experimental", true);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("prepare_multisig", parameters);
        _addressCache.Clear();
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["multisig_info"]!;
    }

    public async Task<string> MakeMultisig(List<string> multisigHexes, int threshold, string password)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("multisig_info", multisigHexes);
        parameters.Add("threshold", threshold);
        parameters.Add("password", password);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("make_multisig", parameters);
        _addressCache.Clear();
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["multisig_info"]!;
    }

    public async Task<MoneroMultisigInitResult> ExchangeMultisigKeys(List<string> multisigHexes,
        string password)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("multisig_info", multisigHexes);
        parameters.Add("password", password);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp =
            await _rpc.SendJsonRequest("exchange_multisig_keys", parameters);
        _addressCache.Clear();
        MoneroJsonRpcParams result = resp.Result!;
        MoneroMultisigInitResult msResult = new();
        msResult.SetAddress((string?)result["address"]);
        msResult.SetMultisigHex((string?)result["multisig_info"]);
        if (string.IsNullOrEmpty(msResult.GetAddress()))
        {
            msResult.SetAddress(null);
        }

        if (string.IsNullOrEmpty(msResult.GetMultisigHex()))
        {
            msResult.SetMultisigHex(null);
        }

        return msResult;
    }

    public async Task<string> ExportMultisigHex()
    {
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("export_multisig_info");
        MoneroJsonRpcParams result = resp.Result!;
        return (string)result["info"]!;
    }

    public async Task<int> ImportMultisigHex(List<string> multisigHexes)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("info", multisigHexes);
        MoneroJsonRpcResponse<MoneroJsonRpcParams>
            resp = await _rpc.SendJsonRequest("import_multisig_info", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        return (int)result["n_outputs"]!;
    }

    public async Task<MoneroMultisigSignResult> SignMultisigTxHex(string multisigTxHex)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("tx_data_hex", multisigTxHex);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("sign_multisig", parameters);
        MoneroJsonRpcParams? result = resp.Result;

        if (result == null)
        {
            throw new MoneroError("Invalid response from sign_multisig: " + resp);
        }

        MoneroMultisigSignResult signResult = new();
        signResult.SetSignedMultisigTxHex((string?)result["tx_data_hex"]);
        signResult.SetTxHashes(((JArray)result["tx_hash_list"]!).ToObject<List<string>>());
        return signResult;
    }

    public async Task<List<string>> SubmitMultisigTxHex(string signedMultisigTxHex)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("tx_data_hex", signedMultisigTxHex);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("submit_multisig", parameters);
        if (resp.Result == null || !resp.Result.ContainsKey("tx_hash_list"))
        {
            throw new MoneroError("Invalid response from submit_multisig: " + resp);
        }

        return ((JArray)resp.Result["tx_hash_list"]!).ToObject<List<string>>()!;
    }

    public async Task ChangePassword(string oldPassword, string newPassword)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("old_password", oldPassword);
        parameters.Add("new_password", newPassword);
        await _rpc.SendJsonRequest("change_wallet_password", parameters);
    }

    public async Task Save()
    {
        await _rpc.SendJsonRequest("store");
    }

    public async Task Close(bool save)
    {
        Clear();
        MoneroJsonRpcParams parameters = [];
        parameters.Add("autosave_current", save);
        await _rpc.SendJsonRequest("close_wallet", parameters);
    }

    public async Task<bool> IsClosed()
    {
        try
        {
            await GetPrimaryAddress();
        }
        catch (MoneroRpcError e)
        {
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
        _listeners.Clear();
        RefreshListening();
        _addressCache.Clear();
        _path = null;
    }

    private void RefreshListening()
    {
        if (_rpc.GetZmqUri() != null)
        {
            return;
        }

        if (_walletPoller == null && _listeners.Count > 0)
        {
            _walletPoller = new MoneroWalletPoller(this, _syncPeriodInMs);
        }

        _walletPoller?.SetIsPolling(_listeners.Count > 0);
    }

    private async Task Poll()
    {
        if (_walletPoller != null && _walletPoller.IsPolling())
        {
            await _walletPoller.Poll();
        }
    }

    private async Task<Dictionary<uint, List<uint>?>> GetAccountIndices(bool getSubaddressIndices)
    {
        Dictionary<uint, List<uint>?> indices = [];
        foreach (MoneroAccount account in await GetAccounts(false, false, null))
        {
            uint? accountIdx = account.GetIndex();
            if (accountIdx == null)
            {
                continue;
            }

            indices.Add((uint)accountIdx, getSubaddressIndices ? await GetSubaddressIndices((uint)accountIdx) : null);
        }

        return indices;
    }

    private async Task<List<uint>> GetSubaddressIndices(uint accountIdx)
    {
        List<uint> subaddressIndices = [];
        MoneroJsonRpcParams parameters = [];
        parameters.Add("account_index", accountIdx);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_address", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        foreach (MoneroJsonRpcParams address in ((JArray)result["addresses"]!)
                 .ToObject<List<Dictionary<string, object?>>>()!)
        {
            subaddressIndices.Add((uint)address["address_index"]!);
        }

        return subaddressIndices;
    }

    private async Task<List<MoneroKeyImage>> RpcExportKeyImages(bool all)
    {
        MoneroJsonRpcParams parameters = [];
        parameters.Add("all", all);
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("export_key_images", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        List<MoneroKeyImage> images = [];
        if (!result.ContainsKey("signed_key_images"))
        {
            return images;
        }

        foreach (MoneroJsonRpcParams rpcImage in ((JArray)result["signed_key_images"]!)
                 .ToObject<List<Dictionary<string, object?>>>()!)
        {
            images.Add(new MoneroKeyImage((string?)rpcImage["key_image"], (string?)rpcImage["signature"]));
        }

        return images;
    }

    private async Task<List<ulong>> GetBalances(uint? accountIdx, uint? subaddressIdx)
    {
        if (accountIdx == null)
        {
            if (subaddressIdx != null)
            {
                throw new MoneroError("Must provide account index with subaddress index");
            }

            ulong balance = 0;
            ulong unlockedBalance = 0;
            foreach (MoneroAccount account in await GetAccounts(false, false, null))
            {
                balance += account.GetBalance();
                unlockedBalance += account.GetUnlockedBalance();
            }

            return [balance, unlockedBalance];
        }

        MoneroJsonRpcParams parameters = [];
        parameters.Add("account_index", accountIdx);
        parameters.Add("address_indices", subaddressIdx == null ? null : new List<uint?> { subaddressIdx });
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_balance", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        if (subaddressIdx == null)
        {
            return [Convert.ToUInt64(result["balance"]!), Convert.ToUInt64(result["unlocked_balance"]!)];
        }

        List<MoneroJsonRpcParams> rpcBalancesPerSubaddress =
            ((JArray)result["per_subaddress"]!).ToObject<List<Dictionary<string, object>>>()!;
        return
        [
            Convert.ToUInt64(rpcBalancesPerSubaddress[0]["balance"]!),
            Convert.ToUInt64(rpcBalancesPerSubaddress[0]["unlocked_balance"]!)
        ];
    }

    private async Task<List<MoneroTransfer>> GetTransfersAux(MoneroTransferQuery? query)
    {
        // copy and normalize query up to block
        if (query == null)
        {
            query = new MoneroTransferQuery();
        }
        else
        {
            if (query.GetTxQuery() == null)
            {
                query = query.Clone();
            }
            else
            {
                MoneroTxQuery _txQuery = query.GetTxQuery()!.Clone();
                if (query.GetTxQuery()!.GetTransferQuery() == query)
                {
                    query = _txQuery.GetTransferQuery()!;
                }
                else
                {
                    if (query.GetTxQuery()!.GetTransferQuery() != null)
                    {
                        throw new MoneroError("Transfer query's tx query must be circular reference or null");
                    }

                    query = query.Clone();
                    query.SetTxQuery(_txQuery);
                }
            }
        }

        if (query.GetTxQuery() == null)
        {
            query.SetTxQuery(new MoneroTxQuery());
        }

        MoneroTxQuery txQuery = query.GetTxQuery()!;

        // build params for get_transfers rpc call
        MoneroJsonRpcParams parameters = [];
        bool canBeConfirmed = txQuery.IsConfirmed() != false && txQuery.InTxPool() != true &&
                              txQuery.IsFailed() != true && txQuery.IsRelayed() != false;
        bool canBeInTxPool = txQuery.IsConfirmed() != true && txQuery.InTxPool() != false &&
                             txQuery.IsFailed() != true && txQuery.GetHeight() == null &&
                             txQuery.GetMaxHeight() == null && txQuery.IsLocked() != false;
        bool canBeIncoming = query.IsIncoming() != false && query.IsOutgoing() != true &&
                             query.HasDestinations() != true;
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
        parameters.Add("failed",
            txQuery.IsFailed() != false && txQuery.IsConfirmed() != true && txQuery.InTxPool() != true);
        if (txQuery.GetMinHeight() != null)
        {
            if (txQuery.GetMinHeight() > 0)
            {
                parameters.Add("min_height",
                    txQuery.GetMinHeight() -
                    1); // TODO monero-project: wallet2::get_payments() min_height is exclusive, so manually offset to match intended range (issues #5751, #5598)
            }
            else
            {
                parameters.Add("min_height", txQuery.GetMinHeight());
            }
        }

        if (txQuery.GetMaxHeight() != null)
        {
            parameters.Add("max_height", txQuery.GetMaxHeight());
        }

        parameters.Add("filter_by_height", txQuery.GetMinHeight() != null || txQuery.GetMaxHeight() != null);
        if (query.GetAccountIndex() == null)
        {
            if (!(query.GetSubaddressIndex() == null && query.GetSubaddressIndices() == null))
            {
                throw new MoneroError("Filter specifies a subaddress index but not an account index");
            }

            parameters.Add("all_accounts", true);
        }
        else
        {
            parameters.Add("account_index", query.GetAccountIndex());

            // set subaddress indices param
            HashSet<uint> subaddressIndices = [];
            if (query.GetSubaddressIndex() != null)
            {
                subaddressIndices.Add((uint)query.GetSubaddressIndex()!);
            }

            if (query.GetSubaddressIndices() != null)
            {
                foreach (uint subaddressIdx in query.GetSubaddressIndices()!)
                {
                    subaddressIndices.Add(subaddressIdx);
                }
            }

            if (subaddressIndices.Count > 0)
            {
                parameters.Add("subaddr_indices", subaddressIndices);
            }
        }

        // cache unique txs and blocks
        Dictionary<string, MoneroTxWallet> txMap = [];
        Dictionary<ulong, MoneroBlock?> blockMap = [];

        // build txs using `get_transfers`
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("get_transfers", parameters);
        MoneroJsonRpcParams result = resp.Result!;
        foreach (string key in result.Keys)
        {
            object val = result[key]!;
            foreach (MoneroJsonRpcParams rpcTx in ((JArray)val).ToObject<List<Dictionary<string, object?>>>()!)
            {
                MoneroTxWallet tx = ConvertRpcTxWithTransfer(rpcTx, null, null, null);
                if (tx.IsConfirmed() == true)
                {
                    if (!tx.GetBlock()!.GetTxs()!.Contains(tx))
                    {
                        throw new MoneroError("Tx not in block");
                    }
                }

                // replace transfer amount with destination sum
                // TODO monero-wallet-rpc: confirmed tx from/to same account has amount 0 but cached transfers
                if (tx.GetOutgoingTransfer() != null && tx.IsRelayed() == true && tx.IsFailed() != true &&
                    tx.GetOutgoingTransfer()!.GetDestinations() != null && tx.GetOutgoingAmount() == 0)
                {
                    MoneroOutgoingTransfer outgoingTransfer = tx.GetOutgoingTransfer()!;
                    ulong transferTotal = 0;
                    foreach (MoneroDestination destination in outgoingTransfer.GetDestinations()!)
                    {
                        transferTotal += (ulong)destination.GetAmount()!;
                    }

                    tx.GetOutgoingTransfer()!.SetAmount(transferTotal);
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
            if (tx.IsIncoming() == null)
            {
                tx.SetIsIncoming(false);
            }

            if (tx.IsOutgoing() == null)
            {
                tx.SetIsOutgoing(false);
            }

            // sort incoming transfers
            if (tx.GetIncomingTransfers() != null)
            {
                tx.GetIncomingTransfers()!.Sort(new MoneroIncomingTransferComparer());
            }

            // collect queried transfers, erase if excluded
            transfers.AddRange(tx.FilterTransfers(query));

            // remove excluded txs from block
            if (tx.GetBlock() != null && tx.GetOutgoingTransfer() == null && tx.GetIncomingTransfers() == null)
            {
                tx.GetBlock()!.GetTxs()!.Remove(tx);
            }
        }

        return transfers;
    }

    private async Task<List<MoneroOutputWallet>> GetOutputsAux(MoneroOutputQuery? query)
    {
        // copy and normalize query up to block
        if (query == null)
        {
            query = new MoneroOutputQuery();
        }
        else
        {
            if (query.GetTxQuery() == null)
            {
                query = query.Clone();
            }
            else
            {
                MoneroTxQuery txQuery = query.GetTxQuery()!.Clone();
                if (query.GetTxQuery()!.GetOutputQuery() == query)
                {
                    query = txQuery.GetOutputQuery()!;
                }
                else
                {
                    if (query.GetTxQuery()!.GetOutputQuery() != null)
                    {
                        throw new MoneroError("Output request's tx request must be circular reference or null");
                    }

                    query = query.Clone();
                    query.SetTxQuery(txQuery);
                }
            }
        }

        if (query.GetTxQuery() == null)
        {
            query.SetTxQuery(new MoneroTxQuery());
        }

        // determine account and subaddress indices to be queried
        Dictionary<uint, List<uint>?> indices = [];
        if (query.GetAccountIndex() != null)
        {
            HashSet<uint> subaddressIndices = [];
            if (query.GetSubaddressIndex() != null)
            {
                subaddressIndices.Add((uint)query.GetSubaddressIndex()!);
            }

            if (query.GetSubaddressIndices() != null)
            {
                foreach (uint subaddressIdx in query.GetSubaddressIndices()!)
                {
                    subaddressIndices.Add(subaddressIdx);
                }
            }

            indices.Add((uint)query.GetAccountIndex()!,
                subaddressIndices.Count == 0 ? null : [.. subaddressIndices]); // null will fetch from all subaddresses
        }
        else
        {
            if (query.GetSubaddressIndex() != null)
            {
                throw new MoneroError("Request specifies a subaddress index but not an account index");
            }

            if (!(query.GetSubaddressIndices() == null || query.GetSubaddressIndices()!.Count == 0))
            {
                throw new MoneroError("Request specifies subaddress indices but not an account index");
            }

            indices = await GetAccountIndices(false); // fetch all account indices without subaddresses
        }

        // cache unique txs and blocks
        Dictionary<string, MoneroTxWallet> txMap = [];
        Dictionary<ulong, MoneroBlock?> blockMap = [];

        // collect txs with outputs for each indicated account using `incoming_transfers` rpc call
        MoneroJsonRpcParams parameters = [];
        string transferType;
        if (query.IsSpent() == true)
        {
            transferType = "unavailable";
        }
        else if (query.IsSpent() != true)
        {
            transferType = "available";
        }
        else
        {
            transferType = "all";
        }

        parameters.Add("transfer_type", transferType);
        parameters.Add("verbose", true);
        foreach (uint accountIdx in indices.Keys)
        {
            // send request
            parameters.Add("account_index", accountIdx);
            parameters.Add("subaddr_indices", indices[accountIdx]);
            MoneroJsonRpcResponse<MoneroJsonRpcParams> resp =
                await _rpc.SendJsonRequest("incoming_transfers", parameters);
            MoneroJsonRpcParams result = resp.Result!;

            // convert response to txs with outputs and merge
            if (!result.ContainsKey("transfers"))
            {
                continue;
            }

            foreach (MoneroJsonRpcParams rpcOutput in ((JArray)result["transfers"]!)
                     .ToObject<List<Dictionary<string, object?>>>()!)
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
            if (tx.GetOutputs() != null)
            {
                tx.GetOutputs()!.Sort(new MoneroOutputComparer());
            }

            // collect queried outputs, erase if excluded
            outputs.AddRange(tx.FilterOutputsWallet(query));

            // remove excluded txs from block
            if (tx.GetOutputs() == null && tx.GetBlock() != null)
            {
                tx.GetBlock()!.GetTxs()!.Remove(tx);
            }
        }

        return outputs;
    }

    private void CheckSweepConfig(MoneroTxConfig? config, bool isSingle)
    {
        if (config == null)
        {
            throw new MoneroError("Sweep request cannot be null");
        }

        if (isSingle)
        {
            if (config.GetAccountIndex() == null)
            {
                throw new MoneroError("Must specify an account index to sweep from");
            }
        }

        if (config.GetDestinations() == null || config.GetDestinations()!.Count != 1)
        {
            throw new MoneroError("Must specify exactly one destination to sweep to");
        }

        if (config.GetDestinations()![0].GetAddress() == null)
        {
            throw new MoneroError("Must specify destination address to sweep to");
        }

        if (config.GetDestinations()![0].GetAmount() != null)
        {
            throw new MoneroError("Cannot specify amount in sweep request");
        }

        if (config.GetKeyImage() != null)
        {
            throw new MoneroError("Key image defined; use sweepOutput() to sweep an output by its key image");
        }
    }

    private async Task<List<MoneroTxWallet>> RpcSweepAccount(MoneroTxConfig config)
    {
        // validate request
        if (config == null)
        {
            throw new MoneroError("Sweep request cannot be null");
        }

        CheckSweepConfig(config, true);

        if (config.GetSubaddressIndices() != null && config.GetSubaddressIndices()!.Count == 0)
        {
            throw new MoneroError("Empty list given for subaddresses indices to sweep");
        }

        if (config.GetSweepEachSubaddress() == true)
        {
            throw new MoneroError("Cannot sweep each subaddress with RPC `sweep_all`");
        }

        if (config.GetSubtractFeeFrom() != null && config.GetSubtractFeeFrom()!.Count > 0)
        {
            throw new MoneroError("Sweeping output does not support subtracting fees from destinations");
        }

        // sweep from all subaddresses if not otherwise defined
        if (config.GetSubaddressIndices() == null)
        {
            config.SetSubaddressIndices([]);
            foreach (MoneroSubaddress subaddress in await GetSubaddresses((uint)config.GetAccountIndex()!, true, null))
            {
                config.GetSubaddressIndices()!.Add((uint)subaddress.GetIndex()!);
            }
        }

        if (config.GetSubaddressIndices()!.Count == 0)
        {
            throw new MoneroError("No subaddresses to sweep from");
        }

        // common request params
        bool relay = config.GetRelay() == true;
        MoneroJsonRpcParams parameters = [];
        parameters.Add("account_index", config.GetAccountIndex());
        parameters.Add("subaddr_indices", config.GetSubaddressIndices());
        parameters.Add("address", config.GetDestinations()![0].GetAddress());
        parameters.Add("priority", config.GetPriority() == null ? null : config.GetPriority());
        parameters.Add("payment_id", config.GetPaymentId());
        parameters.Add("do_not_relay", !relay);
        parameters.Add("below_amount", config.GetBelowAmount());
        parameters.Add("get_tx_keys", true);
        parameters.Add("get_tx_hex", true);
        parameters.Add("get_tx_metadata", true);

        // invoke wallet rpc `sweep_all`
        MoneroJsonRpcResponse<MoneroJsonRpcParams> resp = await _rpc.SendJsonRequest("sweep_all", parameters);
        MoneroJsonRpcParams result = resp.Result!;

        // initialize txs from response
        MoneroTxSet txSet = ConvertRpcSentTxsToTxSet(result, null, config);

        // initialize remaining known fields
        foreach (MoneroTxWallet tx in txSet.GetTxs()!)
        {
            tx.SetIsLocked(true);
            tx.SetIsConfirmed(false);
            tx.SetNumConfirmations(0);
            tx.SetRelay(relay);
            tx.SetInTxPool(relay);
            tx.SetIsRelayed(relay);
            tx.SetIsMinerTx(false);
            tx.SetIsFailed(false);
            tx.SetRingSize(MoneroUtils.RingSize);
            MoneroOutgoingTransfer? transfer = tx.GetOutgoingTransfer();
            if (transfer == null)
            {
                throw new MoneroError("Transfer is null");
            }

            transfer.SetAccountIndex(config.GetAccountIndex());
            if (config.GetSubaddressIndices()!.Count == 1)
            {
                transfer.SetSubaddressIndices([.. config.GetSubaddressIndices()!]);
            }

            MoneroDestination destination = new(config.GetDestinations()![0].GetAddress(), transfer.GetAmount());
            transfer.SetDestinations([destination]);
            FinalizeSentTxWallet(config, tx);
        }

        return txSet.GetTxs()!;
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

    private static bool IsContextual(MoneroTxQuery? txQuery, bool isTransfer)
    {
        if (txQuery == null)
        {
            return false;
        }

        if (txQuery.IsIncoming() != null)
        {
            return true; // requires context of all transfers
        }

        if (txQuery.IsOutgoing() != null)
        {
            return true;
        }

        if (isTransfer)
        {
            if (txQuery.GetInputQuery() != null)
            {
                return true; // requires context of inputs
            }

            if (txQuery.GetOutputQuery() != null)
            {
                return true; // requires context of outputs
            }
        }
        else if (txQuery.GetTransferQuery() != null)
        {
            return true;
        }

        return false;
    }

    private static bool IsContextual(MoneroTransferQuery? query)
    {
        if (query == null)
        {
            return false;
        }

        MoneroTxQuery? txQuery = query.GetTxQuery();

        return IsContextual(txQuery, true);
    }

    private static bool IsContextual(MoneroOutputQuery? query)
    {
        if (query == null)
        {
            return false;
        }

        MoneroTxQuery? txQuery = query.GetTxQuery();

        return IsContextual(txQuery, false);
    }

    private static MoneroAccount ConvertRpcAccount(Dictionary<string, object?> rpcAccount)
    {
        MoneroAccount account = new();
        foreach (string key in rpcAccount.Keys)
        {
            object val = rpcAccount[key]!;
            switch (key)
            {
                case "account_index":
                    account.SetIndex(Convert.ToUInt32(val));
                    break;
                case "balance":
                    account.SetBalance(Convert.ToUInt64(val));
                    break;
                case "unlocked_balance":
                    account.SetUnlockedBalance(Convert.ToUInt64(val));
                    break;
                case "base_address":
                    account.SetPrimaryAddress((string)val);
                    break;
                case "tag":
                    account.SetTag((string)val);
                    break;
                case "label":
                    // label belongs to the first subaddress
                    break;
            }
        }

        if ("".Equals(account.GetTag()))
        {
            account.SetTag(null);
        }

        return account;
    }

    private static MoneroSubaddress ConvertRpcSubaddress(Dictionary<string, object?> rpcSubaddress)
    {
        MoneroSubaddress subaddress = new();
        foreach (string key in rpcSubaddress.Keys)
        {
            object val = rpcSubaddress[key]!;
            if (key.Equals("account_index"))
            {
                subaddress.SetAccountIndex(Convert.ToUInt32(val));
            }
            else if (key.Equals("address_index"))
            {
                subaddress.SetIndex(Convert.ToUInt32(val));
            }
            else if (key.Equals("address"))
            {
                subaddress.SetAddress((string)val);
            }
            else if (key.Equals("balance"))
            {
                subaddress.SetBalance(Convert.ToUInt64(val));
            }
            else if (key.Equals("unlocked_balance"))
            {
                subaddress.SetUnlockedBalance(Convert.ToUInt64(val));
            }
            else if (key.Equals("num_unspent_outputs"))
            {
                subaddress.SetNumUnspentOutputs(Convert.ToUInt64(val));
            }
            else if (key.Equals("label"))
            {
                if (!"".Equals(val))
                {
                    subaddress.SetLabel((string)val);
                }
            }
            else if (key.Equals("used"))
            {
                subaddress.SetIsUsed((bool)val);
            }
            else if (key.Equals("blocks_to_unlock"))
            {
                subaddress.SetNumBlocksToUnlock(Convert.ToUInt64(val));
            }
            else if (key.Equals("time_to_unlock"))
            {
                // ignoring
            }
        }

        return subaddress;
    }

    private static MoneroTxWallet InitSentTxWallet(MoneroTxConfig config, MoneroTxWallet? tx, bool copyDestinations)
    {
        if (tx == null)
        {
            tx = new MoneroTxWallet();
        }

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
        tx.SetRingSize(MoneroUtils.RingSize);
        MoneroOutgoingTransfer transfer = new MoneroOutgoingTransfer().SetTx(tx);
        if (config.GetSubaddressIndices() != null && config.GetSubaddressIndices()!.Count == 1)
        {
            transfer.SetSubaddressIndices([
                .. config.GetSubaddressIndices()!
            ]); // we know src subaddress indices iff request specifies 1
        }

        if (copyDestinations)
        {
            List<MoneroDestination> destCopies = [];
            foreach (MoneroDestination dest in config.GetDestinations()!)
            {
                destCopies.Add(dest.Clone());
            }

            transfer.SetDestinations(destCopies);
        }

        tx.SetOutgoingTransfer(transfer);

        FinalizeSentTxWallet(config, tx);

        return tx;
    }

    private static void FinalizeSentTxWallet(MoneroTxConfig? config, MoneroTxWallet? tx)
    {
        if (config == null)
        {
            throw new MoneroError("Must provide a valid tx config");
        }

        if (tx == null)
        {
            throw new MoneroError("Cannot finalize null tx");
        }

        tx.SetPaymentId(config.GetPaymentId());
        if (tx.GetUnlockTime() == null)
        {
            tx.SetUnlockTime(0);
        }

        if (tx.GetRelay() == true)
        {
            if (tx.GetLastRelayedTimestamp() == null)
            {
                tx.SetLastRelayedTimestamp((ulong)DateTimeOffset.UtcNow
                    .ToUnixTimeMilliseconds()); // TODO (monero-wallet-rpc): provide timestamp on response; unconfirmed timestamps vary
            }

            if (tx.IsDoubleSpendSeen() == null)
            {
                tx.SetIsDoubleSpendSeen(false);
            }
        }
    }

    private static MoneroTxSet ConvertRpcTxSet(Dictionary<string, object?>? rpcMap)
    {
        if (rpcMap == null)
        {
            throw new MoneroError("Cannot convert null rpc tx set");
        }

        MoneroTxSet txSet = new();
        txSet.SetMultisigTxHex((string?)rpcMap.GetValueOrDefault("multisig_txset"));
        txSet.SetUnsignedTxHex((string?)rpcMap.GetValueOrDefault("unsigned_txset"));
        txSet.SetSignedTxHex((string?)rpcMap.GetValueOrDefault("signed_txset"));
        if (string.IsNullOrEmpty(txSet.GetMultisigTxHex()))
        {
            txSet.SetMultisigTxHex(null);
        }

        if (string.IsNullOrEmpty(txSet.GetUnsignedTxHex()))
        {
            txSet.SetUnsignedTxHex(null);
        }

        if (string.IsNullOrEmpty(txSet.GetSignedTxHex()))
        {
            txSet.SetSignedTxHex(null);
        }

        return txSet;
    }

    private static MoneroTxSet ConvertRpcSentTxsToTxSet(Dictionary<string, object?>? rpcTxs, List<MoneroTxWallet>? txs,
        MoneroTxConfig? config)
    {
        if (rpcTxs == null)
        {
            throw new MoneroError("Cannot convert null rpc txs");
        }

        // build shared tx set
        MoneroTxSet txSet = ConvertRpcTxSet(rpcTxs);

        // get number of txs
        string? numTxsKey = rpcTxs.ContainsKey("fee_list") ? "fee_list" :
            rpcTxs.ContainsKey("tx_hash_list") ? "tx_hash_list" : null;
        int numTxs = numTxsKey == null ? 0 : ((JArray)rpcTxs[numTxsKey]!).Count;

        // done if rpc response contains no txs
        if (numTxs == 0)
        {
            if (txs != null)
            {
                throw new Exception("Cannot provide txs when rpc response contains no transactions");
            }

            return txSet;
        }

        // initialize txs if none given
        if (txs != null)
        {
            txSet.SetTxs(txs);
        }
        else
        {
            txs = [];
            for (int i = 0; i < numTxs; i++)
            {
                txs.Add(new MoneroTxWallet());
            }
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
            object val = rpcTxs[key]!;
            switch (key)
            {
                case "tx_hash_list":
                    {
                        List<string> hashes = ((JArray)val).ToObject<List<string>>()!;
                        for (int i = 0; i < hashes.Count; i++)
                        {
                            txs[i].SetHash(hashes[i]);
                        }

                        break;
                    }
                case "tx_key_list":
                    {
                        List<string> keys = ((JArray)val).ToObject<List<string>>()!;
                        for (int i = 0; i < keys.Count; i++)
                        {
                            txs[i].SetKey(keys[i]);
                        }

                        break;
                    }
                case "tx_blob_list":
                    {
                        List<string> blobs = ((JArray)val).ToObject<List<string>>()!;
                        for (int i = 0; i < blobs.Count; i++)
                        {
                            txs[i].SetFullHex(blobs[i]);
                        }

                        break;
                    }
                case "tx_metadata_list":
                    {
                        List<string> metadatas = ((JArray)val).ToObject<List<string>>()!;
                        for (int i = 0; i < metadatas.Count; i++)
                        {
                            txs[i].SetMetadata(metadatas[i]);
                        }

                        break;
                    }
                case "fee_list":
                    {
                        List<ulong> fees = ((JArray)val).ToObject<List<ulong>>()!;
                        for (int i = 0; i < fees.Count; i++)
                        {
                            txs[i].SetFee(fees[i]);
                        }

                        break;
                    }
                case "amount_list":
                    {
                        List<ulong> amounts = ((JArray)val).ToObject<List<ulong>>()!;
                        for (int i = 0; i < amounts.Count; i++)
                        {
                            if (txs[i].GetOutgoingTransfer() == null)
                            {
                                txs[i].SetOutgoingTransfer(new MoneroOutgoingTransfer().SetTx(txs[i]));
                            }

                            txs[i].GetOutgoingTransfer()!.SetAmount(amounts[i]);
                        }

                        break;
                    }
                case "weight_list":
                    {
                        List<ulong> weights = ((JArray)val).ToObject<List<ulong>>()!;
                        for (int i = 0; i < weights.Count; i++)
                        {
                            txs[i].SetWeight(weights[i]);
                        }

                        break;
                    }
                case "multisig_txset":
                case "unsigned_txset":
                case "signed_txset":
                    // handled elsewhere
                    break;
                case "spent_key_images_list":
                    {
                        List<MoneroJsonRpcParams> inputKeyImagesList =
                            ((JArray)val).ToObject<List<Dictionary<string, object?>>>()!;
                        for (int i = 0; i < inputKeyImagesList.Count; i++)
                        {
                            if (txs[i].GetInputs() != null)
                            {
                                throw new Exception("Expected null inputs");
                            }

                            txs[i].SetInputsWallet([]);
                            foreach (string inputKeyImage in ((JArray)inputKeyImagesList[i]["key_images"]!)
                                     .ToObject<List<string>>()!)
                            {
                                txs[i].GetInputs()!.Add(new MoneroOutputWallet()
                                    .SetKeyImage(new MoneroKeyImage().SetHex(inputKeyImage)).SetTx(txs[i]));
                            }
                        }

                        break;
                    }
                case "amounts_by_dest_list":
                    {
                        List<MoneroJsonRpcParams> amountsByDestList =
                            ((JArray)val).ToObject<List<Dictionary<string, object?>>>()!;
                        int destinationIdx = 0;
                        for (int txIdx = 0; txIdx < amountsByDestList.Count; txIdx++)
                        {
                            List<ulong> amountsByDest = ((JArray)amountsByDestList[txIdx]["amounts"]!).ToObject<List<ulong>>()!;
                            if (txs[txIdx].GetOutgoingTransfer() == null)
                            {
                                txs[txIdx].SetOutgoingTransfer(new MoneroOutgoingTransfer().SetTx(txs[txIdx]));
                            }

                            txs[txIdx].GetOutgoingTransfer()!.SetDestinations([]);
                            foreach (ulong amount in amountsByDest)
                            {
                                if (config == null)
                                {
                                    throw new MoneroError("Must provide a valid tx config");
                                }

                                if (config.GetDestinations()!.Count == 1)
                                {
                                    txs[txIdx].GetOutgoingTransfer()!.GetDestinations()!
                                        .Add(new MoneroDestination(config.GetDestinations()![0].GetAddress(),
                                            amount)); // sweeping can create multiple withone address
                                }
                                else
                                {
                                    txs[txIdx].GetOutgoingTransfer()!.GetDestinations()!.Add(
                                        new MoneroDestination(config.GetDestinations()![destinationIdx++].GetAddress(),
                                            amount));
                                }
                            }
                        }

                        break;
                    }
                default:
                    MoneroUtils.Log(0, "ignoring unexpected transaction list field: " + key + ": " + val);
                    break;
            }
        }

        return txSet;
    }

    private static MoneroTxSet ConvertRpcTxToTxSet(Dictionary<string, object?>? rpcTx, MoneroTxWallet? tx,
        bool isOutgoing,
        MoneroTxConfig config)
    {
        MoneroTxSet txSet = ConvertRpcTxSet(rpcTx);
        txSet.SetTxs([ConvertRpcTxWithTransfer(rpcTx, tx, isOutgoing, config).SetTxSet(txSet)]);
        return txSet;
    }

    private static MoneroTxWallet ConvertRpcTxWithTransfer(Dictionary<string, object?>? rpcTx, MoneroTxWallet? tx,
        bool? isOutgoing, MoneroTxConfig? config)
    {
        // TODO: change everything to safe set

        bool? _isOutgoing = isOutgoing;

        // initialize tx to return
        if (tx == null)
        {
            tx = new MoneroTxWallet();
        }

        if (rpcTx == null)
        {
            throw new MoneroError("RPC tx is null");
        }

        // initialize tx state from rpc type
        if (rpcTx.ContainsKey("type"))
        {
            _isOutgoing = DecodeRpcType((string?)rpcTx["type"]!, tx);
        }
        else if (_isOutgoing == null)
        {
            throw new MoneroError("Must indicate if tx is outgoing (true) xor incoming (false) since unknown");
        }

        // TODO: safe set
        // initialize remaining fields  TODO: seems this should be part of common function with DaemonRpc._convertRpcTx
        MoneroBlockHeader? header = null;
        MoneroTransfer? transfer = null;
        foreach (string key in rpcTx.Keys)
        {
            object? val = rpcTx[key];
            switch (key)
            {
                case "txid":
                case "tx_hash":
                    tx.SetHash((string?)val);
                    break;
                case "fee":
                    tx.SetFee(Convert.ToUInt64(val));
                    break;
                case "note":
                    {
                        if (!"".Equals(val))
                        {
                            tx.SetNote((string?)val);
                        }

                        break;
                    }
                case "tx_key":
                    tx.SetKey((string?)val);
                    break;
                case "type":
                    // type already handled
                    break;
                case "tx_size":
                    tx.SetSize(Convert.ToUInt64(val));
                    break;
                case "unlock_time":
                    tx.SetUnlockTime(Convert.ToUInt64(val));
                    break;
                case "weight":
                    tx.SetWeight(Convert.ToUInt64(val));
                    break;
                case "locked":
                    tx.SetIsLocked((bool?)val);
                    break;
                case "tx_blob":
                    tx.SetFullHex((string?)val);
                    break;
                case "tx_metadata":
                    tx.SetMetadata((string?)val);
                    break;
                case "double_spend_seen":
                    tx.SetIsDoubleSpendSeen((bool?)val);
                    break;
                case "block_height":
                case "height":
                    {
                        if (tx.IsConfirmed() == true)
                        {
                            if (header == null)
                            {
                                header = new MoneroBlockHeader();
                            }

                            header.SetHeight(Convert.ToUInt64(val));
                        }

                        break;
                    }
                case "timestamp":
                    {
                        if (tx.IsConfirmed() == true)
                        {
                            if (header == null)
                            {
                                header = new MoneroBlockHeader();
                            }

                            header.SetTimestamp(Convert.ToUInt64(val));
                        }

                        // timestamp of unconfirmed tx is current request time
                        break;
                    }
                case "confirmations":
                    tx.SetNumConfirmations(Convert.ToUInt64(val));
                    break;
                case "suggested_confirmations_threshold":
                    {
                        if (transfer == null)
                        {
                            transfer = _isOutgoing == true
                                ? new MoneroOutgoingTransfer()
                                : new MoneroIncomingTransfer();
                            transfer.SetTx(tx);
                        }

                        if (_isOutgoing != true)
                        {
                            ((MoneroIncomingTransfer)transfer).SetNumSuggestedConfirmations(Convert.ToUInt64(val));
                        }

                        break;
                    }
                case "amount":
                    {
                        if (transfer == null)
                        {
                            transfer = _isOutgoing == true
                                ? new MoneroOutgoingTransfer()
                                : new MoneroIncomingTransfer();
                            transfer.SetTx(tx);
                        }

                        transfer.SetAmount(Convert.ToUInt64(val));
                        break;
                    }
                case "amounts":
                    // ignoring, amounts sum to amount
                    break;
                case "address":
                    {
                        if (_isOutgoing != true)
                        {
                            if (transfer == null)
                            {
                                transfer = new MoneroIncomingTransfer().SetTx(tx);
                            }

                            ((MoneroIncomingTransfer)transfer).SetAddress((string?)val);
                        }

                        break;
                    }
                case "payment_id":
                    {
                        if (!"".Equals(val) && !MoneroTx.DefaultPaymentId.Equals(val))
                        {
                            tx.SetPaymentId((string?)val); // default is undefined
                        }

                        break;
                    }
                case "subaddr_index":
                    {
                        if (!rpcTx.ContainsKey("subaddr_indices"))
                        {
                            throw new MoneroError("subaddress indices not found"); // handled by subaddr_indices
                        }

                        break;
                    }
                case "subaddr_indices":
                    {
                        if (transfer == null)
                        {
                            transfer = _isOutgoing == true
                                ? new MoneroOutgoingTransfer()
                                : new MoneroIncomingTransfer();
                            transfer.SetTx(tx);
                        }

                        List<Dictionary<string, uint>> rpcIndices =
                            ((JArray)val!).ToObject<List<Dictionary<string, uint>>>()!;
                        transfer.SetAccountIndex(rpcIndices[0]["major"]);
                        if (_isOutgoing == true)
                        {
                            List<uint> subaddressIndices = [];
                            foreach (Dictionary<string, uint> rpcIndex in rpcIndices)
                            {
                                subaddressIndices.Add(rpcIndex["minor"]);
                            }

                            ((MoneroOutgoingTransfer)transfer).SetSubaddressIndices(subaddressIndices);
                        }
                        else
                        {
                            if (rpcIndices.Count != 1)
                            {
                                throw new MoneroError(
                                    "Expected exactly one subaddress index for incoming transfer, but got " +
                                    rpcIndices.Count);
                            }

                            ((MoneroIncomingTransfer)transfer).SetSubaddressIndex(rpcIndices[0]["minor"]);
                        }

                        break;
                    }
                case "destinations":
                case "recipients":
                    {
                        if (_isOutgoing != true)
                        {
                            throw new MoneroError("Expected outgoing transfer, but got incoming transfer");
                        }

                        List<MoneroDestination> destinations = [];
                        foreach (MoneroJsonRpcParams rpcDestination in ((JArray)val!)
                                 .ToObject<List<Dictionary<string, object?>>>()!)
                        {
                            MoneroDestination destination = new();
                            destinations.Add(destination);
                            foreach (string destinationKey in rpcDestination.Keys)
                            {
                                switch (destinationKey)
                                {
                                    case "address":
                                        destination.SetAddress((string?)rpcDestination[destinationKey]);
                                        break;
                                    case "amount":
                                        destination.SetAmount((ulong?)rpcDestination[destinationKey]);
                                        break;
                                    default:
                                        throw new MoneroError("Unrecognized transaction destination field: " +
                                                              destinationKey);
                                }
                            }
                        }

                        if (transfer == null)
                        {
                            transfer = new MoneroOutgoingTransfer().SetTx(tx);
                        }

                        ((MoneroOutgoingTransfer)transfer).SetDestinations(destinations);
                        break;
                    }
                case "multisig_txset" when val != null:
                case "unsigned_txset" when val != null:
                    // handled elsewhere; this method only builds a tx wallet
                    break;
                case "amount_in":
                    tx.SetInputSum(Convert.ToUInt64(val));
                    break;
                case "amount_out":
                    tx.SetOutputSum(Convert.ToUInt64(val));
                    break;
                case "change_address":
                    {
                        if (val != null)
                        {
                            tx.SetChangeAddress("".Equals(val) ? null : (string?)val);
                        }

                        break;
                    }
                case "change_amount":
                    tx.SetChangeAmount(Convert.ToUInt64(val));
                    break;
                case "dummy_outputs":
                    tx.SetNumDummyOutputs(Convert.ToUInt32(val));
                    break;
                case "extra":
                    {
                        if (val != null)
                        {
                            tx.SetExtraHex((string)val);
                        }

                        break;
                    }
                case "ring_size":
                    tx.SetRingSize(Convert.ToUInt32(val));
                    break;
                case "spent_key_images":
                    if (val is MoneroJsonRpcParams dict &&
                        dict.TryGetValue("key_images", out object? keyImagesObj) &&
                        keyImagesObj is List<string> inputKeyImages)
                    {
                        if (tx.GetInputs() != null)
                        {
                            throw new MoneroError("Expected null inputs");
                        }

                        tx.SetInputs([]);
                        foreach (string inputKeyImage in inputKeyImages)
                        {
                            tx.GetInputs()!.Add(
                                new MoneroOutputWallet()
                                    .SetKeyImage(new MoneroKeyImage().SetHex(inputKeyImage))
                                    .SetTx(tx)
                            );
                        }
                    }
                    else
                    {
                        throw new MoneroError("Invalid or missing key_images in spent_key_images");
                    }

                    break;
                case "amounts_by_dest" when _isOutgoing != true:
                    throw new MoneroError("Expected outgoing transfer, but got incoming transfer");
                case "amounts_by_dest":
                    {
                        Dictionary<string, object>? valMap = ((JObject)val!).ToObject<Dictionary<string, object>>();
                        if (valMap != null)
                        {
                            List<ulong> amountsByDest = ((JArray)valMap["amounts"]).ToObject<List<ulong>>() ?? [];

                            if (config == null)
                            {
                                throw new MoneroError("Must provide a valid tx config");
                            }

                            if (config.GetDestinations()!.Count != amountsByDest.Count)
                            {
                                throw new MoneroError("Expected " + config.GetDestinations()!.Count +
                                                      " destinations, but got " + amountsByDest.Count);
                            }

                            if (transfer == null)
                            {
                                transfer = new MoneroOutgoingTransfer().SetTx(tx);
                            }

                            ((MoneroOutgoingTransfer)transfer).SetDestinations([]);
                            for (int i = 0; i < config.GetDestinations()!.Count; i++)
                            {
                                ((MoneroOutgoingTransfer)transfer).GetDestinations()!
                                    .Add(new MoneroDestination(config.GetDestinations()![i].GetAddress(),
                                        amountsByDest[i]));
                            }
                        }
                        else
                        {
                            throw new MoneroError("Invalid or missing 'amounts' in 'amounts_by_dest'");
                        }

                        break;
                    }
                default:
                    MoneroUtils.Log(0, "ignoring unexpected transaction field with transfer: " + key + ": " + val);
                    break;
            }
        }

        // link block and tx
        if (header != null)
        {
            tx.SetBlock(new MoneroBlock(header).SetTxs(tx));
        }

        // initialize final fields
        if (transfer != null)
        {
            if (tx.IsConfirmed() == null)
            {
                tx.SetIsConfirmed(false);
            }

            if (transfer.GetTx()!.IsConfirmed() == false)
            {
                tx.SetNumConfirmations(0L);
            }

            if (_isOutgoing == true)
            {
                tx.SetIsOutgoing(true);
                if (tx.GetOutgoingTransfer() != null)
                {
                    if (((MoneroOutgoingTransfer)transfer).GetDestinations() != null)
                    {
                        tx.GetOutgoingTransfer()!
                            .SetDestinations(
                                null); // overwrite to avoid Reconcile error TODO: remove after >18.3.1 when amounts_by_dest supported
                    }

                    tx.GetOutgoingTransfer()!.Merge(transfer);
                }
                else
                {
                    tx.SetOutgoingTransfer((MoneroOutgoingTransfer)transfer);
                }
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

    private static MoneroTxWallet ConvertRpcTxWithOutput(Dictionary<string, object?> rpcOutput)
    {
        // initialize tx
        MoneroTxWallet tx = new();
        tx.SetIsConfirmed(true);
        tx.SetIsRelayed(true);
        tx.SetIsFailed(false);

        // initialize output
        MoneroOutputWallet output = new MoneroOutputWallet().SetTx(tx);
        foreach (string key in rpcOutput.Keys)
        {
            object val = rpcOutput[key]!;
            switch (key)
            {
                case "amount":
                    output.SetAmount(Convert.ToUInt64(val));
                    break;
                case "spent":
                    output.SetIsSpent((bool)val);
                    break;
                case "key_image":
                    {
                        if (!"".Equals(val))
                        {
                            output.SetKeyImage(new MoneroKeyImage((string)val));
                        }

                        break;
                    }
                case "global_index":
                    output.SetIndex(Convert.ToUInt64(val));
                    break;
                case "tx_hash":
                    tx.SetHash((string)val);
                    break;
                case "unlocked":
                    tx.SetIsLocked(!(bool)val);
                    break;
                case "frozen":
                    output.SetIsFrozen((bool)val);
                    break;
                case "pubkey":
                    output.SetStealthPublicKey((string)val);
                    break;
                case "subaddr_index":
                    {
                        Dictionary<string, uint> rpcIndices = (Dictionary<string, uint>)val;
                        output.SetAccountIndex(rpcIndices["major"]);
                        output.SetSubaddressIndex(rpcIndices["minor"]);
                        break;
                    }
                case "block_height":
                    {
                        ulong height = Convert.ToUInt64(val);
                        tx.SetBlock(new MoneroBlock().SetHeight(height).SetTxs(tx));
                        break;
                    }
                default:
                    MoneroUtils.Log(0, "ignoring unexpected transaction field with output: " + key + ": " + val);
                    break;
            }
        }

        // initialize tx with output
        List<MoneroOutput> outputs = [];
        outputs.Add(output);
        tx.SetOutputs(outputs);
        return tx;
    }

    private static MoneroTxSet ConvertRpcDescribeTransfer(Dictionary<string, object?>? rpcDescribeTransferResult)
    {
        if (rpcDescribeTransferResult == null)
        {
            throw new MoneroError("Cannot convert null transfer description");
        }

        MoneroTxSet txSet = new();
        foreach (string key in rpcDescribeTransferResult.Keys)
        {
            object val = rpcDescribeTransferResult[key]!;
            if (key.Equals("desc"))
            {
                txSet.SetTxs([]);
                foreach (MoneroJsonRpcParams txMap in ((JArray)val).ToObject<List<Dictionary<string, object?>>>()!)
                {
                    MoneroTxWallet tx = ConvertRpcTxWithTransfer(txMap, null, true, null);
                    tx.SetTxSet(txSet);
                    txSet.GetTxs()!.Add(tx);
                }
            }
            else if (key.Equals("summary"))
            {
                // TODO: support tx set summary fields?
            }
            else
            {
                MoneroUtils.Log(0, "ignoring unexpected describe transfer field: " + key + ": " + val);
            }
        }

        return txSet;
    }

    private static bool DecodeRpcType(string? rpcType, MoneroTxWallet? tx)
    {
        if (tx == null)
        {
            throw new MoneroError("Tx is null");
        }

        if (rpcType == null)
        {
            throw new MoneroError("Rpc type is null");
        }

        bool isIn = rpcType.Equals("in");
        bool isOut = rpcType.Equals("out");
        bool isPool = rpcType.Equals("pool");
        bool isPending = rpcType.Equals("pending");
        bool isBlock = rpcType.Equals("block");
        bool isFailed = rpcType.Equals("failed");

        bool isValid = isIn || isOut || isPool || isPending || isBlock || isFailed;

        if (!isValid)
        {
            throw new MoneroError("Unrecognized transfer type: " + rpcType);
        }

        tx.SetIsConfirmed(isIn || isOut || isBlock);
        tx.SetInTxPool(isPool || isPending);
        tx.SetIsRelayed(true);
        tx.SetRelay(true);
        tx.SetIsFailed(isFailed);
        tx.SetIsMinerTx(isBlock);

        return isOut || isPending || isFailed;
    }

    private static void MergeTx(MoneroTxWallet tx, Dictionary<string, MoneroTxWallet> txMap,
        Dictionary<ulong, MoneroBlock?> blockMap)
    {
        if (tx.GetHash() == null)
        {
            throw new MoneroError("Cannot merge transaction without hash");
        }

        if (tx.GetHeight() == null)
        {
            throw new MoneroError("Cannot merge transaction without height");
        }

        string txHash = tx.GetHash()!;
        ulong txHeight = (ulong)tx.GetHeight()!;
        // merge tx
        MoneroTxWallet? aTx = txMap.GetValueOrDefault(txHash);
        if (aTx == null)
        {
            txMap.Add(txHash, tx); // cache new tx
        }
        else
        {
            aTx.Merge(tx); // merge with existing tx
        }

        // merge tx's block if confirmed
        if (tx.GetHeight() != null)
        {
            MoneroBlock? aBlock = blockMap.GetValueOrDefault(txHeight);
            if (aBlock == null)
            {
                blockMap.Add(txHeight, tx.GetBlock()); // cache new block
            }
            else
            {
                aBlock.Merge(tx.GetBlock()); // merge with an existing block
            }
        }
    }

    public async Task<string> GetPrimaryAddress()
    {
        string? address = await GetAddress(0, 0);

        if (address == null)
        {
            throw new MoneroError("Could not get primary address");
        }

        return address;
    }

    private static MoneroTransferQuery NormalizeTransferQuery(MoneroTransferQuery? query)
    {
        if (query == null)
        {
            query = new MoneroTransferQuery();
        }
        else
        {
            if (query.GetTxQuery() == null)
            {
                query = query.Clone();
            }
            else
            {
                MoneroTxQuery txQuery = query.GetTxQuery()!.Clone();
                if (query.GetTxQuery()!.GetTransferQuery() == query)
                {
                    query = txQuery.GetTransferQuery()!;
                }
                else
                {
                    if (null != query.GetTxQuery()!.GetTransferQuery())
                    {
                        throw new MoneroError("Transfer query's tx query must be circular reference or null");
                    }

                    query = query.Clone();
                    query.SetTxQuery(txQuery);
                }
            }
        }

        if (query.GetTxQuery() == null)
        {
            query.SetTxQuery(new MoneroTxQuery());
        }

        query.GetTxQuery()!.SetTransferQuery(query);
        if (query.GetTxQuery()!.GetBlock() == null)
        {
            query.GetTxQuery()!.SetBlock(new MoneroBlock().SetTxs(query.GetTxQuery()));
        }

        return query;
    }

    public async Task<bool> IsMultisig()
    {
        MoneroMultisigInfo info = await GetMultisigInfo();
        return info.IsMultisig() == true;
    }

    public Task<MoneroNetworkType> GetNetworkType()
    {
        throw new NotImplementedException();
    }

    public async Task<MoneroTxWallet> CreateTx(MoneroTxConfig config)
    {
        if (config == null)
        {
            throw new MoneroError("Send request cannot be null");
        }

        if (config.GetCanSplit() == true)
        {
            throw new MoneroError(
                "Cannot request split transactions with createTx() which prevents splitting; use createTxs() instead");
        }

        config = config.Clone();
        config.SetCanSplit(false);
        return (await CreateTxs(config))[0];
    }

    public async Task<MoneroSubaddress> GetSubaddress(uint accountIdx, uint subaddressIdx)
    {
        List<MoneroSubaddress> subaddresses = await GetSubaddresses(accountIdx, false, [subaddressIdx]);
        if (subaddresses.Count == 0)
        {
            throw new MoneroError("Subaddress at index " + subaddressIdx + " is not initialized");
        }

        if (1 != subaddresses.Count)
        {
            throw new MoneroError("Only 1 subaddress should be returned");
        }

        return subaddresses[0];
    }

    #endregion
}