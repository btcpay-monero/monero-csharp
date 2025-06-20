﻿using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Test;

public class TestMoneroUtils
{
    [SetUp]
    public void Setup()
    {
    }

    // Can get integrated addresses
    [Test]
    public void TestGetIntegratedAddresses()
    {
        string primaryAddress = "58qRVVjZ4KxMX57TH6yWqGcH5AswvZZS494hWHcHPt6cDkP7V8AqxFhi3RKXZueVRgUnk8niQGHSpY5Bm9DjuWn16GDKXpF";
        string subaddress = "7B9w2xieXjhDumgPX39h1CAYELpsZ7Pe8Wqtr3pVL9jJ5gGDqgxjWt55gTYUCAuhahhM85ajEp6VbQfLDPETt4oT2ZRXa6n";
        string paymentId = "03284e41c342f036";
        MoneroNetworkType networkType = MoneroNetworkType.STAGENET;

        // get integrated address with randomly generated payment id
        MoneroIntegratedAddress integratedAddress = MoneroUtils.GetIntegratedAddress(networkType, primaryAddress, null);
        Assert.That(primaryAddress == integratedAddress.GetStandardAddress());
        Assert.That(16 == integratedAddress.GetPaymentId().Length);
        Assert.That(106 == integratedAddress.GetIntegratedAddress().Length);

        // get integrated address with specific payment id
        integratedAddress = MoneroUtils.GetIntegratedAddress(networkType, primaryAddress, paymentId);
        Assert.That(primaryAddress == integratedAddress.GetStandardAddress());
        Assert.That(paymentId == integratedAddress.GetPaymentId());
        Assert.That(106 == integratedAddress.GetIntegratedAddress().Length);

        // get integrated address with subaddress
        integratedAddress = MoneroUtils.GetIntegratedAddress(networkType, subaddress, paymentId);
        Assert.That(subaddress == integratedAddress.GetStandardAddress());
        Assert.That(paymentId == integratedAddress.GetPaymentId());
        Assert.That(106 == integratedAddress.GetIntegratedAddress().Length);

        // get integrated address with invalid payment id
        try
        {
            MoneroUtils.GetIntegratedAddress(networkType, primaryAddress, "123");
            throw new Exception("Getting integrated address with invalid payment id should have failed");
        }
        catch (MoneroError err)
        {
            Assert.That("Invalid payment id" == err.Message);
        }
    }

