namespace Monero.Wallet.Common;

public class MoneroAccount
{
    private ulong balance;
    private uint? index;
    private string? primaryAddress;
    private List<MoneroSubaddress>? subaddresses;
    private string? tag;
    private ulong unlockedBalance;

    public MoneroAccount()
    {
        balance = 0;
        unlockedBalance = 0;
    }

    public MoneroAccount(uint? index)
    {
        this.index = index;
        balance = 0;
        unlockedBalance = 0;
    }

    public MoneroAccount(uint? index, string? primaryAddress)
    {
        this.index = index;
        this.primaryAddress = primaryAddress;
        balance = 0;
        unlockedBalance = 0;
    }

    public MoneroAccount(uint? index, string? primaryAddress, ulong? balance, ulong? unlockedBalance,
        List<MoneroSubaddress>? subaddresses)
    {
        this.index = index;
        this.primaryAddress = primaryAddress;
        this.balance = balance ?? 0;
        this.unlockedBalance = unlockedBalance ?? 0;
        this.subaddresses = subaddresses;
    }

    public uint? GetIndex()
    {
        return index;
    }

    public MoneroAccount SetIndex(uint? index)
    {
        this.index = index;
        return this;
    }

    public string? GetPrimaryAddress()
    {
        return primaryAddress;
    }

    public MoneroAccount SetPrimaryAddress(string? primaryAddress)
    {
        this.primaryAddress = primaryAddress;
        return this;
    }

    public ulong GetBalance()
    {
        return balance;
    }

    public MoneroAccount SetBalance(ulong balance)
    {
        this.balance = balance;
        return this;
    }

    public ulong GetUnlockedBalance()
    {
        return unlockedBalance;
    }

    public MoneroAccount SetUnlockedBalance(ulong unlockedBalance)
    {
        this.unlockedBalance = unlockedBalance;
        return this;
    }

    public string? GetTag()
    {
        return tag;
    }

    public MoneroAccount SetTag(string? tag)
    {
        this.tag = tag;
        return this;
    }

    public List<MoneroSubaddress>? GetSubaddresses()
    {
        return subaddresses;
    }

    public MoneroAccount SetSubaddresses(List<MoneroSubaddress>? subaddresses)
    {
        this.subaddresses = subaddresses;
        if (subaddresses != null)
        {
            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                subaddress.SetAccountIndex(index);
            }
        }

        return this;
    }
}