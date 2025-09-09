
namespace Monero.Wallet.Common;

internal class MoneroIncomingTransferComparer : Comparer<MoneroIncomingTransfer>
{
    private static readonly MoneroTxHeightComparer TX_HEIGHT_COMPARATOR = new MoneroTxHeightComparer();

    public override int Compare(MoneroIncomingTransfer? t1, MoneroIncomingTransfer? t2)
    {
        // compare by height
        int heightComparison = TX_HEIGHT_COMPARATOR.Compare(t1.GetTx(), t2.GetTx());
        if (heightComparison != 0) return heightComparison;

        // compare by account and subaddress index
        if (t1.GetAccountIndex() < t2.GetAccountIndex()) return -1;
        else if (t1.GetAccountIndex() == t2.GetAccountIndex()) return t1.GetSubaddressIndex().CompareTo(t2.GetSubaddressIndex());
        return 1;
    }
}