using Monero.Common;
using Monero.Wallet.Common;

using NUnit.Framework;

using static Monero.Common.MoneroUtils;

namespace Monero.UnitTests;

public class MoneroUtilsUnitTest
{
    // Can validate keys
    [Test]
    public void TestKeyValidation()
    {
        // test private view key validation
        Assert.That(IsValidPrivateViewKey("86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d"), Is.True);
        TestInvalidPrivateViewKey("");
        TestInvalidPrivateViewKey(null);
        TestInvalidPrivateViewKey(
            "5B8s3obCY2ETeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRVQkunGS");

        // test public view key validation
        Assert.That(IsValidPublicViewKey("99873d76ca874ff1aad676b835dd303abcb21c9911ca8a3d9130abc4544d8a0a"), Is.True);
        TestInvalidPublicViewKey("");
        TestInvalidPublicViewKey(null);
        TestInvalidPublicViewKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");

        // test private spend key validation
        Assert.That(IsValidPrivateSpendKey("e9ba887e93620ef9fafdfe0c6d3022949f1c5713cbd9ef631f18a0fb00421dee"), Is.True);
        TestInvalidPrivateSpendKey("");
        TestInvalidPrivateSpendKey(null);
        TestInvalidPrivateSpendKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");

        // test public spend key validation
        Assert.That(IsValidPublicSpendKey("3e48df9e9d8038dbf6f5382fac2becd8686273cda5bd87187e45dca7ec5af37b"), Is.True);
        TestInvalidPublicSpendKey("");
        TestInvalidPublicSpendKey(null);
        TestInvalidPublicSpendKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");
    }

    // Can convert between XMR and atomic units
    [Test]
    public void TestAtomicUnitConversion()
    {
        Assert.That(XmrToAtomicUnits(1), Is.EqualTo(1000000000000));
        Assert.That(AtomicUnitsToXmr(1000000000000), Is.EqualTo(1));
        Assert.That(XmrToAtomicUnits(0.001), Is.EqualTo(1000000000));
        Assert.That(AtomicUnitsToXmr(1000000000), Is.EqualTo(0.001));
        Assert.That(XmrToAtomicUnits(0.25), Is.EqualTo(250000000000));
        Assert.That(AtomicUnitsToXmr(250000000000), Is.EqualTo(0.25));
        Assert.That(XmrToAtomicUnits(1.25), Is.EqualTo(1250000000000));
        Assert.That(AtomicUnitsToXmr(1250000000000), Is.EqualTo(1.25));
        Assert.That(XmrToAtomicUnits(2.79672619), Is.EqualTo(2796726190000));
        Assert.That(AtomicUnitsToXmr(2796726190000), Is.EqualTo(2.79672619));
        Assert.That(XmrToAtomicUnits(2.796726190001), Is.EqualTo(2796726190001));
        Assert.That(AtomicUnitsToXmr(2796726190001), Is.EqualTo(2.796726190001));
        Assert.That(XmrToAtomicUnits(2.796726189999), Is.EqualTo(2796726189999));
        Assert.That(AtomicUnitsToXmr(2796726189999), Is.EqualTo(2.796726189999));
        Assert.That(XmrToAtomicUnits(2.79672618), Is.EqualTo(2796726180000));
        Assert.That(AtomicUnitsToXmr(2796726180000), Is.EqualTo(2.79672618));
    }

    [Test]
    [Ignore("GetPaymentUri is not implemented yet")]
    public void TestGetPaymentUri()
    {
        MoneroTxConfig unused = new MoneroTxConfig()
            .SetAddress(
                "9xSyMy1r9h3BVjMrF3CTqQCQy36yCfkpn7uVfMyTUbez3hhumqBUqGUNNALjcd7f1HJBRdeH82bCC3veFHW7z3xm28gug4d")
            .SetAmount(XmrToAtomicUnits(0.25))
            .SetRecipientName("John Doe")
            .SetNote("My transfer to wallet");
        const string paymentUri = "";
        Assert.That(paymentUri, Is.EqualTo(
            "monero:9xSyMy1r9h3BVjMrF3CTqQCQy36yCfkpn7uVfMyTUbez3hhumqBUqGUNNALjcd7f1HJBRdeH82bCC3veFHW7z3xm28gug4d?tx_amount=0.25&recipient_name=John%20Doe&tx_description=My%20transfer%20to%20wallet"));
    }

    private static void TestInvalidPrivateViewKey(string? privateViewKey)
    {
        Assert.That(IsValidPrivateViewKey(privateViewKey), Is.False);
        try
        {
            ValidatePrivateViewKey(privateViewKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0, Is.True);
        }
    }

    private static void TestInvalidPublicViewKey(string? publicViewKey)
    {
        Assert.That(IsValidPublicViewKey(publicViewKey), Is.False);
        try
        {
            ValidatePublicViewKey(publicViewKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0, Is.True);
        }
    }

    private static void TestInvalidPrivateSpendKey(string? privateSpendKey)
    {
        try
        {
            Assert.That(IsValidPublicSpendKey(privateSpendKey), Is.False);
            ValidatePrivateSpendKey(privateSpendKey);
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0, Is.True);
        }
    }

    private static void TestInvalidPublicSpendKey(string? publicSpendKey)
    {
        Assert.That(IsValidPublicSpendKey(publicSpendKey), Is.False);
        try
        {
            ValidatePublicSpendKey(publicSpendKey);
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0, Is.True);
        }
    }
}