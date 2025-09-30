using System.Text;
using System.Web;

using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroTxConfig
{
    private uint? _accountIndex;
    private ulong? _belowAmount;
    private bool? _canSplit;
    private List<MoneroDestination>? _destinations;
    private ulong? _fee;
    private string? _keyImage;
    private string? _note;
    private string? _paymentId;
    private MoneroTxPriority? _priority;
    private string? _recipientName;
    private bool? _relay;
    private List<uint>? _subaddressIndices;
    private List<uint>? _subtractFeeFrom;
    private bool? _sweepEachSubaddress;

    public MoneroTxConfig() { }

    public MoneroTxConfig(MoneroTxConfig config)
    {
        if (config._destinations != null)
        {
            _destinations = [.. config._destinations];
        }

        if (config._subtractFeeFrom != null)
        {
            _subtractFeeFrom = [.. config._subtractFeeFrom];
        }

        _paymentId = config._paymentId;
        _priority = config._priority;
        _fee = config._fee;
        _accountIndex = config._accountIndex;
        if (config._subaddressIndices != null)
        {
            _subaddressIndices = [.. config._subaddressIndices];
        }

        _canSplit = config._canSplit;
        _relay = config._relay;
        _note = config._note;
        _recipientName = config._recipientName;
        _belowAmount = config._belowAmount;
        _sweepEachSubaddress = config._sweepEachSubaddress;
        _keyImage = config._keyImage;
    }

    public string GetPaymentUri()
    {
        return GetPaymentUri(this);
    }

    public static string GetPaymentUri(MoneroTxConfig? config)
    {
        if (config == null)
        {
            throw new ArgumentException("Must provide a valid tx config");
        }

        if (config.GetAddress() == null)
        {
            throw new ArgumentException("Payment URI requires an address");
        }

        StringBuilder sb = new();
        sb.Append("monero:");
        sb.Append(config.GetAddress());

        StringBuilder paramSb = new();
        ulong? amount = config.GetAmount();
        if (amount != null)
        {
            paramSb.Append("&tx_amount=").Append(MoneroUtils.AtomicUnitsToXmr((ulong)amount));
        }

        if (!string.IsNullOrEmpty(config.GetRecipientName()))
        {
            paramSb.Append("&recipient_name=").Append(HttpUtility.UrlEncode(config.GetRecipientName()));
        }

        if (!string.IsNullOrEmpty(config.GetNote()))
        {
            paramSb.Append("&tx_description=").Append(HttpUtility.UrlEncode(config.GetNote()));
        }

        if (!string.IsNullOrEmpty(config.GetPaymentId()))
        {
            throw new ArgumentException("Standalone payment id deprecated, use integrated address instead");
        }

        string paramStr = paramSb.ToString();
        if (paramStr.Length > 0)
        {
            paramStr = "?" + paramStr.Substring(1); // Replace the first '&' with '?'
        }

        return sb + paramStr;
    }


    public MoneroTxConfig Clone()
    {
        return new MoneroTxConfig(this);
    }

    public MoneroTxConfig SetAddress(string? address)
    {
        if (_destinations == null)
        {
            _destinations = [];
        }

        if (_destinations.Count > 1)
        {
            throw new MoneroError("Cannot Set address when multiple destinations are specified.");
        }

        _destinations.Clear();
        _destinations.Add(new MoneroDestination { Address = address, Amount = 0 });
        return this;
    }

    public string? GetAddress()
    {
        if (_destinations == null || _destinations.Count != 1)
        {
            throw new MoneroError("Cannot Get address because MoneroTxConfig does not have exactly one destination");
        }

        return _destinations[0].Address;
    }

    public MoneroTxConfig SetAmount(ulong? amount)
    {
        if (_destinations != null && _destinations.Count > 1)
        {
            throw new MoneroError("Cannot Set amount because MoneroTxConfig already has multiple destinations");
        }

        if (_destinations == null || _destinations.Count == 0)
        {
            AddDestination(new MoneroDestination { Address = null, Amount = amount });
        }
        else
        {
            _destinations[0].Amount = amount;
        }

        return this;
    }

    public MoneroTxConfig SetAmount(string amount)
    {
        return SetAmount(ulong.Parse(amount));
    }


    public ulong? GetAmount()
    {
        if (_destinations == null || _destinations.Count != 1)
        {
            throw new MoneroError("Cannot Get amount because MoneroTxConfig does not have exactly one destination");
        }

        return _destinations[0].Amount;
    }

    public MoneroTxConfig AddDestination(string address, ulong amount)
    {
        return AddDestination(new MoneroDestination { Address = address, Amount = amount });
    }

    public MoneroTxConfig AddDestination(MoneroDestination destination)
    {
        if (_destinations == null)
        {
            _destinations = [];
        }

        _destinations.Add(destination);
        return this;
    }

    public List<MoneroDestination>? GetDestinations()
    {
        return _destinations;
    }

    public MoneroTxConfig SetDestinations(List<MoneroDestination> destinations)
    {
        _destinations = destinations;
        return this;
    }

    public List<uint>? GetSubtractFeeFrom()
    {
        return _subtractFeeFrom;
    }

    public MoneroTxConfig SetSubtractFeeFrom(List<uint> destinationIndices)
    {
        _subtractFeeFrom = destinationIndices;
        return this;
    }

    public string? GetPaymentId()
    {
        return _paymentId;
    }

    public MoneroTxConfig SetPaymentId(string? paymentId)
    {
        _paymentId = paymentId;
        return this;
    }

    public MoneroTxPriority? GetPriority()
    {
        return _priority;
    }

    public MoneroTxConfig SetPriority(MoneroTxPriority priority)
    {
        _priority = priority;
        return this;
    }

    public ulong? GetFee()
    {
        return _fee;
    }

    public MoneroTxConfig SetFee(ulong? fee)
    {
        _fee = fee;
        return this;
    }

    public uint? GetAccountIndex()
    {
        return _accountIndex;
    }

    public MoneroTxConfig SetAccountIndex(uint? accountIndex)
    {
        _accountIndex = accountIndex;
        return this;
    }

    public List<uint>? GetSubaddressIndices()
    {
        return _subaddressIndices;
    }

    public MoneroTxConfig SetSubaddressIndex(uint subaddressIndex)
    {
        SetSubaddressIndices([subaddressIndex]);
        return this;
    }

    public MoneroTxConfig SetSubaddressIndices(List<uint>? subaddressIndicesList)
    {
        _subaddressIndices = subaddressIndicesList;
        return this;
    }

    public MoneroTxConfig SetSubaddressIndices(uint subaddressIndex)
    {
        _subaddressIndices = [subaddressIndex];
        return this;
    }

    public bool? GetCanSplit()
    {
        return _canSplit;
    }

    public MoneroTxConfig SetCanSplit(bool? canSplit)
    {
        _canSplit = canSplit;
        return this;
    }

    public bool? GetRelay()
    {
        return _relay;
    }

    public MoneroTxConfig SetRelay(bool? relay)
    {
        _relay = relay;
        return this;
    }

    public string? GetNote()
    {
        return _note;
    }

    public MoneroTxConfig SetNote(string? note)
    {
        _note = note;
        return this;
    }

    public string? GetRecipientName()
    {
        return _recipientName;
    }

    public MoneroTxConfig SetRecipientName(string? recipientName)
    {
        _recipientName = recipientName;
        return this;
    }

    public ulong? GetBelowAmount()
    {
        return _belowAmount;
    }

    public MoneroTxConfig SetBelowAmount(ulong? belowAmount)
    {
        _belowAmount = belowAmount;
        return this;
    }

    public bool? GetSweepEachSubaddress()
    {
        return _sweepEachSubaddress;
    }

    public MoneroTxConfig SetSweepEachSubaddress(bool? sweepEachSubaddress)
    {
        _sweepEachSubaddress = sweepEachSubaddress;
        return this;
    }

    public string? GetKeyImage()
    {
        return _keyImage;
    }

    public MoneroTxConfig SetKeyImage(string keyImage)
    {
        _keyImage = keyImage;
        return this;
    }
}