namespace Monero.Wallet.Common;

public class MoneroKeyImageImportResult
{
    private ulong? _height;
    private ulong? _spentAmount;
    private ulong? _unspentAmount;

    public MoneroKeyImageImportResult()
    {
    }

    public MoneroKeyImageImportResult(ulong? height)
    {
        _height = height;
    }

    public MoneroKeyImageImportResult(ulong? height, ulong? spentAmount)
    {
        _height = height;
        _spentAmount = spentAmount;
    }

    public MoneroKeyImageImportResult(ulong? height, ulong? spentAmount, ulong? unspentAmount)
    {
        _height = height;
        _spentAmount = spentAmount;
        _unspentAmount = unspentAmount;
    }

    public MoneroKeyImageImportResult(MoneroKeyImageImportResult importResult)
    {
        _height = importResult._height;
        _spentAmount = importResult._spentAmount;
        _unspentAmount = importResult._unspentAmount;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public MoneroKeyImageImportResult SetHeight(ulong? height)
    {
        _height = height;
        return this;
    }

    public ulong? GetSpentAmount()
    {
        return _spentAmount;
    }

    public MoneroKeyImageImportResult SetSpentAmount(ulong? spentAmount)
    {
        _spentAmount = spentAmount;
        return this;
    }

    public ulong? GetUnspentAmount()
    {
        return _unspentAmount;
    }

    public MoneroKeyImageImportResult SetUnspentAmount(ulong? unspentAmount)
    {
        _unspentAmount = unspentAmount;
        return this;
    }

    public MoneroKeyImageImportResult Clone()
    {
        return new MoneroKeyImageImportResult(this);
    }
}