    // Can validate addresses
    [Test]
    public void TestAddressValidation()
    {

        // test mainnet primary address validation
        Assert.That(MoneroUtils.IsValidAddress("42U9v3qs5CjZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKS3rvM3L", MoneroNetworkType.MAINNET));
        Assert.That(MoneroUtils.IsValidAddress("48ZxX3Y2y5s4nJ8fdz2w65TrTEp9PRsv5J8iHSShkHQcE2V31FhnWptioNst1K9oeDY4KpWZ7v8V2BZNVa4Wdky89iqmPz2", MoneroNetworkType.MAINNET));
        Assert.That(MoneroUtils.IsValidAddress("48W972Fx1SQMCHVKENnPpM7tRcL5oWMgpMCqQDbhH8UrjDFg2H9i5AQWXuU1qacJgUUCVLTsgDmZKXGz1vPLXY8QB5ypYqG", MoneroNetworkType.MAINNET));

        // test mainnet integrated address validation
        MoneroUtils.ValidateAddress("4CApvrfMgUFZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKeGLQ9vfRBRKFKnBtVH", MoneroNetworkType.MAINNET);
        MoneroUtils.ValidateAddress("4JGdXrMXaMP4nJ8fdz2w65TrTEp9PRsv5J8iHSShkHQcE2V31FhnWptioNst1K9oeDY4KpWZ7v8V2BZNVa4Wdky8DvDyXvDZXvE9jTQwom", MoneroNetworkType.MAINNET);
        MoneroUtils.ValidateAddress("4JCp7q5SchvMCHVKENnPpM7tRcL5oWMgpMCqQDbhH8UrjDFg2H9i5AQWXuU1qacJgUUCVLTsgDmZKXGz1vPLXY8QFySJXARQWju8AuRN2z", MoneroNetworkType.MAINNET);

        // test mainnet subaddress validation
        MoneroUtils.ValidateAddress("891TQPrWshJVpnBR4ZMhHiHpLx1PUnMqa3ccV5TJFBbqcJa3DWhjBh2QByCv3Su7WDPTGMHmCKkiVFN2fyGJKwbM1t6G7Ea", MoneroNetworkType.MAINNET);
        MoneroUtils.ValidateAddress("88fyq3t8Gxn1QWMG189EufHtMHXZXkfJtJKFJXqeA4GpSiuyfjVwVyp47PeQJnD7Tc8iK8TDvvhcmEmfh8nx7Va2ToP8wAo", MoneroNetworkType.MAINNET);
        MoneroUtils.ValidateAddress("88hnoBiX3TPjbFaQE8RxgyBcf3DtMKZWWQMoArBjQfn37JJwtm568mPX6ipcCuGKDnLCzgjmpLSqce4aBDyapJJAFtNxUMb", MoneroNetworkType.MAINNET);

        // test testnet primary address validation
        MoneroUtils.ValidateAddress("9tUBnNCkC3UKGygHCwYvAB1FscpjUuq5e9MYJd2rXuiiTjjfVeSVjnbSG5VTnJgBgy9Y7GTLfxpZNMUwNZjGfdFr1z79eV1", MoneroNetworkType.TESTNET);
        MoneroUtils.ValidateAddress("9xZmQa1kYakGoHcfXeBgcsLf622NCpChcACwXxfdgY9uAa9hXSPCV9cLvUsAShfDcFKDdPzCNJ1n5cFGKw5GVM722pjuGPd", MoneroNetworkType.TESTNET);
        MoneroUtils.ValidateAddress("A2TXS6QFQ4wEsp8U7C2Y4B7wBtiML8aDG7mdCbRvDQmRaRNj1YSSgJE46fSzUkwgpMUCXFqscvrQuN7oKpP6eDyQ7XuYsuf", MoneroNetworkType.TESTNET);

        // test testnet integrated address validation
        MoneroUtils.ValidateAddress("A4AroB2EoJzKGygHCwYvAB1FscpjUuq5e9MYJd2rXuiiTjjfVeSVjnbSG5VTnJgBgy9Y7GTLfxpZNMUwNZjGfdFr2QY5Ba2aHhTEdQa2ra", MoneroNetworkType.TESTNET);
        MoneroUtils.ValidateAddress("A8GSRNqF9rGGoHcfXeBgcsLf622NCpChcACwXxfdgY9uAa9hXSPCV9cLvUsAShfDcFKDdPzCNJ1n5cFGKw5GVM723iPoCEF1Fs9BcPYxTW", MoneroNetworkType.TESTNET);
        MoneroUtils.ValidateAddress("ACACSuDk1LTEsp8U7C2Y4B7wBtiML8aDG7mdCbRvDQmRaRNj1YSSgJE46fSzUkwgpMUCXFqscvrQuN7oKpP6eDyQAdgDoT3UnMYKQz7SHC", MoneroNetworkType.TESTNET);

        // test testnet subaddress validation
        MoneroUtils.ValidateAddress("BgnKzHPJQDcg7xiP7bMN9MfPv9Z8ciT71iEMYnCdgBRBFETWgu9nKTr8fnzyGfU9h9gyNA8SFzYYzHfTS9KhqytSU943Nu1", MoneroNetworkType.TESTNET);
        MoneroUtils.ValidateAddress("BZwiuKkoNP59zgPHTxpNw3PM4DW2xiAVQJWqfFRrGyeZ7afVdQqoiJg3E2dDL3Ja8BV4ov2LEoHx9UjzF3W4ihPBSZvWwTx", MoneroNetworkType.TESTNET);
        MoneroUtils.ValidateAddress("Bhf1DEYrentcehUvNreLK5gxosnC2VStMXNCCs163RTxQq4jxFYvpw7LrQFmrMwWW2KsXLhMRtyho6Lq11ci3Fb246bxYmi", MoneroNetworkType.TESTNET);

        // test stagenet primary address validation
        MoneroUtils.ValidateAddress("5B8s3obCY2ETeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRVQkunGS", MoneroNetworkType.STAGENET);
        MoneroUtils.ValidateAddress("57VfotUbSZLG82UkKhWXDjS5ZEK9ZCDcmjdk4gpVq2fbKdEgwRCFrGTLZ2MMdSHphRWJDWVBi5qS8T7dz13JTCWtC228zyn", MoneroNetworkType.STAGENET);
        MoneroUtils.ValidateAddress("52FysgWJYmAG73QUQZRULJj2Dv2C2mceUMB5zHqNzMn8WBtfPWQrSUFSQUKTX9r7bUMmVSGbrau976xYLynR8jTWLdA7rfp", MoneroNetworkType.STAGENET);

        // test stagenet integrated address validation
        MoneroUtils.ValidateAddress("5LqY4cQh9HkTeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRj6LZRFrjuGK8Whthg2", MoneroNetworkType.STAGENET);
        MoneroUtils.ValidateAddress("5HCLphJ63prG82UkKhWXDjS5ZEK9ZCDcmjdk4gpVq2fbKdEgwRCFrGTLZ2MMdSHphRWJDWVBi5qS8T7dz13JTCWtHETX8zcUhDjVKcynf6", MoneroNetworkType.STAGENET);
        MoneroUtils.ValidateAddress("5BxetVKoA2gG73QUQZRULJj2Dv2C2mceUMB5zHqNzMn8WBtfPWQrSUFSQUKTX9r7bUMmVSGbrau976xYLynR8jTWVwQwpHNg5fCLgtA2Dv", MoneroNetworkType.STAGENET);

        // test stagenet subaddress validation
        MoneroUtils.ValidateAddress("778B5D2JmMh5TJVWFbygJR15dvio5Z5B24hfSrWDzeroM8j8Lqc9sMoFE6324xg2ReaAZqHJkgfGFRugRmYHugHZ4f17Gxo", MoneroNetworkType.STAGENET);
        MoneroUtils.ValidateAddress("73U97wGEH9RCVUf6bopo45jSgoqjMzz4mTUsvWs5EusmYAmFcBYFm7wKMVmgtVKCBhMQqXrcMbHvwck2md63jMZSFJxUhQ2", MoneroNetworkType.STAGENET);
        MoneroUtils.ValidateAddress("747wPpaPKrjDPZrF48jAfz9pRRUHLMCWfYu2UanP4ZfTG8NrmYrSEWNW8gYoadU8hTiwBjV14e6DLaC5xfhyEpX5154aMm6", MoneroNetworkType.STAGENET);

        // test invalid addresses on mainnet
        TestInvalidAddress(null, MoneroNetworkType.MAINNET);
        TestInvalidAddress("", MoneroNetworkType.MAINNET);
        TestInvalidAddress("42ZxX3Y2y5s4nJ8fdz2w65TrTEp9PRsv5J8iHSShkHQcE2V31FhnWptioNst1K9oeDY4KpWZ7v8V2BZNVa4Wdky89iqmPz2", MoneroNetworkType.MAINNET);
        TestInvalidAddress("41ApvrfMgUFZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKeGLQ9vfRBRKFKnBtVH", MoneroNetworkType.MAINNET);
        TestInvalidAddress("81fyq3t8Gxn1QWMG189EufHtMHXZXkfJtJKFJXqeA4GpSiuyfjVwVyp47PeQJnD7Tc8iK8TDvvhcmEmfh8nx7Va2ToP8wAo", MoneroNetworkType.MAINNET);

        // test invalid addresses on testnet
        TestInvalidAddress(null, MoneroNetworkType.TESTNET);
        TestInvalidAddress("", MoneroNetworkType.TESTNET);
        TestInvalidAddress("91UBnNCkC3UKGygHCwYvAB1FscpjUuq5e9MYJd2rXuiiTjjfVeSVjnbSG5VTnJgBgy9Y7GTLfxpZNMUwNZjGfdFr1z79eV1", MoneroNetworkType.TESTNET);
        TestInvalidAddress("A1AroB2EoJzKGygHCwYvAB1FscpjUuq5e9MYJd2rXuiiTjjfVeSVjnbSG5VTnJgBgy9Y7GTLfxpZNMUwNZjGfdFr2QY5Ba2aHhTEdQa2ra", MoneroNetworkType.TESTNET);
        TestInvalidAddress("B1nKzHPJQDcg7xiP7bMN9MfPv9Z8ciT71iEMYnCdgBRBFETWgu9nKTr8fnzyGfU9h9gyNA8SFzYYzHfTS9KhqytSU943Nu1", MoneroNetworkType.TESTNET);

        // test invalid addresses on stagenet
        TestInvalidAddress(null, MoneroNetworkType.STAGENET);
        TestInvalidAddress("", MoneroNetworkType.STAGENET);
        TestInvalidAddress("518s3obCY2ETeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRVQkunGS", MoneroNetworkType.STAGENET);
        TestInvalidAddress("51qY4cQh9HkTeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRj6LZRFrjuGK8Whthg2", MoneroNetworkType.STAGENET);
        TestInvalidAddress("718B5D2JmMh5TJVWFbygJR15dvio5Z5B24hfSrWDzeroM8j8Lqc9sMoFE6324xg2ReaAZqHJkgfGFRugRmYHugHZ4f17Gxo", MoneroNetworkType.STAGENET);
    }

