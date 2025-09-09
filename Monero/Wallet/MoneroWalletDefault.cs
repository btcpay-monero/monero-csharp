
using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Wallet;

public abstract class MoneroWalletDefault : MoneroWallet
{
    protected MoneroConnectionManager? connectionManager;
    protected MoneroConnectionManagerListener? connectionManagerListener;
    protected List<MoneroWalletListener> listeners = [];
    protected bool isClosed = false;

    public abstract MoneroWalletType GetWalletType();

    public abstract MoneroNetworkType GetNetworkType();

    public abstract int AddAddressBookEntry(string address, string description);

    public virtual void AddListener(MoneroWalletListener listener)
    {
        lock (listeners)
        {
            if (listener == null) throw new MoneroError("Cannot add null listener");
            listeners.Add(listener);
        }
    }

    public abstract void ChangePassword(string oldPassword, string newPassword);

    public abstract MoneroCheckReserve CheckReserveProof(string address, string message, string signature);

    public abstract bool CheckSpendProof(string txHash, string message, string signature);

    public abstract MoneroCheckTx CheckTxKey(string txHash, string txKey, string address);

    public abstract MoneroCheckTx CheckTxProof(string txHash, string address, string message, string signature);

    public virtual void Close(bool save = false)
    {
        if (connectionManager != null && connectionManagerListener != null) connectionManager.RemoveListener(connectionManagerListener);
        connectionManager = null;
        connectionManagerListener = null;
        listeners.Clear();
        isClosed = true;
    }

    public abstract MoneroAccount CreateAccount(string? label = null);

    public abstract MoneroSubaddress CreateSubaddress(uint accountIdx, string? label = null);

    public virtual MoneroTxWallet CreateTx(MoneroTxConfig config)
    {
        if (config == null) throw new MoneroError("Send request cannot be null");
        if (config.GetCanSplit() == true) throw new MoneroError("Cannot request split transactions with createTx() which prevents splitting; use createTxs() instead");
        config = config.Clone();
        config.SetCanSplit(false);
        return CreateTxs(config)[0];
    }

    public abstract List<MoneroTxWallet> CreateTxs(MoneroTxConfig config);

    public abstract MoneroIntegratedAddress DecodeIntegratedAddress(string integratedAddress);

    public abstract void DeleteAddressBookEntry(uint entryIdx);

    public virtual MoneroTxSet DescribeMultisigTxSet(string multisigTxHex)
    {
        return DescribeTxSet(new MoneroTxSet().SetMultisigTxHex(multisigTxHex));
    }

    public abstract MoneroTxSet DescribeTxSet(MoneroTxSet txSet);

    public virtual MoneroTxSet DescribeUnsignedTxSet(string unsignedTxHex)
    {
        return DescribeTxSet(new MoneroTxSet().SetUnsignedTxHex(unsignedTxHex));
    }

