namespace Monero.Wallet.Common;

public class MoneroOutgoingTransfer : MoneroTransfer
{
    private List<string>? addresses;
    private List<MoneroDestination>? destinations;
    private List<uint>? subaddressIndices;

    public MoneroOutgoingTransfer()
    {
        // nothing to initialize
    }

    public MoneroOutgoingTransfer(MoneroOutgoingTransfer transfer) : base(transfer)
    {
        if (transfer.subaddressIndices != null)
        {
            subaddressIndices = new List<uint>(transfer.subaddressIndices);
        }

        if (transfer.addresses != null)
        {
            addresses = new List<string>(transfer.addresses);
        }

        if (transfer.destinations != null)
        {
            destinations = [];
            foreach (MoneroDestination destination in transfer.GetDestinations())
            {
                destinations.Add(destination.Clone());
            }
        }
    }

    public override MoneroOutgoingTransfer Clone()
    {
        return new MoneroOutgoingTransfer(this);
    }

    public override MoneroOutgoingTransfer SetTx(MoneroTxWallet tx)
    {
        _tx = tx;
        return this;
    }

    public override bool? IsIncoming()
    {
        return false;
    }

    public List<uint>? GetSubaddressIndices()
    {
        return subaddressIndices;
    }

    public MoneroOutgoingTransfer SetSubaddressIndices(List<uint> subaddressIndices)
    {
        this.subaddressIndices = subaddressIndices;
        return this;
    }

    public List<string>? GetAddresses()
    {
        return addresses;
    }

    public MoneroOutgoingTransfer SetAddresses(List<string> addresses)
    {
        this.addresses = addresses;
        return this;
    }

    public List<MoneroDestination>? GetDestinations()
    {
        return destinations;
    }

    public MoneroOutgoingTransfer SetDestinations(List<MoneroDestination>? destinations)
    {
        this.destinations = destinations;
        return this;
    }
}