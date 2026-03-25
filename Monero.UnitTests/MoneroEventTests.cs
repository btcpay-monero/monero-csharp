using Monero.Common;

using NUnit.Framework;

namespace Monero.UnitTests;

public class MoneroEventTest
{
    [Test]
    public void DefaultInitialization_ShouldHaveNullProperties()
    {
        var moneroEvent = new MoneroEvent();

        Assert.That(moneroEvent.BlockHash, Is.Null);
        Assert.That(moneroEvent.TransactionHash, Is.Null);
        Assert.That(moneroEvent.CryptoCode, Is.Null);
    }

    [Test]
    public void PropertyAssignment_ShouldSetAndRetrieveValues()
    {
        var moneroEvent = new MoneroEvent
        {
            BlockHash = "block123",
            TransactionHash = "tx456",
            CryptoCode = "XMR"
        };

        Assert.That(moneroEvent.BlockHash, Is.EqualTo("block123"));
        Assert.That(moneroEvent.TransactionHash, Is.EqualTo("tx456"));
        Assert.That(moneroEvent.CryptoCode, Is.EqualTo("XMR"));
    }

    [TestCase("block123", "tx456", "XMR", "XMR: Tx Update New Block (tx456block123)")]
    public void ToString_ShouldReturnCorrectString(string blockHash, string transactionHash, string cryptoCode, string expected)
    {
        var moneroEvent = new MoneroEvent
        {
            BlockHash = blockHash,
            TransactionHash = transactionHash,
            CryptoCode = cryptoCode
        };

        var result = moneroEvent.ToString();

        Assert.That(result, Is.EqualTo(expected));
    }
}