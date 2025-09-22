namespace Monero.Wallet.Common;

public class MoneroIncomingTransfer : MoneroTransfer
{
    private string? _address;
    private ulong? _numSuggestedConfirmations;
    private uint? _subaddressIndex;

    public MoneroIncomingTransfer()
    {
        // nothing to initialize
    }

    public MoneroIncomingTransfer(MoneroIncomingTransfer transfer) : base(transfer)
    {
        _subaddressIndex = transfer._subaddressIndex;
        _address = transfer._address;
        _numSuggestedConfirmations = transfer._numSuggestedConfirmations;
    }

    public override MoneroIncomingTransfer Clone()
    {
        return new MoneroIncomingTransfer(this);
    }

    public override MoneroIncomingTransfer SetTx(MoneroTxWallet? tx)
    {
        _tx = tx;
        return this;
    }

    public override bool? IsIncoming()
    {
        return true;
    }

    public uint? GetSubaddressIndex()
    {
        return _subaddressIndex;
    }

    public MoneroIncomingTransfer SetSubaddressIndex(uint? subaddressIndex)
    {
        _subaddressIndex = subaddressIndex;
        return this;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroIncomingTransfer SetAddress(string? address)
    {
        _address = address;
        return this;
    }

    public ulong? GetNumSuggestedConfirmations()
    {
        return _numSuggestedConfirmations;
    }

    public MoneroIncomingTransfer SetNumSuggestedConfirmations(ulong? numSuggestedConfirmations)
    {
        _numSuggestedConfirmations = numSuggestedConfirmations;
        return this;
    }
}