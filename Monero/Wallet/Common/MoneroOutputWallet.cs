using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroOutputWallet : MoneroOutput
{
    private uint? _accountIndex;
    private bool? _isFrozen;
    private bool? _isSpent;
    private uint? _subaddressIndex;

    public MoneroOutputWallet()
    {
    }

    public MoneroOutputWallet(MoneroOutputWallet output) : base(output)
    {
        _accountIndex = output._accountIndex;
        _subaddressIndex = output._subaddressIndex;
        _isSpent = output._isSpent;
        _isFrozen = output._isFrozen;
    }

    public override MoneroOutputWallet Clone()
    {
        return new MoneroOutputWallet(this);
    }

    public override MoneroTxWallet? GetTx()
    {
        return base.GetTx() as MoneroTxWallet;
    }


    public override MoneroOutputWallet SetTx(MoneroTx? tx)
    {
        base.SetTx(tx);
        return this;
    }

    public MoneroOutputWallet SetTx(MoneroTxWallet tx)
    {
        base.SetTx(tx);
        return this;
    }

    public uint? GetAccountIndex()
    {
        return _accountIndex;
    }

    public MoneroOutputWallet SetAccountIndex(uint? accountIndex)
    {
        _accountIndex = accountIndex;
        return this;
    }

    public uint? GetSubaddressIndex()
    {
        return _subaddressIndex;
    }

    public MoneroOutputWallet SetSubaddressIndex(uint? subaddressIndex)
    {
        _subaddressIndex = subaddressIndex;
        return this;
    }

    public override MoneroOutputWallet SetAmount(ulong? amount)
    {
        base.SetAmount(amount);
        return this;
    }

    public bool? IsSpent()
    {
        return _isSpent;
    }

    public MoneroOutputWallet SetIsSpent(bool? isSpent)
    {
        _isSpent = isSpent;
        return this;
    }

    public bool? IsFrozen()
    {
        return _isFrozen;
    }

    public MoneroOutputWallet SetIsFrozen(bool? isFrozen)
    {
        _isFrozen = isFrozen;
        return this;
    }


    public bool? IsLocked()
    {
        MoneroTxWallet? tx = GetTx();
        if (tx == null)
        {
            return null;
        }

        return tx.IsLocked();
    }
}