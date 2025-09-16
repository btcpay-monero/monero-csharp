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

    public override int AddAddressBookEntry(string address, string description)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void ChangePassword(string oldPassword, string newPassword)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroCheckReserve CheckReserveProof(string address, string message, string signature)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override bool CheckSpendProof(string txHash, string message, string signature)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroCheckTx CheckTxKey(string txHash, string txKey, string address)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroCheckTx CheckTxProof(string txHash, string address, string message, string signature)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroAccount CreateAccount(string? label)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroSubaddress CreateSubaddress(uint accountIdx, string? label)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroTxWallet> CreateTxs(MoneroTxConfig config)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroIntegratedAddress DecodeIntegratedAddress(string integratedAddress)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void DeleteAddressBookEntry(uint entryIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroTxSet DescribeTxSet(MoneroTxSet txSet)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription,
        string description)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroMultisigInitResult ExchangeMultisigKeys(List<string> multisigHexes, string password)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroKeyImage> ExportKeyImages(bool all)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string ExportMultisigHex()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string ExportOutputs(bool all)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void FreezeOutput(string keyImage)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroAccount GetAccount(uint accountIdx, bool includeSubaddresses)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroAccount> GetAccounts(bool includeSubaddresses, string? tag)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroAccountTag> GetAccountTags()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetAddress(uint accountIdx, uint subaddressIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroAddressBookEntry> GetAddressBookEntries(List<uint>? entryIndices)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroSubaddress GetAddressIndex(string address)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string? GetAttribute(string key)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override ulong GetBalance(uint? accountIdx, uint? subaddressIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroRpcConnection? GetDaemonConnection()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override ulong GetDaemonHeight()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroTxPriority GetDefaultFeePriority()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override ulong GetHeight()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override ulong GetHeightByDate(int year, int month, int day)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroIntegratedAddress GetIntegratedAddress(string? standardAddress, string? paymentId)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroMultisigInfo GetMultisigInfo()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroKeyImage> GetNewKeyImagesFromLastImport()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroOutputWallet> GetOutputs(MoneroOutputQuery query)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetPath()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetPaymentUri(MoneroTxConfig config)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetPrivateSpendKey()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetPrivateViewKey()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetPublicSpendKey()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetPublicViewKey()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetReserveProofAccount(uint accountIdx, ulong amount, string message)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetReserveProofWallet(string message)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetSeed()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetSeedLanguage()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetSpendProof(string txHash, string? message)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroSubaddress> GetSubaddresses(uint accountIdx, List<uint>? subaddressIndices)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroTransfer> GetTransfers(MoneroTransferQuery query)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetTxKey(string txHash)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<string> GetTxNotes(List<string> txHashes)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string GetTxProof(string txHash, string address, string? message)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroTxWallet> GetTxs(MoneroTxQuery? query)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override ulong GetUnlockedBalance(uint? accountIdx, uint? subaddressIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroVersion GetVersion()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroNetworkType GetNetworkType()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroWalletType GetWalletType()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroKeyImageImportResult ImportKeyImages(List<MoneroKeyImage> keyImages)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override int ImportMultisigHex(List<string> multisigHexes)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override int ImportOutputs(string outputsHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override bool IsConnectedToDaemon()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override bool IsMultisigImportNeeded()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override bool IsOutputFrozen(string keyImage)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override bool IsViewOnly()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string MakeMultisig(List<string> multisigHexes, int threshold, string password)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroTxConfig ParsePaymentUri(string uri)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string PrepareMultisig()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<string> RelayTxs(List<string> txMetadatas)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void RescanBlockchain()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void RescanSpent()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void Save()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void ScanTxs(List<string> txHashes)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void SetAccountTagLabel(string tag, string label)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void SetAttribute(string key, string val)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void SetDaemonConnection(MoneroRpcConnection? daemonConnection)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void SetProxyUri(string? uri)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void SetSubaddressLabel(uint accountIdx, uint subaddressIdx, string? label)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void SetTxNotes(List<string> txHashes, List<string> notes)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override string SignMessage(string message, MoneroMessageSignatureType signatureType,
        uint accountIdx, uint subaddressIdx)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroMultisigSignResult SignMultisigTxHex(string multisigTxHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroTxSet SignTxs(string unsignedTxHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void StartSyncing(ulong? syncPeriodInMs)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void StopMining()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void StopSyncing()
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<string> SubmitMultisigTxHex(string signedMultisigTxHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<string> SubmitTxs(string signedTxHex)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroTxWallet> SweepDust(bool relay)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroTxWallet SweepOutput(MoneroTxConfig config)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override List<MoneroTxWallet> SweepUnlocked(MoneroTxConfig config)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroSyncResult Sync(ulong? startHeight, MoneroWalletListener? listener)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void TagAccounts(string tag, List<uint> accountIndices)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void ThawOutput(string keyImage)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override void UntagAccounts(List<uint> accountIndices)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    public override MoneroMessageSignatureResult VerifyMessage(string message, string address, string signature)
    {
        throw new NotImplementedException("Not supported by keys only wallet");
    }

    #endregion
}