    // Can validate keys
    [Test]
    public void TestKeyValidation()
    {

        // test private view key validation
        Assert.That(MoneroUtils.IsValidPrivateViewKey("86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d"));
        TestInvalidPrivateViewKey("");
        TestInvalidPrivateViewKey(null);
        TestInvalidPrivateViewKey("5B8s3obCY2ETeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRVQkunGS");

        // test public view key validation
        Assert.That(MoneroUtils.IsValidPublicViewKey("99873d76ca874ff1aad676b835dd303abcb21c9911ca8a3d9130abc4544d8a0a"));
        TestInvalidPublicViewKey("");
        TestInvalidPublicViewKey(null);
        TestInvalidPublicViewKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");

        // test private spend key validation
        Assert.That(MoneroUtils.IsValidPrivateSpendKey("e9ba887e93620ef9fafdfe0c6d3022949f1c5713cbd9ef631f18a0fb00421dee"));
        TestInvalidPrivateSpendKey("");
        TestInvalidPrivateSpendKey(null);
        TestInvalidPrivateSpendKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");

        // test public spend key validation
        Assert.That(MoneroUtils.IsValidPublicSpendKey("3e48df9e9d8038dbf6f5382fac2becd8686273cda5bd87187e45dca7ec5af37b"));
        TestInvalidPublicSpendKey("");
        TestInvalidPublicSpendKey(null);
        TestInvalidPublicSpendKey("z86cf351d10894769feba29b9e201e12fb100b85bb52fc5825c864eef55c5840d");
    }

