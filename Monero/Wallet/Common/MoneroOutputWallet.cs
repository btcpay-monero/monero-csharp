using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroOutputWallet : MoneroOutput
{
    private uint? accountIndex;
    private uint? subaddressIndex;
    private bool isSpent;
    private bool isFrozen;

    public MoneroOutputWallet()
    {
    }

    public MoneroOutputWallet(MoneroOutputWallet output) : base(output)
    {
        this.accountIndex = output.accountIndex;
        this.subaddressIndex = output.subaddressIndex;
        this.isSpent = output.isSpent;
        this.isFrozen = output.isFrozen;
    }

    public override MoneroOutputWallet Clone()
    {
        return new MoneroOutputWallet(this);
    }

    public override MoneroTxWallet GetTx()
    {
        return (MoneroTxWallet)base.GetTx();
    }


    public override MoneroOutputWallet SetTx(MoneroTx tx)
    {
        //if (tx != null && !(tx instanceof MoneroTxWallet)) throw new MoneroError("Wallet output's transaction must be of type MoneroTxWallet");
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
        return accountIndex;
    }

    public MoneroOutputWallet SetAccountIndex(uint? accountIndex)
    {
        this.accountIndex = accountIndex;
        return this;
    }

    public uint? GetSubaddressIndex()
    {
        return subaddressIndex;
    }

    public MoneroOutputWallet SetSubaddressIndex(uint? subaddressIndex)
    {
        this.subaddressIndex = subaddressIndex;
        return this;
    }

    public override MoneroOutputWallet SetAmount(ulong? amount)
    {
        base.SetAmount(amount);
        return this;
    }

    public bool IsSpent()
    {
        return isSpent;
    }

    public MoneroOutputWallet SetIsSpent(bool isSpent)
    {
        this.isSpent = isSpent;
        return this;
    }

    public bool IsFrozen()
    {
        return isFrozen;
    }

    public MoneroOutputWallet SetIsFrozen(bool isFrozen)
    {
        this.isFrozen = isFrozen;
        return this;
    }


    public bool? IsLocked()
    {
        if (GetTx() == null) return null;
        return GetTx().IsLocked();
    }
}