
namespace Monero.Wallet.Common;

public class MoneroIncomingTransfer : MoneroTransfer
{
    private uint subaddressIndex;
    private string address;
    private ulong numSuggestedConfirmations;

    public MoneroIncomingTransfer()
    {
        // nothing to initialize
    }

    public MoneroIncomingTransfer(MoneroIncomingTransfer transfer) : base(transfer)
    {
        subaddressIndex = transfer.subaddressIndex;
        address = transfer.address;
        numSuggestedConfirmations = transfer.numSuggestedConfirmations;
    }

    public override MoneroIncomingTransfer Clone()
    {
        return new MoneroIncomingTransfer(this);
    }

    public override MoneroIncomingTransfer SetTx(MoneroTxWallet tx)
    {
        _tx = tx;
        return this;
    }

    public override bool? IsIncoming()
    {
        return true;
    }

    public uint GetSubaddressIndex()
    {
        return subaddressIndex;
    }

    public MoneroIncomingTransfer SetSubaddressIndex(uint subaddressIndex)
    {
        this.subaddressIndex = subaddressIndex;
        return this;
    }

    public string GetAddress()
    {
        return address;
    }

    public MoneroIncomingTransfer SetAddress(string address)
    {
        this.address = address;
        return this;
    }

    public ulong GetNumSuggestedConfirmations()
    {
        return numSuggestedConfirmations;
    }

    public MoneroIncomingTransfer SetNumSuggestedConfirmations(ulong numSuggestedConfirmations)
    {
        this.numSuggestedConfirmations = numSuggestedConfirmations;
        return this;
    }
}