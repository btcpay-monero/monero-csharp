using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroSubaddress
{
    [JsonPropertyName("major")]
    private uint? _accountIndex;
    private string? _address;
    private ulong? _balance;
    [JsonPropertyName("minor")]
    private uint? _index;
    private bool? _isUsed;
    private string? _label = "";
    private ulong? _numBlocksToUnlock;
    private ulong? _numUnspentOutputs;
    private ulong? _unlockedBalance;

    public MoneroSubaddress() { }

    public MoneroSubaddress(string address)
    {
        _address = address;
    }

    public MoneroSubaddress(uint accountIndex, uint index)
    {
        _accountIndex = accountIndex;
        _index = index;
    }

    public bool Equals(MoneroSubaddress? other)
    {
        if (other == null)
        {
            return false;
        }

        if (this == other)
        {
            return true;
        }

        return _accountIndex == other._accountIndex &&
               _index == other._index &&
               _address == other._address &&
               _label == other._label &&
               _balance == other._balance &&
               _unlockedBalance == other._unlockedBalance &&
               _numUnspentOutputs == other._numUnspentOutputs &&
               _isUsed == other._isUsed &&
               _numBlocksToUnlock == other._numBlocksToUnlock;
    }

    public uint? GetAccountIndex()
    {
        return _accountIndex;
    }

    public MoneroSubaddress SetAccountIndex(uint? accountIndex)
    {
        _accountIndex = accountIndex;
        return this;
    }

    public uint? GetIndex()
    {
        return _index;
    }

    public MoneroSubaddress SetIndex(uint? index)
    {
        _index = index;
        return this;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroSubaddress SetAddress(string? address)
    {
        _address = address;
        return this;
    }

    public string? GetLabel()
    {
        return _label;
    }

    public MoneroSubaddress SetLabel(string? label)
    {
        _label = label;
        return this;
    }

    public ulong? GetBalance()
    {
        return _balance;
    }

    public MoneroSubaddress SetBalance(ulong? balance)
    {
        _balance = balance;
        return this;
    }

    public ulong? GetUnlockedBalance()
    {
        return _unlockedBalance;
    }

    public MoneroSubaddress SetUnlockedBalance(ulong? unlockedBalance)
    {
        _unlockedBalance = unlockedBalance;
        return this;
    }

    public ulong? GetNumUnspentOutputs()
    {
        return _numUnspentOutputs;
    }

    public MoneroSubaddress SetNumUnspentOutputs(ulong? numUnspentOutputs)
    {
        _numUnspentOutputs = numUnspentOutputs;
        return this;
    }

    public bool? IsUsed()
    {
        return _isUsed;
    }

    public MoneroSubaddress SetIsUsed(bool? isUsed)
    {
        _isUsed = isUsed;
        return this;
    }

    public ulong? GetNumBlocksToUnlock()
    {
        return _numBlocksToUnlock;
    }

    public MoneroSubaddress SetNumBlocksToUnlock(ulong? numBlocksToUnlock)
    {
        _numBlocksToUnlock = numBlocksToUnlock;
        return this;
    }
}