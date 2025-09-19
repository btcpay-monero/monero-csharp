namespace Monero.Wallet.Common;

public class MoneroAccount
{
    private ulong _balance;
    private uint? _index;
    private string? _primaryAddress;
    private List<MoneroSubaddress>? _subaddresses;
    private string? _tag;
    private ulong _unlockedBalance;

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