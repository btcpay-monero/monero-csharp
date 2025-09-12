namespace Monero.Common;

public class MoneroCheck
{
    protected bool _isGood;

    public MoneroCheck(bool isGood = false)
    {
        this._isGood = isGood;
    }

    public virtual bool IsGood() { return _isGood; }

    public virtual MoneroCheck SetIsGood(bool isGood)
    {
        this._isGood = isGood;
        return this;
    }
}