    public abstract void EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription, string description);

    public abstract MoneroMultisigInitResult ExchangeMultisigKeys(List<string> multisigHexes, string password);

    public abstract List<MoneroKeyImage> ExportKeyImages(bool all = false);

    public abstract string ExportMultisigHex();

    public abstract string ExportOutputs(bool all = false);

    public abstract void FreezeOutput(string keyImage);

    public abstract MoneroAccount GetAccount(uint accountIdx, bool includeSubaddresses = false);

    public virtual List<MoneroAccount> GetAccounts(string tag)
    {
        return GetAccounts(false, tag);
    }

    public abstract List<MoneroAccount> GetAccounts(bool includeSubaddresses = false, string? tag = null);

    public abstract List<MoneroAccountTag> GetAccountTags();

    public abstract string GetAddress(uint accountIdx, uint subaddressIdx);

    public virtual List<MoneroAddressBookEntry> GetAddressBookEntries()
    {
        return GetAddressBookEntries(null);
    }

    public abstract List<MoneroAddressBookEntry> GetAddressBookEntries(List<uint>? entryIndices);

    public abstract MoneroSubaddress GetAddressIndex(string address);

    public abstract string? GetAttribute(string key);

    public abstract ulong GetBalance(uint? accountIdx = null, uint? subaddressIdx = null);

    public virtual MoneroConnectionManager? GetConnectionManager()
    {
        return connectionManager;
    }

    public abstract MoneroRpcConnection? GetDaemonConnection();

    public abstract ulong GetDaemonHeight();

    public abstract MoneroTxPriority GetDefaultFeePriority();

    public abstract ulong GetHeight();

    public abstract ulong GetHeightByDate(int year, int month, int day);

    public virtual List<MoneroIncomingTransfer> GetIncomingTransfers()
    {
        return GetIncomingTransfers(new MoneroTransferQuery());
    }

    public virtual List<MoneroIncomingTransfer> GetIncomingTransfers(MoneroTransferQuery query)
    {
        // copy query and set direction
        query = NormalizeTransferQuery(query);
        if (query.IsIncoming() == false) throw new MoneroError("Transfer query contradicts getting incoming transfers");
        query.SetIsIncoming(true);

        // fetch and cast transfers
        List<MoneroIncomingTransfer> inTransfers = [];
        foreach (MoneroTransfer transfer in GetTransfers(query))
        {
            inTransfers.Add((MoneroIncomingTransfer)transfer);
        }
        return inTransfers;
    }

    public abstract MoneroIntegratedAddress GetIntegratedAddress(string? standardAddress = null, string? paymentId = null);

    public virtual List<MoneroWalletListener> GetListeners()
    {
        return [.. listeners];
    }

    public abstract MoneroMultisigInfo GetMultisigInfo();

    public abstract List<MoneroKeyImage> GetNewKeyImagesFromLastImport();

    public virtual List<MoneroOutgoingTransfer> GetOutgoingTransfers()
    {
        return GetOutgoingTransfers(new MoneroTransferQuery());
    }

    public virtual List<MoneroOutgoingTransfer> GetOutgoingTransfers(MoneroTransferQuery query)
    {
        // copy query and set direction
        query = NormalizeTransferQuery(query);
        if (query.IsOutgoing() == false) throw new MoneroError("Transfer query contradicts getting outgoing transfers");
        query.SetIsOutgoing(true);

        // fetch and cast transfers
        List<MoneroOutgoingTransfer> outTransfers = [];
        foreach (MoneroTransfer transfer in GetTransfers(query))
        {
            outTransfers.Add((MoneroOutgoingTransfer)transfer);
        }
        return outTransfers;
    }

    public virtual List<MoneroOutputWallet> GetOutputs()
    {
        return GetOutputs(new MoneroOutputQuery());
    }

    public abstract List<MoneroOutputWallet> GetOutputs(MoneroOutputQuery query);

    public abstract string GetPath();

    public abstract string GetPaymentUri(MoneroTxConfig config);

    public virtual string GetPrimaryAddress()
    {
        return GetAddress(0, 0);
    }

    public abstract string GetPrivateSpendKey();

    public abstract string GetPrivateViewKey();

    public abstract string GetPublicSpendKey();

    public abstract string GetPublicViewKey();

    public abstract string GetReserveProofAccount(uint accountIdx, ulong amount, string message);

    public abstract string GetReserveProofWallet(string message);

    public abstract string GetSeed();

    public abstract string GetSeedLanguage();

    public abstract string GetSpendProof(string txHash, string? message = null);

    public virtual MoneroSubaddress GetSubaddress(uint accountIdx, uint subaddressIdx)
    {
        List<MoneroSubaddress> subaddresses = GetSubaddresses(accountIdx, [subaddressIdx]);
        if (subaddresses.Count == 0) throw new MoneroError("Subaddress at index " + subaddressIdx + " is not initialized");
        if (1 != subaddresses.Count) throw new MoneroError("Only 1 subaddress should be returned");
        return subaddresses[0];
    }

    public abstract List<MoneroSubaddress> GetSubaddresses(uint accountIdx, List<uint>? subaddressIndices = null);

    public virtual List<MoneroTransfer> GetTransfers()
    {
        return GetTransfers(new MoneroTransferQuery());
    }

    public virtual List<MoneroTransfer> GetTransfers(uint accountIdx)
    {
        var query = new MoneroTransferQuery();
        query.SetAccountIndex(accountIdx);
        return GetTransfers(query);
    }

    public virtual List<MoneroTransfer> GetTransfers(uint accountIdx, uint subaddressIdx)
    {
        var query = new MoneroTransferQuery();
        query.SetAccountIndex(accountIdx).SetSubaddressIndex(subaddressIdx);
        return GetTransfers(query);
    }

    public abstract List<MoneroTransfer> GetTransfers(MoneroTransferQuery query);

    public MoneroTxWallet? GetTx(string txHash)
    {
        var txs = GetTxs([txHash]);

        if (txs.Count == 0) return null;

        return txs[0];
    }

    public abstract string GetTxKey(string txHash);

    public virtual string? GetTxNote(string txHash)
    {
        var notes = GetTxNotes([txHash]);
        return notes.Count > 0 ? notes[0] : null;
    }

    public abstract List<string> GetTxNotes(List<string> txHashes);

    public abstract string GetTxProof(string txHash, string address, string? message = null);

    public virtual List<MoneroTxWallet> GetTxs()
    {
        return GetTxs(new MoneroTxQuery());
    }

    public virtual List<MoneroTxWallet> GetTxs(List<string> txHashes)
    {
        return GetTxs(new MoneroTxQuery().SetHashes(txHashes));
    }

    public abstract List<MoneroTxWallet> GetTxs(MoneroTxQuery query);

    public abstract ulong GetUnlockedBalance(uint? accountIdx = null, uint? subaddressIdx = null);

    public abstract MoneroVersion GetVersion();

    public abstract MoneroKeyImageImportResult ImportKeyImages(List<MoneroKeyImage> keyImages);

    public abstract int ImportMultisigHex(List<string> multisigHexes);

    public abstract int ImportOutputs(string outputsHex);

    public virtual bool IsClosed()
    {
        return isClosed;
    }

    public abstract bool IsConnectedToDaemon();

    public virtual bool IsMultisig()
    {
        return GetMultisigInfo().IsMultisig() == true;
    }

    public abstract bool IsMultisigImportNeeded();

    public abstract bool IsOutputFrozen(string keyImage);

    public abstract bool IsViewOnly();

    public abstract string MakeMultisig(List<string> multisigHexes, int threshold, string password);

    public abstract MoneroTxConfig ParsePaymentUri(string uri);

    public abstract string PrepareMultisig();

    public virtual string RelayTx(string txMetadata)
    {
        var hashes = RelayTxs([txMetadata]);
        if (hashes.Count == 0) return "";
        return hashes[0];
    }

    public virtual string RelayTx(MoneroTxWallet tx)
    {
        return RelayTx(tx.GetMetadata());
    }

    public abstract List<string> RelayTxs(List<string> txMetadatas);

    public virtual List<string> RelayTxs(List<MoneroTxWallet> txs)
    {
        List<string> txMetadatas = [];

        foreach (MoneroTxWallet tx in txs)
        {
            txMetadatas.Add(tx.GetMetadata());
        }

        return RelayTxs(txMetadatas);
    }

    public virtual void RemoveListener(MoneroWalletListener listener)
    {
        lock (listeners)
        {
            listeners.Remove(listener);
        }
    }

    public abstract void RescanBlockchain();

    public abstract void RescanSpent();

    public abstract void Save();

    public abstract void ScanTxs(List<string> txHashes);

    public virtual void SetAccountLabel(uint accountIdx, string label)
    {
        SetSubaddressLabel(accountIdx, 0, label);
    }

    public abstract void SetAccountTagLabel(string tag, string label);

    public abstract void SetAttribute(string key, string val);

    public virtual void SetConnectionManager(MoneroConnectionManager? connectionManager)
    {
        if (this.connectionManager != null && connectionManagerListener != null) this.connectionManager.RemoveListener(connectionManagerListener);
        this.connectionManager = connectionManager;
        if (connectionManager == null) return;

        if (connectionManagerListener == null)
        {
            connectionManagerListener = new MoneroWalletConnectionManagerListener(this);
        }

        connectionManager.AddListener(connectionManagerListener);
        SetDaemonConnection(connectionManager.GetConnection());
    }

    public virtual void SetDaemonConnection(string? uri, string? username = null, string? password = null)
    {
        if (uri == null) SetDaemonConnection((MoneroRpcConnection?)null);
        else SetDaemonConnection(new MoneroRpcConnection(uri, username, password));
    }

    public abstract void SetDaemonConnection(MoneroRpcConnection? daemonConnection);

    public abstract void SetProxyUri(string? uri);

    public abstract void SetSubaddressLabel(uint accountIdx, uint subaddressIdx, string label);

    public virtual void SetTxNote(string txHash, string note)
    {
        SetTxNotes([txHash], [note]);
    }

    public abstract void SetTxNotes(List<string> txHashes, List<string> notes);

    public abstract string SignMessage(string message, MoneroMessageSignatureType signatureType = MoneroMessageSignatureType.SIGN_WITH_SPEND_KEY, uint accountIdx = 0, uint subaddressIdx = 0);

    public abstract MoneroMultisigSignResult SignMultisigTxHex(string multisigTxHex);

    public abstract MoneroTxSet SignTxs(string unsignedTxHex);

    public abstract void StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery);

    public abstract void StartSyncing(ulong? SyncPeriodInMs = null);

    public abstract void StopMining();

    public abstract void StopSyncing();

    public abstract List<string> SubmitMultisigTxHex(string signedMultisigTxHex);

    public abstract List<string> SubmitTxs(string signedTxHex);

    public abstract List<MoneroTxWallet> SweepDust(bool relay);

    public abstract MoneroTxWallet SweepOutput(MoneroTxConfig config);

    public abstract List<MoneroTxWallet> SweepUnlocked(MoneroTxConfig config);

    public virtual MoneroSyncResult Sync(MoneroWalletListener listener)
    {
        return Sync(null, listener);
    }

    public abstract MoneroSyncResult Sync(ulong? startHeight = null, MoneroWalletListener? listener = null);

    public abstract void TagAccounts(string tag, List<uint> accountIndices);

    public abstract void ThawOutput(string keyImage);

    public abstract void UntagAccounts(List<uint> accountIndices);

    public abstract MoneroMessageSignatureResult VerifyMessage(string message, string address, string signature);

    protected static MoneroTransferQuery NormalizeTransferQuery(MoneroTransferQuery query)
    {
        if (query == null) query = new MoneroTransferQuery();
        else
        {
            if (query.GetTxQuery() == null) query = query.Clone();
            else
            {
                MoneroTxQuery txQuery = query.GetTxQuery().Clone();
                if (query.GetTxQuery().GetTransferQuery() == query) query = txQuery.GetTransferQuery();
                else
                {
                    if (null != query.GetTxQuery().GetTransferQuery()) throw new MoneroError("Transfer query's tx query must be circular reference or null");
                    query = query.Clone();
                    query.SetTxQuery(txQuery);
                }
            }
        }
        if (query.GetTxQuery() == null) query.SetTxQuery(new MoneroTxQuery());
        query.GetTxQuery().SetTransferQuery(query);
        if (query.GetTxQuery().GetBlock() == null) query.GetTxQuery().SetBlock(new MoneroBlock().SetTxs(query.GetTxQuery()));
        return query;
    }
}

internal class MoneroWalletConnectionManagerListener : MoneroConnectionManagerListener
{
    private readonly MoneroWalletDefault wallet;
    public MoneroWalletConnectionManagerListener(MoneroWalletDefault wallet)
    {
        this.wallet = wallet;
    }

    public void OnConnectionChanged(MoneroRpcConnection? connection)
    {
        wallet.SetDaemonConnection(connection);
    }
}