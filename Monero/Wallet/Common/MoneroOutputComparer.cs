using Monero.Common;

namespace Monero.Wallet.Common;

internal class MoneroOutputComparer : Comparer<MoneroOutput>
{
    private static readonly MoneroTxHeightComparer TX_HEIGHT_COMPARATOR = new MoneroTxHeightComparer();

    public override int Compare(MoneroOutput? o1, MoneroOutput? o2)
    {
        MoneroOutputWallet ow1 = (MoneroOutputWallet)o1;
        MoneroOutputWallet ow2 = (MoneroOutputWallet)o2;

        if (ow1 == null && ow2 == null) return 0;
        if (ow1 == null) return -1; // nulls are less than non-nulls
        if (ow2 == null) return 1; // non-nulls are greater than nulls

        // compare by height
        int heightComparison = TX_HEIGHT_COMPARATOR.Compare(ow1.GetTx(), ow2.GetTx());
        if (heightComparison != 0) return heightComparison;

        uint? ow1AccountIdx = ow1.GetAccountIndex();
        uint? ow2AccountIdx = ow2.GetAccountIndex();

        if (ow1AccountIdx == null && ow2AccountIdx == null) return 0; // both outputs have no account index
        if (ow1AccountIdx == null) return -1; // output 1 has no account index
        if (ow2AccountIdx == null) return 1; // output 2 has no account index

        // compare by account index, subaddress index, output index, then key image hex
        int compare = ((uint)ow1AccountIdx).CompareTo(ow2AccountIdx);
        if (compare != 0) return compare;

        uint? ow1SubaddressIdx = ow1.GetSubaddressIndex();
        uint? ow2SubaddressIdx = ow2.GetSubaddressIndex();

        if (ow1SubaddressIdx == null && ow2SubaddressIdx == null) return 0; // both outputs have no subaddress index
        if (ow1SubaddressIdx == null) return -1; // output 1 has no subaddress index
        if (ow2SubaddressIdx == null) return 1; // output 2 has no subaddress index

        compare = ((uint)ow1SubaddressIdx).CompareTo(ow2SubaddressIdx);
        if (compare != 0) return compare;

        ulong? ow1Index = ow1.GetIndex();
        ulong? ow2Index = ow2.GetIndex();

        if (ow1Index == null && ow2Index == null) return 0; // both outputs have no index
        if (ow1Index == null) return -1; // output 1 has no index
        if (ow2Index == null) return 1; // output 2 has no index

        compare = ((ulong)ow1Index).CompareTo(ow2Index);
        if (compare != 0) return compare;

        // finally compare by key image hex
        MoneroKeyImage ow1KeyImage = ow1.GetKeyImage();
        MoneroKeyImage ow2KeyImage = ow2.GetKeyImage();

        if (ow1KeyImage == null && ow2KeyImage == null) return 0; // both outputs have no key image

        if (ow1KeyImage == null) return -1; // output 1 has no key image
        if (ow2KeyImage == null) return 1; // output 2 has no key image

        string? ow1KeyImageHex = ow1KeyImage.GetHex();
        string? ow2KeyImageHex = ow2KeyImage.GetHex();

        if (ow1KeyImageHex == null && ow2KeyImageHex == null) return 0; // both outputs have no key image hex
        if (ow1KeyImageHex == null) return -1; // output 1 has no key image hex
        if (ow2KeyImageHex == null) return 1; // output 2 has no key image hex
        return ow1KeyImageHex.CompareTo(ow2KeyImageHex);
    }
}