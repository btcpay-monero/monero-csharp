using Monero.Common;

namespace Monero.Wallet.Common;

public abstract class MoneroTransfer
{
    protected uint? _accountIndex;
    protected ulong? _amount;
    protected MoneroTxWallet? _tx;

    protected MoneroTransfer() { }

    protected MoneroTransfer(MoneroTransfer transfer)
    {
        _tx = transfer._tx;
        _amount = transfer._amount;
        _accountIndex = transfer._accountIndex;
    }

    public abstract MoneroTransfer Clone();

    public MoneroTxWallet? GetTx()
    {
        return _tx;
    }

    public virtual MoneroTransfer SetTx(MoneroTxWallet tx)
    {
        _tx = tx;
        return this;
    }

    public abstract bool? IsIncoming();

    public virtual bool? IsOutgoing()
    {
        return !IsIncoming();
    }

    public ulong? GetAmount()
    {
        return _amount;
    }

    public virtual MoneroTransfer SetAmount(ulong? amount)
    {
        _amount = amount;
        return this;
    }

    public uint? GetAccountIndex()
    {
        return _accountIndex;
    }

    public virtual MoneroTransfer SetAccountIndex(uint? accountIndex)
    {
        _accountIndex = accountIndex;
        return this;
    }

    public MoneroTransfer Merge(MoneroTransfer transfer)
    {
        if (this == transfer)
        {
            return this;
        }

        // merge txs if they're different which comes back to merging transfers
        if (GetTx() != transfer.GetTx())
        {
            GetTx().Merge(transfer.GetTx());
            return this;
        }

        // otherwise merge transfer fields
        SetAccountIndex(GenUtils.Reconcile(GetAccountIndex(), transfer.GetAccountIndex()));

        // TODO monero-project: failed tx in pool (after testUpdateLockedDifferentAccounts()) causes non-originating saved wallets to return duplicate incoming transfers but one has amount of 0
        if ((GetAmount() != null && transfer.GetAmount() != null &&
             !GetAmount().Equals(transfer.GetAmount()) && 0 == GetAmount()) || 0 == transfer.GetAmount())
        {
            MoneroUtils.Log(0, "WARNING: monero-project returning transfers with 0 amount/numSuggestedConfirmations");
        }
        else
        {
            SetAmount(GenUtils.Reconcile(GetAmount(), transfer.GetAmount()));
        }

        return this;
    }
}