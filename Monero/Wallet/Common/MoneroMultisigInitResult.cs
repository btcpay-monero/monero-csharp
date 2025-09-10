namespace Monero.Wallet.Common;

public class MoneroMultisigInitResult
{
    private string? _address;
    private string? _multisigHex;

    public MoneroMultisigInitResult(string? address = null, string? multisigHex = null)
    {
        _address = address;
        _multisigHex = multisigHex;
    }

    public MoneroMultisigInitResult(MoneroMultisigInitResult initResult)
    {
        _address = initResult._address;
        _multisigHex = initResult._multisigHex;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroMultisigInitResult SetAddress(string? address)
    {
        _address = address;
        return this;
    }

    public string? GetMultisigHex()
    {
        return _multisigHex;
    }

    public MoneroMultisigInitResult SetMultisigHex(string? multisigHex)
    {
        _multisigHex = multisigHex;
        return this;
    }

    public MoneroMultisigInitResult Clone()
    {
        return new MoneroMultisigInitResult(this);
    }
}