namespace Monero.Wallet.Common;

public class MoneroAddressBookEntry
{
    private string? address;
    private string? description;
    private uint? index;
    private string? paymentId;

    public MoneroAddressBookEntry()
    {

    }

    public MoneroAddressBookEntry(uint? index)
    {
        this.index = index;
    }

    public MoneroAddressBookEntry(uint? index, string? address)
    {
        this.index = index;
        this.address = address;
    }

    public MoneroAddressBookEntry(uint? index, string? address, string? description)
    {
        this.index = index;
        this.address = address;
        this.description = description;
    }

    public MoneroAddressBookEntry(uint? index, string? address, string? description,
        string? paymentId)
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