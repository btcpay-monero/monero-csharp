using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroAccount
{
    [JsonPropertyName("balance")]
    [JsonInclude]
    private ulong _balance { get; set; }
    [JsonPropertyName("account_index")]
    [JsonInclude]
    private uint? _index { get; set; }
    [JsonPropertyName("base_address")]
    [JsonInclude]
    private string? _primaryAddress { get; set; }
    private List<MoneroSubaddress>? _subaddresses;
    [JsonPropertyName("tag")]
    [JsonInclude]
    private string? _tag { get; set; }
    [JsonPropertyName("unlocked_balance")]
    [JsonInclude]
    private ulong _unlockedBalance { get; set; }

    public MoneroAccount()
    {
        _balance = 0;
        _unlockedBalance = 0;
    }

    public MoneroAccount(uint? index)
    {
        _index = index;
        _balance = 0;
        _unlockedBalance = 0;
    }

    public MoneroAccount(uint? index, string? primaryAddress)
    {
        _index = index;
        _primaryAddress = primaryAddress;
        _balance = 0;
        _unlockedBalance = 0;
    }

    public MoneroAccount(uint? index, string? primaryAddress, ulong? balance, ulong? unlockedBalance,
        List<MoneroSubaddress>? subaddresses)
    {
        _index = index;
        _primaryAddress = primaryAddress;
        _balance = balance ?? 0;
        _unlockedBalance = unlockedBalance ?? 0;
        _subaddresses = subaddresses;
    }

    public uint? GetIndex()
    {
        return _index;
    }

    public MoneroAccount SetIndex(uint? index)
    {
        _index = index;
        return this;
    }

    public string? GetPrimaryAddress()
    {
        return _primaryAddress;
    }

    public MoneroAccount SetPrimaryAddress(string? primaryAddress)
    {
        _primaryAddress = primaryAddress;
        return this;
    }

    public ulong GetBalance()
    {
        return _balance;
    }

    public MoneroAccount SetBalance(ulong balance)
    {
        _balance = balance;
        return this;
    }

    public ulong GetUnlockedBalance()
    {
        return _unlockedBalance;
    }

    public MoneroAccount SetUnlockedBalance(ulong unlockedBalance)
    {
        _unlockedBalance = unlockedBalance;
        return this;
    }

    public string? GetTag()
    {
        return _tag;
    }

    public MoneroAccount SetTag(string? tag)
    {
        _tag = tag;
        return this;
    }

    public List<MoneroSubaddress>? GetSubaddresses()
    {
        return _subaddresses;
    }

    public MoneroAccount SetSubaddresses(List<MoneroSubaddress>? subaddresses)
    {
        _subaddresses = subaddresses;
        if (subaddresses != null)
        {
            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                subaddress.SetAccountIndex(_index);
            }
        }

        return this;
    }
}