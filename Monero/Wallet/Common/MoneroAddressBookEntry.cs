namespace Monero.Wallet.Common;

public class MoneroAddressBookEntry
{
    private string? _address;
    private string? _description;
    private uint? _index;
    private string? _paymentId;

    public MoneroAddressBookEntry()
    {

    }

    public MoneroAddressBookEntry(uint? index)
    {
        this._index = index;
    }

    public MoneroAddressBookEntry(uint? index, string? address)
    {
        this._index = index;
        this._address = address;
    }

    public MoneroAddressBookEntry(uint? index, string? address, string? description)
    {
        this._index = index;
        this._address = address;
        this._description = description;
    }

    public MoneroAddressBookEntry(uint? index, string? address, string? description,
        string? paymentId)
    {
        this._index = index;
        this._address = address;
        this._paymentId = paymentId;
        this._description = description;
    }


    public uint? GetIndex()
    {
        return _index;
    }

    public MoneroAddressBookEntry SetIndex(uint? index)
    {
        this._index = index;
        return this;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroAddressBookEntry SetAddress(string? address)
    {
        this._address = address;
        return this;
    }

    public string? GetPaymentId()
    {
        return _paymentId;
    }

    public MoneroAddressBookEntry SetPaymentId(string? paymentId)
    {
        this._paymentId = paymentId;
        return this;
    }

    public string? GetDescription()
    {
        return _description;
    }

    public MoneroAddressBookEntry SetDescription(string? description)
    {
        this._description = description;
        return this;
    }
}