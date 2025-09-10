using Monero.Common;

namespace Monero.Wallet.Common;

internal class MoneroTxHeightComparer : Comparer<MoneroTx>
{
    public override int Compare(MoneroTx? tx1, MoneroTx? tx2)
    {
        ulong? height1 = tx1?.GetHeight();
        ulong? height2 = tx2?.GetHeight();
        if (height1 == null && height2 == null)
        {
            return 0; // both unconfirmed
        }

        if (height1 == null)
        {
            return 1; // tx1 is unconfirmed
        }

        if (height2 == null)
        {
            return -1; // tx2 is unconfirmed
        }

        if (height1 < height2)
        {
            return -1; // tx1 is earlier
        }

        if (height1 > height2)
        {
            return 1; // tx2 is earlier
        }

        return tx1.GetBlock().GetTxs().IndexOf(tx1) -
               tx2.GetBlock().GetTxs().IndexOf(tx2); // txs are in the same block so retain their original order
    }
}