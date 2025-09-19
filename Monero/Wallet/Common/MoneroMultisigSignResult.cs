namespace Monero.Wallet.Common;

public class MoneroMultisigSignResult
{
    private string? _signedMultisigHex;
    private List<string>? _txHashes;

    public MoneroMultisigSignResult() { }

    public MoneroMultisigSignResult(string signedMultisigHex, List<string> txHashes)
    {
        _signedMultisigHex = signedMultisigHex;
        _txHashes = txHashes;
    }

    public MoneroMultisigSignResult(MoneroMultisigSignResult signResult)
    {
        _signedMultisigHex = signResult._signedMultisigHex;
        if (signResult._txHashes != null)
        {
            _txHashes = [.. signResult._txHashes];
        }
    }

    public string? GetSignedMultisigTxHex()
    {
        return _signedMultisigHex;
    }

    public MoneroMultisigSignResult SetSignedMultisigTxHex(string? signedMultisigHex)
    {
        _signedMultisigHex = signedMultisigHex;
        return this;
    }

    public List<string>? GetTxHashes()
    {
        return _txHashes;
    }

    public MoneroMultisigSignResult SetTxHashes(List<string>? txHashes)
    {
        if (txHashes == null)
        {
            _txHashes = null;
        }
        else
        {
            _txHashes = [.. txHashes];
        }

        return this;
    }

    public MoneroMultisigSignResult Clone()
    {
        return new MoneroMultisigSignResult(this);
    }
}