using Monero.Common;
using Monero.Wallet.Common;
using Monero.Wallet.Rpc;

namespace Monero.Wallet;

public interface IMoneroWallet
{
    public static string DefaultLanguage => "English";

    void AddListener(MoneroWalletListener listener);

    void RemoveListener(MoneroWalletListener listener);

    List<MoneroWalletListener> GetListeners();

    Task<bool> IsViewOnly();

    Task SetDaemonConnection(MoneroRpcConnection? connection, bool? isTrusted, SslOptions? sslOptions);

    Task<MoneroRpcConnection?> GetDaemonConnection();

    Task SetProxyUri(string? uri);

    Task<bool> IsConnectedToDaemon();

    Task<MoneroVersion> GetVersion();

    Task<MoneroNetworkType> GetNetworkType();

    Task<string> GetPath();

    Task<string> GetSeed();

    Task<string> GetSeedLanguage();

    Task<string> GetPrivateViewKey();

    Task<string> GetPrivateSpendKey();

    Task<string> GetPublicViewKey();

    Task<string> GetPublicSpendKey();

    Task<string> GetPrimaryAddress();

    Task<string?> GetAddress(uint accountIdx, uint subaddressIdx);

    Task<MoneroSubaddress> GetAddressIndex(string address);

    Task<MoneroIntegratedAddress> GetIntegratedAddress(string? standardAddress, string? paymentId);

    Task<MoneroIntegratedAddress> DecodeIntegratedAddress(string integratedAddress);

    Task<ulong> GetHeight();

    Task<ulong> GetDaemonHeight();

    Task<ulong> GetHeightByDate(int year, int month, int day);

    Task StartSyncing(ulong? syncPeriodInMs);

    Task StopSyncing();

    Task ScanTxs(List<string> txHashes);

    Task RescanSpent();

    Task RescanBlockchain();

    Task<ulong> GetBalance(uint? accountIdx, uint? subaddressIdx);

    Task<List<MoneroAccount>> GetAccounts(bool includeSubaddresses, bool skipBalances, string? tag);

    Task<MoneroAccount> CreateAccount(string? label);

    Task SetAccountLabel(uint accountIdx, uint subaddressIdx, string label);

    Task<List<MoneroSubaddress>> GetSubaddresses(uint accountIdx, bool skipBalances, List<uint>? subaddressIndices);

    Task<MoneroSubaddress> GetSubaddress(uint accountIdx, uint subaddressIdx);

    Task<MoneroSubaddress> CreateSubaddress(uint accountIdx, string? label);

    Task<GetTransfersResult> GetTransfers(MoneroTransferQuery query);

    Task<GetTransferByTxIdResult> GetTransferByTxId(string txId, int? accountIndex);

    Task<string> ExportOutputs(bool all);

    Task<int> ImportOutputs(string outputsHex);

    Task<List<MoneroKeyImage>> ExportKeyImages(bool all);

    Task<MoneroKeyImageImportResult> ImportKeyImages(List<MoneroKeyImage> keyImages);

    Task FreezeOutput(string keyImage);

    Task ThawOutput(string keyImage);

    Task<bool> IsOutputFrozen(string keyImage);

    Task<MoneroTxPriority> GetDefaultFeePriority();

    Task<string> RelayTx(string txMetadata);

    Task<List<string>> SubmitTxs(string signedTxHex);

    Task<string> SignMessage(string message, MoneroMessageSignatureType signatureType, uint accountIdx,
        uint subaddressIdx);

    Task<MoneroMessageSignatureResult> VerifyMessage(string message, string address, string signature);

    Task<string> GetTxKey(string txHash);

    Task<MoneroCheckTx> CheckTxKey(string txHash, string txKey, string address);

    Task<string> GetTxProof(string txHash, string address, string? message);

    Task<MoneroCheckTx> CheckTxProof(string txHash, string address, string message, string signature);

    Task<string> GetSpendProof(string txHash, string? message);

    Task<bool> CheckSpendProof(string txHash, string message, string signature);

    Task<string> GetReserveProofWallet(string message);

    Task<string> GetReserveProofAccount(uint accountIdx, ulong amount, string message);

    Task<MoneroCheckReserve> CheckReserveProof(string address, string message, string signature);

    Task<List<string>> GetTxNotes(List<string> txHashes);

    Task SetTxNotes(List<string> txHashes, List<string> notes);

    Task<List<MoneroAddressBookEntry>> GetAddressBookEntries(List<uint> entryIndices);

    Task<int> AddAddressBookEntry(string address, string description);

    Task EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription,
        string description);

    Task DeleteAddressBookEntry(uint entryIdx);

    Task TagAccounts(string tag, List<uint> accountIndices);

    Task UntagAccounts(List<uint> accountIndices);

    Task<List<MoneroAccountTag>> GetAccountTags();

    Task SetAccountTagLabel(string tag, string label);

    Task<string> GetPaymentUri(MoneroTxConfig config);

    Task<MoneroTxConfig> ParsePaymentUri(string uri);

    Task<string?> GetAttribute(string key);

    Task SetAttribute(string key, string val);

    Task StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery);

    Task StopMining();

    Task<bool> IsMultisigImportNeeded();

    Task<bool> IsMultisig();

    Task<MoneroMultisigInfo> GetMultisigInfo();

    Task<string> PrepareMultisig();

    Task<string> MakeMultisig(List<string> multisigHexes, int threshold, string password);

    Task<MoneroMultisigInitResult> ExchangeMultisigKeys(List<string> multisigHexes, string password);

    Task<string> ExportMultisigHex();

    Task<int> ImportMultisigHex(List<string> multisigHexes);

    Task<MoneroMultisigSignResult> SignMultisigTxHex(string multisigTxHex);

    Task<List<string>> SubmitMultisigTxHex(string signedMultisigTxHex);

    Task ChangePassword(string oldPassword, string newPassword);

    Task Save();

    Task Close(bool save);

    Task<bool> IsClosed();
}