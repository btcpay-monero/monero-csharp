namespace Monero.Wallet.Common;

public class MoneroAddressBookEntry
{
    private string? address;
    private string? description;
    private uint? index;
    private string? paymentId;

    public MoneroAddressBookEntry(uint? index = null, string? address = null, string? description = null,
        string? paymentId = null)
    {
        this.index = index;
        this.address = address;
        this.paymentId = paymentId;
        this.description = description;
    }


    public uint? GetIndex()
    {
        return index;
    }

    public MoneroAddressBookEntry SetIndex(uint? index)
    {
        this.index = index;
        return this;
    }

    public string? GetAddress()
    {
        return address;
    }

    public MoneroAddressBookEntry SetAddress(string? address)
    {
        this.address = address;
        return this;
    }

    public string? GetPaymentId()
    {
        return paymentId;
    }

    public MoneroAddressBookEntry SetPaymentId(string? paymentId)
    {
        this.paymentId = paymentId;
        return this;
    }

    public string? GetDescription()
    {
        return description;
    }

    public MoneroAddressBookEntry SetDescription(string? description)
    {
        this.description = description;
        return this;
    }
}