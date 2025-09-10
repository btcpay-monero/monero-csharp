namespace Monero.Common;

public class MoneroCheck
{
    protected bool isGood;

    public MoneroCheck(bool isGood = false)
    {
        this.isGood = isGood;
    }

    public virtual bool IsGood() { return isGood; }

    public virtual MoneroCheck SetIsGood(bool isGood)
    {
        this.isGood = isGood;
        return this;
    }
}