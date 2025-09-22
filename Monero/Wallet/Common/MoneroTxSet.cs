namespace Monero.Wallet.Common;

public class MoneroTxSet
{
    private string? _multisigTxHex;
    private string? _signedTxHex;
    private List<MoneroTxWallet>? _txs;
    private string? _unsignedTxHex;

    public List<MoneroTxWallet>? GetTxs()
    {
        return _txs;
    }

    public MoneroTxSet SetTxs(List<MoneroTxWallet>? txs)
    {
        _txs = txs;
        return this;
    }

    public string? GetMultisigTxHex()
    {
        return _multisigTxHex;
    }

    public MoneroTxSet SetMultisigTxHex(string? multisigTxHex)
    {
        _multisigTxHex = multisigTxHex;
        return this;
    }

    public string? GetUnsignedTxHex()
    {
        return _unsignedTxHex;
    }

    public MoneroTxSet SetUnsignedTxHex(string? unsignedTxHex)
    {
        _unsignedTxHex = unsignedTxHex;
        return this;
    }

    public string? GetSignedTxHex()
    {
        return _signedTxHex;
    }

    public MoneroTxSet SetSignedTxHex(string? signedTxHex)
    {
        _signedTxHex = signedTxHex;
        return this;
    }
}