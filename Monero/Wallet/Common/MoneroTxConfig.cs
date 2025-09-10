using System.Text;
using System.Web;

using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroTxConfig
{
    private uint accountIndex;
    private ulong belowAmount;
    private bool canSplit;
    private List<MoneroDestination>? destinations;
    private ulong fee;
    private string? keyImage;
    private string? note;
    private string? paymentId;
    private MoneroTxPriority? priority;
    private string? recipientName;
    private bool relay;
    private List<uint>? subaddressIndices;
    private List<uint>? subtractFeeFrom;
    private bool sweepEachSubaddress;

    public MoneroTxConfig() { }

    public MoneroTxConfig(MoneroTxConfig config)
    {
        destinations = [.. config.destinations];
        subtractFeeFrom = [.. config.subtractFeeFrom];
        paymentId = config.paymentId;
        priority = config.priority;
        fee = config.fee;
        accountIndex = config.accountIndex;
        subaddressIndices = [.. config.subaddressIndices];
        canSplit = config.canSplit;
        relay = config.relay;
        note = config.note;
        recipientName = config.recipientName;
        belowAmount = config.belowAmount;
        sweepEachSubaddress = config.sweepEachSubaddress;
        keyImage = config.keyImage;
    }

    public string GetPaymentUri()
    {
        return GetPaymentUri(this);
    }

    public static string GetPaymentUri(MoneroTxConfig config)
    {
        if (config.GetAddress() == null)
        {
            throw new ArgumentException("Payment URI requires an address");
        }

        StringBuilder sb = new();
        sb.Append("monero:");
        sb.Append(config.GetAddress());

        StringBuilder paramSb = new();

        if (config.GetAmount() != null)
        {
            paramSb.Append("&tx_amount=").Append(MoneroUtils.AtomicUnitsToXmr((ulong)config.GetAmount()));
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
            paramStr = "?" + paramStr.Substring(1); // Replace first '&' with '?'
        }

        return sb + paramStr;
    }


    public MoneroTxConfig Clone()
    {
        return new MoneroTxConfig(this);
    }

    public MoneroTxConfig SetAddress(string address)
    {
        if (destinations.Count > 1)
        {
            throw new MoneroError("Cannot Set address when multiple destinations are specified.");
        }

        destinations.Clear();
        destinations.Add(new MoneroDestination(address, 0));
        return this;
    }

    public string? GetAddress()
    {
        if (destinations.Count != 1)
        {
            throw new MoneroError("Cannot Get address because MoneroTxConfig does not have exactly one destination");
        }

        return destinations[0].GetAddress();
    }

    public MoneroTxConfig SetAmount(ulong amount)
    {
        if (destinations != null && destinations.Count > 1)
        {
            throw new MoneroError("Cannot Set amount because MoneroTxConfig already has multiple destinations");
        }

        if (destinations == null || destinations.Count == 0)
        {
            AddDestination(new MoneroDestination(null, amount));
        }
        else
        {
            destinations[0].SetAmount(amount);
        }

        return this;
    }

    public MoneroTxConfig SetAmount(string amount)
    {
        return SetAmount(ulong.Parse(amount));
    }


    public ulong? GetAmount()
    {
        if (destinations == null || destinations.Count != 1)
        {
            throw new MoneroError("Cannot Get amount because MoneroTxConfig does not have exactly one destination");
        }

        return destinations[0].GetAmount();
    }

    public MoneroTxConfig AddDestination(string address, ulong amount)
    {
        return AddDestination(new MoneroDestination(address, amount));
    }

    public MoneroTxConfig AddDestination(MoneroDestination destination)
    {
        if (destinations == null)
        {
            destinations = [];
        }

        destinations.Add(destination);
        return this;
    }

    public List<MoneroDestination>? GetDestinations()
    {
        return destinations;
    }

    public MoneroTxConfig SetDestinations(List<MoneroDestination> destinations)
    {
        this.destinations = destinations;
        return this;
    }

    public List<uint>? GetSubtractFeeFrom()
    {
        return subtractFeeFrom;
    }

    public MoneroTxConfig SetSubtractFeeFrom(List<uint> destinationIndices)
    {
        subtractFeeFrom = destinationIndices;
        return this;
    }

    public string? GetPaymentId()
    {
        return paymentId;
    }

    public MoneroTxConfig SetPaymentId(string? paymentId)
    {
        this.paymentId = paymentId;
        return this;
    }

    public MoneroTxPriority? GetPriority()
    {
        return priority;
    }

    public MoneroTxConfig SetPriority(MoneroTxPriority priority)
    {
        this.priority = priority;
        return this;
    }

    public ulong GetFee()
    {
        return fee;
    }

    public MoneroTxConfig SetFee(ulong fee)
    {
        this.fee = fee;
        return this;
    }

    public uint GetAccountIndex()
    {
        return accountIndex;
    }

    public MoneroTxConfig SetAccountIndex(uint accountIndex)
    {
        this.accountIndex = accountIndex;
        return this;
    }

    public List<uint>? GetSubaddressIndices()
    {
        return subaddressIndices;
    }

    public MoneroTxConfig SetSubaddressIndex(uint subaddressIndex)
    {
        SetSubaddressIndices([subaddressIndex]);
        return this;
    }

    public MoneroTxConfig SetSubaddressIndices(List<uint>? subaddressIndicesList)
    {
        this.subaddressIndices = subaddressIndicesList;
        return this;
    }

    public MoneroTxConfig SetSubaddressIndices(uint subaddressIndex)
    {
        subaddressIndices = [subaddressIndex];
        return this;
    }

    public bool GetCanSplit()
    {
        return canSplit;
    }

    public MoneroTxConfig SetCanSplit(bool canSplit)
    {
        this.canSplit = canSplit;
        return this;
    }

    public bool GetRelay()
    {
        return relay;
    }

    public MoneroTxConfig SetRelay(bool relay)
    {
        this.relay = relay;
        return this;
    }

    public string? GetNote()
    {
        return note;
    }

    public MoneroTxConfig SetNote(string? note)
    {
        this.note = note;
        return this;
    }

    public string? GetRecipientName()
    {
        return recipientName;
    }

    public MoneroTxConfig SetRecipientName(string? recipientName)
    {
        this.recipientName = recipientName;
        return this;
    }

    public ulong GetBelowAmount()
    {
        return belowAmount;
    }

    public MoneroTxConfig SetBelowAmount(ulong belowAmount)
    {
        this.belowAmount = belowAmount;
        return this;
    }

    public bool GetSweepEachSubaddress()
    {
        return sweepEachSubaddress;
    }

    public MoneroTxConfig SetSweepEachSubaddress(bool sweepEachSubaddress)
    {
        this.sweepEachSubaddress = sweepEachSubaddress;
        return this;
    }

    public string? GetKeyImage()
    {
        return keyImage;
    }

    public MoneroTxConfig SetKeyImage(string keyImage)
    {
        this.keyImage = keyImage;
        return this;
    }
}