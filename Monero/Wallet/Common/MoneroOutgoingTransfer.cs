namespace Monero.Wallet.Common;

public class MoneroOutgoingTransfer : MoneroTransfer
{
    private List<string>? _addresses;
    private List<MoneroDestination>? _destinations;
    private List<uint>? _subaddressIndices;

    public MoneroOutgoingTransfer()
    {
        // nothing to initialize
    }

    public MoneroOutgoingTransfer(MoneroOutgoingTransfer transfer) : base(transfer)
    {
        if (transfer._subaddressIndices != null)
        {
            _subaddressIndices = transfer._subaddressIndices;
        }

        if (transfer._addresses != null)
        {
            _addresses = transfer._addresses;
        }

        if (transfer._destinations != null)
        {
            _destinations = [];
            foreach (MoneroDestination destination in transfer._destinations)
            {
                _destinations.Add(destination.Clone());
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
        return _subaddressIndices;
    }

    public MoneroOutgoingTransfer SetSubaddressIndices(List<uint> subaddressIndices)
    {
        _subaddressIndices = subaddressIndices;
        return this;
    }

    public List<string>? GetAddresses()
    {
        return _addresses;
    }

    public MoneroOutgoingTransfer SetAddresses(List<string> addresses)
    {
        _addresses = addresses;
        return this;
    }

    public List<MoneroDestination>? GetDestinations()
    {
        return _destinations;
    }

    public MoneroOutgoingTransfer SetDestinations(List<MoneroDestination>? destinations)
    {
        _destinations = destinations;
        return this;
    }
}