    // Can convert between XMR and atomic units
    [Test]
    public void TestAtomicUnitConversion()
    {
        Assert.That(1000000000000 == MoneroUtils.XmrToAtomicUnits(1));
        Assert.That(1000000000000 == MoneroUtils.AtomicUnitsToXmr(1000000000000));
        Assert.That(1000000000 == MoneroUtils.XmrToAtomicUnits(0.001));
        Assert.That(.001 == MoneroUtils.AtomicUnitsToXmr(1000000000));
        Assert.That(250000000000 == MoneroUtils.XmrToAtomicUnits(.25));
        Assert.That(.25 == MoneroUtils.AtomicUnitsToXmr(250000000000));
        Assert.That(1250000000000 == MoneroUtils.XmrToAtomicUnits(1.25));
        Assert.That(1.25 == MoneroUtils.AtomicUnitsToXmr(1250000000000));
        Assert.That(2796726190000 == MoneroUtils.XmrToAtomicUnits(2.79672619));
        Assert.That(2.79672619 == MoneroUtils.AtomicUnitsToXmr(2796726190000));
        Assert.That(2796726190001 == MoneroUtils.XmrToAtomicUnits(2.796726190001));
        Assert.That(2.796726190001 == MoneroUtils.AtomicUnitsToXmr(2796726190001));
        Assert.That(2796726189999 == MoneroUtils.XmrToAtomicUnits(2.796726189999));
        Assert.That(2.796726189999 == MoneroUtils.AtomicUnitsToXmr(2796726189999));
        Assert.That(2796726180000 == MoneroUtils.XmrToAtomicUnits(2.79672618));
        Assert.That(2.79672618 == MoneroUtils.AtomicUnitsToXmr(2796726180000));
    }

    [Test]
    [Ignore("GetPaymentUri is not implemented yet")]
    public void TestGetPaymentUri()
    {
        MoneroTxConfig config = new MoneroTxConfig()
            .SetAddress("42U9v3qs5CjZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKS3rvM3L")
            .SetAmount(MoneroUtils.XmrToAtomicUnits(0.25))
            .SetRecipientName("John Doe")
            .SetNote("My transfer to wallet");
        string paymentUri = ""; //MoneroUtils.GetPaymentUri(config);
        Assert.That("monero:42U9v3qs5CjZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKS3rvM3L?tx_amount=0.25&recipient_name=John%20Doe&tx_description=My%20transfer%20to%20wallet" == paymentUri);
    }

    #region Private Helpers

    private static void TestInvalidAddress(string address, MoneroNetworkType networkType)
    {
        Assert.That(false == MoneroUtils.IsValidAddress(address, networkType));
        try
        {
            MoneroUtils.ValidateAddress(address, networkType);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0);
        }
    }

    private static void TestInvalidPrivateViewKey(string privateViewKey)
    {
        Assert.That(false == MoneroUtils.IsValidPrivateViewKey(privateViewKey));
        try
        {
            MoneroUtils.ValidatePrivateViewKey(privateViewKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0);
        }
    }

    private static void TestInvalidPublicViewKey(string publicViewKey)
    {
        Assert.That(false == MoneroUtils.IsValidPublicViewKey(publicViewKey));
        try
        {
            MoneroUtils.ValidatePublicViewKey(publicViewKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0);
        }
    }

    private static void TestInvalidPrivateSpendKey(string privateSpendKey)
    {
        try
        {
            Assert.That(false == MoneroUtils.IsValidPrivateSpendKey(privateSpendKey));
            MoneroUtils.ValidatePrivateSpendKey(privateSpendKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0);
        }
    }

    private static void TestInvalidPublicSpendKey(string publicSpendKey)
    {
        Assert.That(false == MoneroUtils.IsValidPublicSpendKey(publicSpendKey));
        try
        {
            MoneroUtils.ValidatePublicSpendKey(publicSpendKey);
            throw new Exception("Should have thrown exception");
        }
        catch (MoneroError e)
        {
            Assert.That(e.Message.Length > 0);
        }
    }

    #endregion

}
