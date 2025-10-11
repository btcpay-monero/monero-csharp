using Monero.Common;
using Monero.Wallet.Common;

using Xunit;

using static Monero.Common.MoneroUtils;

using static Xunit.Assert;

namespace Monero.UnitTests;

public class MoneroUtilsUnitTest
{
    // Can validate keys
    [Fact]
    public void TestKeyValidation()
    {
        // test private view key validation
        True(IsValidPrivateViewKey("86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d"));
        TestInvalidPrivateViewKey("");
        TestInvalidPrivateViewKey(null);
        TestInvalidPrivateViewKey(
            "5B8s3obCY2ETeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRVQkunGS");

        // test public view key validation
        True(IsValidPublicViewKey("99873d76ca874ff1aad676b835dd303abcb21c9911ca8a3d9130abc4544d8a0a"));
        TestInvalidPublicViewKey("");
        TestInvalidPublicViewKey(null);
        TestInvalidPublicViewKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");

        // test private spend key validation
        True(IsValidPrivateSpendKey("e9ba887e93620ef9fafdfe0c6d3022949f1c5713cbd9ef631f18a0fb00421dee"));
        TestInvalidPrivateSpendKey("");
        TestInvalidPrivateSpendKey(null);
        TestInvalidPrivateSpendKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");

        // test public spend key validation
        True(IsValidPublicSpendKey("3e48df9e9d8038dbf6f5382fac2becd8686273cda5bd87187e45dca7ec5af37b"));
        TestInvalidPublicSpendKey("");
        TestInvalidPublicSpendKey(null);
        TestInvalidPublicSpendKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");
    }

    // Can convert between XMR and atomic units
    [Fact]
    public void TestAtomicUnitConversion()
    {
        True(1000000000000 == XmrToAtomicUnits(1));
        True(1 == AtomicUnitsToXmr(1000000000000));
        True(1000000000 == XmrToAtomicUnits(0.001));
        True(.001 == AtomicUnitsToXmr(1000000000));
        True(250000000000 == XmrToAtomicUnits(.25));
        True(.25 == AtomicUnitsToXmr(250000000000));
        True(1250000000000 == XmrToAtomicUnits(1.25));
        True(1.25 == AtomicUnitsToXmr(1250000000000));
        True(2796726190000 == XmrToAtomicUnits(2.79672619));
        True(2.79672619 == AtomicUnitsToXmr(2796726190000));
        True(2796726190001 == XmrToAtomicUnits(2.796726190001));
        True(2.796726190001 == AtomicUnitsToXmr(2796726190001));
        True(2796726189999 == XmrToAtomicUnits(2.796726189999));
        True(2.796726189999 == AtomicUnitsToXmr(2796726189999));
        True(2796726180000 == XmrToAtomicUnits(2.79672618));
        True(2.79672618 == AtomicUnitsToXmr(2796726180000));
    }

    [Fact(Skip = "GetPaymentUri is not implemented yet")]
    public void TestGetPaymentUri()
    {
        MoneroTxConfig config = new MoneroTxConfig()
            .SetAddress(
                "42U9v3qs5CjZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKS3rvM3L")
            .SetAmount(XmrToAtomicUnits(0.25))
            .SetRecipientName("John Doe")
            .SetNote("My transfer to wallet");
        string paymentUri = "";
        True(
            "monero:42U9v3qs5CjZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKS3rvM3L?tx_amount=0.25&recipient_name=John%20Doe&tx_description=My%20transfer%20to%20wallet" ==
            paymentUri);
    }

    private static void TestInvalidPrivateViewKey(string? privateViewKey)
    {
        False(IsValidPrivateViewKey(privateViewKey));
        try
        {
            ValidatePrivateViewKey(privateViewKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            True(e.Message.Length > 0);
        }
    }

    private static void TestInvalidPublicViewKey(string? publicViewKey)
    {
        False(IsValidPublicViewKey(publicViewKey));
        try
        {
            ValidatePublicViewKey(publicViewKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            True(e.Message.Length > 0);
        }
    }

    private static void TestInvalidPrivateSpendKey(string? privateSpendKey)
    {
        try
        {
            False(IsValidPrivateSpendKey(privateSpendKey));
            ValidatePrivateSpendKey(privateSpendKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            True(e.Message.Length > 0);
        }
    }

    private static void TestInvalidPublicSpendKey(string? publicSpendKey)
    {
        False(IsValidPublicSpendKey(publicSpendKey));
        try
        {
            ValidatePublicSpendKey(publicSpendKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            True(e.Message.Length > 0);
        }
    }
}