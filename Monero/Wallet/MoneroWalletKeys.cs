using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Wallet;

public class MoneroWalletKeys : MoneroWalletDefault
{
    public static MoneroWalletKeys CreateWalletFromKeys(MoneroWalletConfig config)
    {
        throw new NotImplementedException("MoneroWalletKeys.CreateWalletFromKeys(): not implemented");
    }

    public static MoneroWalletKeys CreateWalletFromSeed(MoneroWalletConfig config)
    {
        throw new NotImplementedException("MoneroWalletKeys.CreateWalletFromSeed(): not implemented");
    }

    public static MoneroWalletKeys CreateWalletRandom(MoneroWalletConfig config)
    {
        throw new NotImplementedException("MoneroWalletKeys.CreateWalletRandom(): not implemented");
    }

    #region Override Base Methods

    public override Task<int> AddAddressBookEntry(string address, string description)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task ChangePassword(string oldPassword, string newPassword)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroCheckReserve> CheckReserveProof(string address, string message, string signature)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<bool> CheckSpendProof(string txHash, string message, string signature)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroCheckTx> CheckTxKey(string txHash, string txKey, string address)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroCheckTx> CheckTxProof(string txHash, string address, string message, string signature)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroAccount> CreateAccount(string? label)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroSubaddress> CreateSubaddress(uint accountIdx, string? label)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroTxWallet>> CreateTxs(MoneroTxConfig config)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroIntegratedAddress> DecodeIntegratedAddress(string integratedAddress)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task DeleteAddressBookEntry(uint entryIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroTxSet> DescribeTxSet(MoneroTxSet txSet)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription,
        string description)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroMultisigInitResult> ExchangeMultisigKeys(List<string> multisigHexes, string password)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroKeyImage>> ExportKeyImages(bool all)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> ExportMultisigHex()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> ExportOutputs(bool all)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task FreezeOutput(string keyImage)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroAccount> GetAccount(uint accountIdx, bool includeSubaddresses)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroAccount>> GetAccounts(bool includeSubaddresses, string? tag)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroAccountTag>> GetAccountTags()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetAddress(uint accountIdx, uint subaddressIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroAddressBookEntry>> GetAddressBookEntries(List<uint>? entryIndices)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroSubaddress> GetAddressIndex(string address)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string?> GetAttribute(string key)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<ulong> GetBalance(uint? accountIdx, uint? subaddressIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroRpcConnection?> GetDaemonConnection()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<ulong> GetDaemonHeight()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroTxPriority> GetDefaultFeePriority()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<ulong> GetHeight()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<ulong> GetHeightByDate(int year, int month, int day)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroIntegratedAddress> GetIntegratedAddress(string? standardAddress, string? paymentId)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroMultisigInfo> GetMultisigInfo()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroKeyImage>> GetNewKeyImagesFromLastImport()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroOutputWallet>> GetOutputs(MoneroOutputQuery query)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetPath()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetPaymentUri(MoneroTxConfig config)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetPrivateSpendKey()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetPrivateViewKey()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetPublicSpendKey()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetPublicViewKey()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetReserveProofAccount(uint accountIdx, ulong amount, string message)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetReserveProofWallet(string message)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetSeed()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetSeedLanguage()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetSpendProof(string txHash, string? message)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroSubaddress>> GetSubaddresses(uint accountIdx, List<uint>? subaddressIndices)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroTransfer>> GetTransfers(MoneroTransferQuery query)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetTxKey(string txHash)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<string>> GetTxNotes(List<string> txHashes)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> GetTxProof(string txHash, string address, string? message)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroTxWallet>> GetTxs(MoneroTxQuery? query)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<ulong> GetUnlockedBalance(uint? accountIdx, uint? subaddressIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroVersion> GetVersion()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroNetworkType> GetNetworkType()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroWalletType GetWalletType()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroKeyImageImportResult> ImportKeyImages(List<MoneroKeyImage> keyImages)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<int> ImportMultisigHex(List<string> multisigHexes)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<int> ImportOutputs(string outputsHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<bool> IsConnectedToDaemon()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<bool> IsMultisigImportNeeded()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<bool> IsOutputFrozen(string keyImage)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<bool> IsViewOnly()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> MakeMultisig(List<string> multisigHexes, int threshold, string password)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroTxConfig> ParsePaymentUri(string uri)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> PrepareMultisig()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<string>> RelayTxs(List<string> txMetadatas)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task RescanBlockchain()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task RescanSpent()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task Save()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task ScanTxs(List<string> txHashes)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task SetAccountTagLabel(string tag, string label)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task SetAttribute(string key, string val)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task SetDaemonConnection(MoneroRpcConnection? daemonConnection)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task SetProxyUri(string? uri)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task SetSubaddressLabel(uint accountIdx, uint subaddressIdx, string? label)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task SetTxNotes(List<string> txHashes, List<string> notes)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<string> SignMessage(string message, MoneroMessageSignatureType signatureType,
        uint accountIdx, uint subaddressIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroMultisigSignResult> SignMultisigTxHex(string multisigTxHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroTxSet> SignTxs(string unsignedTxHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task StartSyncing(ulong? syncPeriodInMs)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task StopMining()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task StopSyncing()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<string>> SubmitMultisigTxHex(string signedMultisigTxHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<string>> SubmitTxs(string signedTxHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroTxWallet>> SweepDust(bool relay)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroTxWallet> SweepOutput(MoneroTxConfig config)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<List<MoneroTxWallet>> SweepUnlocked(MoneroTxConfig config)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroSyncResult> Sync(ulong? startHeight, MoneroWalletListener? listener)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task TagAccounts(string tag, List<uint> accountIndices)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task ThawOutput(string keyImage)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task UntagAccounts(List<uint> accountIndices)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task<MoneroMessageSignatureResult> VerifyMessage(string message, string address, string signature)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override Task Close(bool save)
    {
        throw new NotImplementedException();
    }

    #endregion
}