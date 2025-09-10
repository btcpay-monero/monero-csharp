namespace Monero.Wallet.Common;

public class MoneroAccountTag
{
    private List<uint> _accountIndices = [];
    private string? _label;
    private string? _tag;

    public MoneroAccountTag(string? tag = null, string? label = null, List<uint>? accountIndices = null)
    {
        _tag = tag;
        _label = label;
        _accountIndices = accountIndices ?? [];
    }

    public string? GetTag()
    {
        return _tag;
    }

    public MoneroAccountTag SetTag(string? tag)
    {
        _tag = tag;
        return this;
    }

    public string? GetLabel()
    {
        return _label;
    }

    public MoneroAccountTag SetLabel(string? label = null)
    {
        _label = label;
        return this;
    }

    public List<uint> GetAccountIndices()
    {
        return _accountIndices;
    }

    public MoneroAccountTag SetAccountIndices(List<uint> accountIndices)
    {
        _accountIndices = accountIndices ?? [];
        return this;
    }
}