using System.Text;

using Monero.Common;

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

    public bool Equals(MoneroOutgoingTransfer? other)
    {
        if (other == null)
        {
            return false;
        }

        if (other == this)
        {
            return true;
        }

        if (_subaddressIndices == null)
        {
            if (other._subaddressIndices != null)
            {
                return false;
            }
        }
        else
        {
            if (other._subaddressIndices == null)
            {
                return false;
            }

            if (_subaddressIndices.Count != other._subaddressIndices.Count)
            {
                return false;
            }
            int i = 0;

            foreach (var index in _subaddressIndices)
            {
                var otherIndex = other._subaddressIndices[i]!;
                if (index != otherIndex)
                {
                    return false;
                }
                i++;
            }
        }

        if (_addresses == null)
        {
            if (other._addresses != null)
            {
                return false;
            }
        }
        else
        {
            if (other._addresses == null)
            {
                return false;
            }

            if (_addresses.Count != other._addresses.Count)
            {
                return false;
            }
            int i = 0;

            foreach (var address in _addresses)
            {
                var otherAddress = other._addresses[i]!;
                if (address != otherAddress)
                {
                    return false;
                }
                i++;
            }
        }

        if (_destinations == null)
        {
            if (other._destinations != null)
            {
                return false;
            }
        }
        else
        {
            if (other._destinations == null)
            {
                return false;
            }

            if (_destinations.Count != other._destinations.Count)
            {
                return false;
            }

            int i = 0;
            foreach (var destination in _destinations)
            {
                var otherDest = other._destinations[i];
                if (destination.Equals(otherDest))
                {
                    return false;
                }
                i++;
            }
        }

        return true;
    }

    public override string ToString(int indent)
    {
        var sb = new StringBuilder();
        sb.Append(base.ToString(indent) + "\n");
        sb.Append(GenUtils.KvLine("Subaddress indices", GetSubaddressIndices(), indent));
        sb.Append(GenUtils.KvLine("Addresses", GetAddresses(), indent));
        if (GetDestinations() != null)
        {
            sb.Append(GenUtils.KvLine("Destinations", "", indent));
            for (int i = 0; i < GetDestinations()!.Count; i++)
            {
                sb.Append(GenUtils.KvLine(i + 1, "", indent + 1));
                sb.Append(GetDestinations()![i].ToString(indent + 2) + "\n");
            }
        }
        string str = sb.ToString();
        return str.Substring(0, str.Length - 1);
    }
}