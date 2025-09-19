namespace Monero.Common;

public class MoneroCheck
{
    protected bool _isGood;

    public MoneroCheck()
    {
        _isGood = false;
    }

    public MoneroCheck(bool isGood)
    {
        _isGood = isGood;
    }

    public virtual bool IsGood() { return _isGood; }

    public virtual MoneroCheck SetIsGood(bool isGood)
    {
        _isGood = isGood;
        return this;
    }
}