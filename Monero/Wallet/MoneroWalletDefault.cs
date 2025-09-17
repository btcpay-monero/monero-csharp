using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Wallet;

public abstract class MoneroWalletDefault : MoneroWallet
{
    protected MoneroConnectionManager? _connectionManager;
    protected MoneroConnectionManagerListener? _connectionManagerListener;
    protected bool _isClosed;
    protected readonly List<MoneroWalletListener> _listeners = [];

    public abstract MoneroWalletType GetWalletType();

    public abstract Task<MoneroNetworkType> GetNetworkType();

    public abstract Task<int> AddAddressBookEntry(string address, string description);

    public virtual void AddListener(MoneroWalletListener listener)
    {
        lock (_listeners)
        {
            if (listener == null)
            {
                throw new MoneroError("Cannot add null listener");
            }

            _listeners.Add(listener);
        }
    }

    public abstract Task ChangePassword(string oldPassword, string newPassword);

    public abstract Task<MoneroCheckReserve> CheckReserveProof(string address, string message, string signature);

    public abstract Task<bool> CheckSpendProof(string txHash, string message, string signature);

    public abstract Task<MoneroCheckTx> CheckTxKey(string txHash, string txKey, string address);

    public abstract Task<MoneroCheckTx> CheckTxProof(string txHash, string address, string message, string signature);

    public virtual async Task Close()
    {
        await Close(false);
    }

    public abstract Task Close(bool save);

    protected virtual void CloseInternal(bool save)
    {
        if (_connectionManager != null && _connectionManagerListener != null)
        {
            _connectionManager.RemoveListener(_connectionManagerListener);
        }

        _connectionManager = null;
        _connectionManagerListener = null;
        _listeners.Clear();
        _isClosed = true;
    }

    public async Task<MoneroAccount> CreateAccount()
    {
        return await CreateAccount(null);
    }

    public abstract Task<MoneroAccount> CreateAccount(string? label);

    public async Task<MoneroSubaddress> CreateSubaddress(uint accountIdx)
    {
        return await CreateSubaddress(accountIdx, null);
    }

    public abstract Task<MoneroSubaddress> CreateSubaddress(uint accountIdx, string? label);

    public virtual async Task<MoneroTxWallet> CreateTx(MoneroTxConfig config)
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

    public abstract Task<List<MoneroTxWallet>> CreateTxs(MoneroTxConfig config);

    public abstract Task<MoneroIntegratedAddress> DecodeIntegratedAddress(string integratedAddress);

    public abstract Task DeleteAddressBookEntry(uint entryIdx);

    public virtual async Task<MoneroTxSet> DescribeMultisigTxSet(string multisigTxHex)
    {
        return await DescribeTxSet(new MoneroTxSet().SetMultisigTxHex(multisigTxHex));
    }

    public abstract Task<MoneroTxSet> DescribeTxSet(MoneroTxSet txSet);

    public virtual async Task<MoneroTxSet> DescribeUnsignedTxSet(string unsignedTxHex)
    {
        return await DescribeTxSet(new MoneroTxSet().SetUnsignedTxHex(unsignedTxHex));
    }

