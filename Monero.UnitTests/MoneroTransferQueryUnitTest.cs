using Monero.Common;
using Monero.Wallet;
using Monero.Wallet.Common;

using NUnit.Framework;

namespace Monero.UnitTests;

public class MoneroTransferQueryUnitTest
{
    // Can set and get account and subaddress indices
    [Test]
    public void TestAccountAndSubaddressIndexSetters()
    {
        MoneroTransferQuery query = new MoneroTransferQuery().SetAccountIndex(2).SetSubaddressIndex(5);

        Assert.That(query.GetAccountIndex(), Is.EqualTo(2));
        Assert.That(query.GetSubaddressIndex(), Is.EqualTo(5));

        query.SetAccountIndex(null).SetSubaddressIndex(null);

        Assert.That(query.GetAccountIndex(), Is.Null);
        Assert.That(query.GetSubaddressIndex(), Is.Null);
    }

    // Clone copies account and subaddress indices
    [Test]
    public void TestCloneCopiesIndices()
    {
        MoneroTransferQuery query = new MoneroTransferQuery().SetAccountIndex(1).SetSubaddressIndex(3);

        MoneroTransferQuery clone = query.Clone();

        Assert.That(clone.GetAccountIndex(), Is.EqualTo(1));
        Assert.That(clone.GetSubaddressIndex(), Is.EqualTo(3));
    }

    // Mutating a clone's lists does not affect the original query
    [Test]
    public void TestCloneIsIndependentOfOriginal()
    {
        MoneroTransferQuery query = new MoneroTransferQuery()
            .SetSubaddressIndices([0, 1])
            .SetAddresses(["address1"])
            .SetDestinations([new TransferDestination { Address = "address1", Amount = 1 }]);

        MoneroTransferQuery clone = query.Clone();

        // A new instance is created for the clone, the query and clone cant reference the same object
        // Checking for accidently returning the same object reference, which would be a bug
        Assert.That(clone, Is.Not.SameAs(query));

        // Lists are distinct instances in memory (different pointer reference), clone must not point to the same object reference
        Assert.That(clone.GetSubaddressIndices(), Is.Not.SameAs(query.GetSubaddressIndices()));
        Assert.That(clone.GetAddresses(), Is.Not.SameAs(query.GetAddresses()));
        Assert.That(clone.GetDestinations(), Is.Not.SameAs(query.GetDestinations()));

        // Different object reference, but still contain the same elements
        Assert.That(clone.GetSubaddressIndices(), Is.EqualTo(query.GetSubaddressIndices()));
        Assert.That(clone.GetAddresses(), Is.EqualTo(query.GetAddresses()));
        Assert.That(clone.GetDestinations(), Is.EqualTo(query.GetDestinations()));
    }

    // Filtering transfers by subaddress index without an account index is rejected before any RPC call is made
    [Test]
    public async Task TestGetTransfersSubaddressWithoutAccountThrows()
    {
        // unreachable endpoint for testing is fine because the validation must throw before any request is sent anyway
        MoneroWalletRpc wallet = new(new MoneroRpcConnection(new Uri("http://127.0.0.1:1"), "", ""));

        // check single subaddress index without account index
        MoneroError? singleIndexEx = await Assert.ThrowsAsync<MoneroError>(async () =>
            await wallet.GetTransfers(new MoneroTransferQuery().SetSubaddressIndex(1)));
        Assert.That(singleIndexEx!.Message, Is.EqualTo("Filter specifies a subaddress index but not an account index"));

        // check multiple subaddress indices without account index
        MoneroError? multipleIndicesEx = await Assert.ThrowsAsync<MoneroError>(async () =>
            await wallet.GetTransfers(new MoneroTransferQuery().SetSubaddressIndices([0, 1])));
        Assert.That(multipleIndicesEx!.Message, Is.EqualTo("Filter specifies a subaddress index but not an account index"));
    }
}