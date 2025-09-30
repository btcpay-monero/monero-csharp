using Monero.Common;

namespace Monero.Wallet.Common;

internal class MoneroTxHeightComparer : Comparer<MoneroTx>
{
    public override int Compare(MoneroTx? tx1, MoneroTx? tx2)
    {
        ulong? height1 = tx1?.GetHeight();
        ulong? height2 = tx2?.GetHeight();
        if ((height1 == null && height2 == null) || (tx1 == null && tx2 == null))
        {
            return 0; // both unconfirmed
        }

        if (height1 == null || tx1 == null)
        {
            return 1; // tx1 is unconfirmed
        }

        if (height2 == null || tx2 == null)
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

        return CompareBlocks(tx1.GetBlock(), tx2.GetBlock(), tx1, tx2);
    }

    private static int CompareBlocks(MoneroBlock? block1, MoneroBlock? block2, MoneroTx? tx1, MoneroTx? tx2)
    {
        if (tx1 == null)
        {
            throw new MoneroError("Cannot compare blocks, tx1 is null");
        }

        if (tx2 == null)
        {
            throw new MoneroError("Cannot compare blocks, tx2 is null");
        }

        if (block1 == null && block2 == null)
        {
            return 0;
        }

        if (block1 == null)
        {
            return 1;
        }

        if (block2 == null)
        {
            return -1;
        }

        List<MoneroTx> txs1 = block1.Txs ?? [];
        List<MoneroTx> txs2 = block2.Txs ?? [];

        return txs1.IndexOf(tx1) -
               txs2.IndexOf(tx2); // txs are in the same block so retain their original order
    }
}