    public abstract Task EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription,
        string description);

    public abstract Task<MoneroMultisigInitResult> ExchangeMultisigKeys(List<string> multisigHexes, string password);

    public virtual async Task<List<MoneroKeyImage>> ExportKeyImages()
    {
        return await ExportKeyImages(false);
    }

    public abstract Task<List<MoneroKeyImage>> ExportKeyImages(bool all);

    public abstract Task<string> ExportMultisigHex();

    public virtual async Task<string> ExportOutputs()
    {
        return await ExportOutputs(false);
    }

    public abstract Task<string> ExportOutputs(bool all);

    public abstract Task FreezeOutput(string keyImage);

    public virtual async Task<MoneroAccount> GetAccount(uint accountIdx)
    {
        return await GetAccount(accountIdx, false);
    }

    public abstract Task<MoneroAccount> GetAccount(uint accountIdx, bool includeSubaddresses);

    public virtual async Task<List<MoneroAccount>> GetAccounts()
    {
        return await GetAccounts(false);
    }

    public virtual async Task<List<MoneroAccount>> GetAccounts(string tag)
    {
        return await GetAccounts(false, tag);
    }

    public virtual async Task<List<MoneroAccount>> GetAccounts(bool includeSubaddresses)
    {
        return await GetAccounts(includeSubaddresses, null);
    }

    public abstract Task<List<MoneroAccount>> GetAccounts(bool includeSubaddresses, string? tag);

    public abstract Task<List<MoneroAccountTag>> GetAccountTags();

    public abstract Task<string> GetAddress(uint accountIdx, uint subaddressIdx);

    public virtual async Task<List<MoneroAddressBookEntry>> GetAddressBookEntries()
    {
        return await GetAddressBookEntries(null);
    }

    public abstract Task<List<MoneroAddressBookEntry>> GetAddressBookEntries(List<uint>? entryIndices);

    public abstract Task<MoneroSubaddress> GetAddressIndex(string address);

    public abstract Task<string?> GetAttribute(string key);

    public virtual async Task<ulong> GetBalance()
    {
        return await GetBalance(null);
    }

    public virtual async Task<ulong> GetBalance(uint? accountIdx)
    {
        return await GetBalance(accountIdx, null);
    }

    public abstract Task<ulong> GetBalance(uint? accountIdx, uint? subaddressIdx);

    public virtual MoneroConnectionManager? GetConnectionManager()
    {
        return _connectionManager;
    }

    public abstract Task<MoneroRpcConnection?> GetDaemonConnection();

    public abstract Task<ulong> GetDaemonHeight();

    public abstract Task<MoneroTxPriority> GetDefaultFeePriority();

    public abstract Task<ulong> GetHeight();

    public abstract Task<ulong> GetHeightByDate(int year, int month, int day);

    public virtual async Task<List<MoneroIncomingTransfer>> GetIncomingTransfers()
    {
        return await GetIncomingTransfers(new MoneroTransferQuery());
    }

    public virtual async Task<List<MoneroIncomingTransfer>> GetIncomingTransfers(MoneroTransferQuery query)
    {
        // copy query and set direction
        query = NormalizeTransferQuery(query);
        if (query.IsIncoming() == false)
        {
            throw new MoneroError("Transfer query contradicts getting incoming transfers");
        }

        query.SetIsIncoming(true);

        // fetch and cast transfers
        List<MoneroIncomingTransfer> inTransfers = [];
        foreach (MoneroTransfer transfer in await GetTransfers(query))
        {
            inTransfers.Add((MoneroIncomingTransfer)transfer);
        }

        return inTransfers;
    }

    public virtual async Task<MoneroIntegratedAddress> GetIntegratedAddress()
    {
        return await GetIntegratedAddress(null);
    }

    public virtual async Task<MoneroIntegratedAddress> GetIntegratedAddress(string? standardAddress)
    {
        return await GetIntegratedAddress(standardAddress, null);
    }

    public abstract Task<MoneroIntegratedAddress> GetIntegratedAddress(string? standardAddress, string? paymentId);

    public virtual List<MoneroWalletListener> GetListeners()
    {
        return [.. _listeners];
    }

    public abstract Task<MoneroMultisigInfo> GetMultisigInfo();

    public abstract Task<List<MoneroKeyImage>> GetNewKeyImagesFromLastImport();

    public virtual async Task<List<MoneroOutgoingTransfer>> GetOutgoingTransfers()
    {
        return await GetOutgoingTransfers(new MoneroTransferQuery());
    }

    public virtual async Task<List<MoneroOutgoingTransfer>> GetOutgoingTransfers(MoneroTransferQuery query)
    {
        // copy query and set direction
        query = NormalizeTransferQuery(query);
        if (query.IsOutgoing() == false)
        {
            throw new MoneroError("Transfer query contradicts getting outgoing transfers");
        }

        query.SetIsOutgoing(true);

        // fetch and cast transfers
        List<MoneroOutgoingTransfer> outTransfers = [];
        foreach (MoneroTransfer transfer in await GetTransfers(query))
        {
            outTransfers.Add((MoneroOutgoingTransfer)transfer);
        }

        return outTransfers;
    }

    public virtual async Task<List<MoneroOutputWallet>> GetOutputs()
    {
        return await GetOutputs(new MoneroOutputQuery());
    }

    public abstract Task<List<MoneroOutputWallet>> GetOutputs(MoneroOutputQuery query);

    public abstract Task<string> GetPath();

    public abstract Task<string> GetPaymentUri(MoneroTxConfig config);

    public virtual async Task<string> GetPrimaryAddress()
    {
        return await GetAddress(0, 0);
    }

    public abstract Task<string> GetPrivateSpendKey();

    public abstract Task<string> GetPrivateViewKey();

    public abstract Task<string> GetPublicSpendKey();

    public abstract Task<string> GetPublicViewKey();

    public abstract Task<string> GetReserveProofAccount(uint accountIdx, ulong amount, string message);

    public abstract Task<string> GetReserveProofWallet(string message);

    public abstract Task<string> GetSeed();

    public abstract Task<string> GetSeedLanguage();

    public virtual async Task<string> GetSpendProof(string txHash)
    {
        return await GetSpendProof(txHash, null);
    }

    public abstract Task<string> GetSpendProof(string txHash, string? message);

    public virtual async Task<MoneroSubaddress> GetSubaddress(uint accountIdx, uint subaddressIdx)
    {
        List<MoneroSubaddress> subaddresses = await GetSubaddresses(accountIdx, [subaddressIdx]);
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

    public virtual async Task<List<MoneroSubaddress>> GetSubaddresses(uint accountIdx)
    {
        return await GetSubaddresses(accountIdx, null);
    }

    public abstract Task<List<MoneroSubaddress>> GetSubaddresses(uint accountIdx, List<uint>? subaddressIndices);

    public virtual async Task<List<MoneroTransfer>> GetTransfers()
    {
        return await GetTransfers(new MoneroTransferQuery());
    }

    public virtual async Task<List<MoneroTransfer>> GetTransfers(uint accountIdx)
    {
        MoneroTransferQuery query = new();
        query.SetAccountIndex(accountIdx);
        return await GetTransfers(query);
    }

    public virtual async Task<List<MoneroTransfer>> GetTransfers(uint accountIdx, uint subaddressIdx)
    {
        MoneroTransferQuery query = new();
        query.SetAccountIndex(accountIdx).SetSubaddressIndex(subaddressIdx);
        return await GetTransfers(query);
    }

    public abstract Task<List<MoneroTransfer>> GetTransfers(MoneroTransferQuery query);

    public virtual async Task<MoneroTxWallet?> GetTx(string txHash)
    {
        List<MoneroTxWallet> txs = await GetTxs([txHash]);

        if (txs.Count == 0)
        {
            return null;
        }

        return txs[0];
    }

    public abstract Task<string> GetTxKey(string txHash);

    public virtual async Task<string?> GetTxNote(string txHash)
    {
        List<string> notes = await GetTxNotes([txHash]);
        return notes.Count > 0 ? notes[0] : null;
    }

    public abstract Task<List<string>> GetTxNotes(List<string> txHashes);

    public virtual async Task<string> GetTxProof(string txHash, string address)
    {
        return await GetTxProof(txHash, address, null);
    }

    public abstract Task<string> GetTxProof(string txHash, string address, string? message);

    public virtual async Task<List<MoneroTxWallet>> GetTxs()
    {
        return await GetTxs(new MoneroTxQuery());
    }

    public virtual async Task<List<MoneroTxWallet>> GetTxs(List<string> txHashes)
    {
        return await GetTxs(new MoneroTxQuery().SetHashes(txHashes));
    }

    public abstract Task<List<MoneroTxWallet>> GetTxs(MoneroTxQuery? query);

    public virtual async Task<ulong> GetUnlockedBalance()
    {
        return await GetBalance(null);
    }

    public virtual async Task<ulong> GetUnlockedBalance(uint? accountIdx)
    {
        return await GetBalance(accountIdx, null);
    }

    public abstract Task<ulong> GetUnlockedBalance(uint? accountIdx, uint? subaddressIdx);

    public abstract Task<MoneroVersion> GetVersion();

    public abstract Task<MoneroKeyImageImportResult> ImportKeyImages(List<MoneroKeyImage> keyImages);

    public abstract Task<int> ImportMultisigHex(List<string> multisigHexes);

    public abstract Task<int> ImportOutputs(string outputsHex);

    public virtual Task<bool> IsClosed()
    {
        return Task.FromResult(_isClosed);
    }

    public abstract Task<bool> IsConnectedToDaemon();

    public virtual async Task<bool> IsMultisig()
    {
        MoneroMultisigInfo info = await GetMultisigInfo();
        return info.IsMultisig() == true;
    }

    public abstract Task<bool> IsMultisigImportNeeded();

    public abstract Task<bool> IsOutputFrozen(string keyImage);

    public abstract Task<bool> IsViewOnly();

    public abstract Task<string> MakeMultisig(List<string> multisigHexes, int threshold, string password);

    public abstract Task<MoneroTxConfig> ParsePaymentUri(string uri);

    public abstract Task<string> PrepareMultisig();

    public virtual async Task<string> RelayTx(string txMetadata)
    {
        List<string> hashes = await RelayTxs([txMetadata]);
        if (hashes.Count == 0)
        {
            return "";
        }

        return hashes[0];
    }

    public virtual async Task<string> RelayTx(MoneroTxWallet tx)
    {
        string? metadata = tx.GetMetadata();

        if (metadata == null)
        {
            throw new MoneroError("Cannot relay tx, metadata is null");
        }

        return await RelayTx(metadata);
    }

    public abstract Task<List<string>> RelayTxs(List<string> txMetadatas);

    public virtual async Task<List<string>> RelayTxs(List<MoneroTxWallet> txs)
    {
        List<string> txMetadatas = [];

        foreach (MoneroTxWallet tx in txs)
        {
            string? metadata = tx.GetMetadata();

            if (metadata == null)
            {
                throw new MoneroError($"Cannot relay tx {tx.GetHash()}, metadata is null");
            }

            txMetadatas.Add(metadata);
        }

        return await RelayTxs(txMetadatas);
    }

    public virtual void RemoveListener(MoneroWalletListener listener)
    {
        lock (_listeners)
        {
            _listeners.Remove(listener);
        }
    }

    public abstract Task RescanBlockchain();

    public abstract Task RescanSpent();

    public abstract Task Save();

    public abstract Task ScanTxs(List<string> txHashes);

    public virtual async Task SetAccountLabel(uint accountIdx, string label)
    {
        await SetSubaddressLabel(accountIdx, 0, label);
    }

    public abstract Task SetAccountTagLabel(string tag, string label);

    public abstract Task SetAttribute(string key, string val);

    public virtual async Task SetConnectionManager(MoneroConnectionManager? connectionManager)
    {
        if (this._connectionManager != null && _connectionManagerListener != null)
        {
            this._connectionManager.RemoveListener(_connectionManagerListener);
        }

        this._connectionManager = connectionManager;
        if (connectionManager == null)
        {
            return;
        }

        if (_connectionManagerListener == null)
        {
            _connectionManagerListener = new MoneroWalletConnectionManagerListener(this);
        }

        connectionManager.AddListener(_connectionManagerListener);
        await SetDaemonConnection(connectionManager.GetConnection());
    }

    public virtual async Task SetDaemonConnection(string uri)
    {
        await SetDaemonConnection(uri, null, null);
    }

    public virtual async Task SetDaemonConnection(string? uri, string? username, string? password)
    {
        if (uri == null)
        {
            await SetDaemonConnection((MoneroRpcConnection?)null);
        }
        else
        {
            await SetDaemonConnection(new MoneroRpcConnection(uri, username, password));
        }
    }

    public abstract Task SetDaemonConnection(MoneroRpcConnection? daemonConnection);

    public abstract Task SetProxyUri(string? uri);

    public abstract Task SetSubaddressLabel(uint accountIdx, uint subaddressIdx, string label);

    public virtual async Task SetTxNote(string txHash, string note)
    {
        await SetTxNotes([txHash], [note]);
    }

    public abstract Task SetTxNotes(List<string> txHashes, List<string> notes);

    public virtual async Task<string> SignMessage(string message)
    {
        return await SignMessage(message, MoneroMessageSignatureType.SignWithSpendKey, 0, 0);
    }

    public virtual async Task<string> SignMessage(string message, MoneroMessageSignatureType signatureType)
    {
        return await SignMessage(message, signatureType, 0, 0);
    }

    public virtual async Task<string> SignMessage(string message, MoneroMessageSignatureType signatureType, uint accountIdx)
    {
        return await SignMessage(message, signatureType, accountIdx, 0);
    }

    public abstract Task<string> SignMessage(string message, MoneroMessageSignatureType signatureType, uint accountIdx, uint subaddressIdx);

    public abstract Task<MoneroMultisigSignResult> SignMultisigTxHex(string multisigTxHex);

    public abstract Task<MoneroTxSet> SignTxs(string unsignedTxHex);

    public abstract Task StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery);

    public virtual async Task StartSyncing()
    {
        await StartSyncing(null);
    }

    public abstract Task StartSyncing(ulong? syncPeriodInMs);

    public abstract Task StopMining();

    public abstract Task StopSyncing();

    public abstract Task<List<string>> SubmitMultisigTxHex(string signedMultisigTxHex);

    public abstract Task<List<string>> SubmitTxs(string signedTxHex);

    public abstract Task<List<MoneroTxWallet>> SweepDust(bool relay);

    public abstract Task<MoneroTxWallet> SweepOutput(MoneroTxConfig config);

    public abstract Task<List<MoneroTxWallet>> SweepUnlocked(MoneroTxConfig config);

    public virtual async Task<MoneroSyncResult> Sync()
    {
        return await Sync(null, null);
    }

    public virtual async Task<MoneroSyncResult> Sync(MoneroWalletListener listener)
    {
        return await Sync(null, listener);
    }

    public virtual async Task<MoneroSyncResult> Sync(ulong? startHeight)
    {
        return await Sync(startHeight, null);
    }

    public abstract Task<MoneroSyncResult> Sync(ulong? startHeight, MoneroWalletListener? listener);

    public abstract Task TagAccounts(string tag, List<uint> accountIndices);

    public abstract Task ThawOutput(string keyImage);

    public abstract Task UntagAccounts(List<uint> accountIndices);

    public abstract Task<MoneroMessageSignatureResult> VerifyMessage(string message, string address, string signature);

    protected static MoneroTransferQuery NormalizeTransferQuery(MoneroTransferQuery? query)
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
}

internal class MoneroWalletConnectionManagerListener : MoneroConnectionManagerListener
{
    private readonly MoneroWalletDefault _wallet;

    public MoneroWalletConnectionManagerListener(MoneroWalletDefault wallet)
    {
        this._wallet = wallet;
    }

    public void OnConnectionChanged(MoneroRpcConnection? connection)
    {
        _wallet.SetDaemonConnection(connection);
    }
}