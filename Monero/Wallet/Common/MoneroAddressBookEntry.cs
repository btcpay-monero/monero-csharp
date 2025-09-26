using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroAddressBookEntry
{
    [JsonPropertyName("address")]
    private string? _address;
    [JsonPropertyName("description")]
    private string? _description;
    [JsonPropertyName("index")]
    private uint? _index;
    [JsonPropertyName("payment_id")]
    private string? _paymentId;

    public MoneroAddressBookEntry()
    {
    }

    public MoneroAddressBookEntry(uint? index)
    {
        _index = index;
    }

    public MoneroAddressBookEntry(uint? index, string? address)
    {
        _index = index;
        _address = address;
    }

    public MoneroAddressBookEntry(uint? index, string? address, string? description)
    {
        _index = index;
        _address = address;
        _description = description;
    }

    public MoneroAddressBookEntry(uint? index, string? address, string? description,
        string? paymentId)
    {
        _index = index;
        _address = address;
        _paymentId = paymentId;
        _description = description;
    }


    public uint? GetIndex()
    {
        return _index;
    }

    public MoneroAddressBookEntry SetIndex(uint? index)
    {
        _index = index;
        return this;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroAddressBookEntry SetAddress(string? address)
    {
        _address = address;
        return this;
    }

    public string? GetPaymentId()
    {
        return _paymentId;
    }

    public MoneroAddressBookEntry SetPaymentId(string? paymentId)
    {
        _paymentId = paymentId;
        return this;
    }

    public string? GetDescription()
    {
        return _description;
    }

    public MoneroAddressBookEntry SetDescription(string? description)
    {
        _description = description;
        return this;
    }
}