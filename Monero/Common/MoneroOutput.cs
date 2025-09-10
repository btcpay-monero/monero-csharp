namespace Monero.Common;

public class MoneroOutput
{
    private ulong? amount;
    private ulong? index;
    private MoneroKeyImage? keyImage;
    private List<ulong>? ringOutputIndices;
    private string? stealthPublicKey;
    private MoneroTx? tx;

    public MoneroOutput()
    {
        // nothing to build
    }

    public MoneroOutput(MoneroOutput output)
    {
        if (output.keyImage != null)
        {
            keyImage = output.keyImage.Clone();
        }

        amount = output.amount;
        index = output.index;
        if (output.ringOutputIndices != null)
        {
            ringOutputIndices = new List<ulong>(output.ringOutputIndices);
        }

        stealthPublicKey = output.stealthPublicKey;
    }

    public virtual MoneroOutput Clone()
    {
        return new MoneroOutput(this);
    }

    public virtual MoneroTx? GetTx()
    {
        return tx;
    }

    public virtual MoneroOutput SetTx(MoneroTx? tx)
    {
        this.tx = tx;
        return this;
    }

    public MoneroKeyImage? GetKeyImage()
    {
        return keyImage;
    }

    public MoneroOutput SetKeyImage(MoneroKeyImage? keyImage)
    {
        this.keyImage = keyImage;
        return this;
    }

    public ulong? GetAmount()
    {
        return amount;
    }

    public virtual MoneroOutput SetAmount(ulong? amount)
    {
        this.amount = amount;
        return this;
    }

    public ulong? GetIndex()
    {
        return index;
    }

    public virtual MoneroOutput SetIndex(ulong? index)
    {
        this.index = index;
        return this;
    }

    public List<ulong>? GetRingOutputIndices()
    {
        return ringOutputIndices;
    }

    public virtual MoneroOutput SetRingOutputIndices(List<ulong>? ringOutputIndices)
    {
        this.ringOutputIndices = ringOutputIndices;
        return this;
    }

    public string? GetStealthPublicKey()
    {
        return stealthPublicKey;
    }

    public virtual MoneroOutput SetStealthPublicKey(string? stealthPublicKey)
    {
        this.stealthPublicKey = stealthPublicKey;
        return this;
    }

    public MoneroOutput Merge(MoneroOutput output)
    {
        if (!(output is MoneroOutput))
        {
            throw new MoneroError("Cannot merge outputs of different types");
        }

        if (this == output)
        {
            return this;
        }

        // merge txs if they're different which comes back to merging outputs
        if (GetTx() != output.GetTx())
        {
            GetTx().Merge(output.GetTx());
        }

        // otherwise merge output fields
        else
        {
            if (GetKeyImage() == null)
            {
                SetKeyImage(output.GetKeyImage());
            }
            else if (output.GetKeyImage() != null)
            {
                GetKeyImage().Merge(output.GetKeyImage());
            }

            SetAmount(GenUtils.Reconcile(GetAmount(), output.GetAmount()));
            SetIndex(GenUtils.Reconcile(GetIndex(), output.GetIndex()));
        }

        return this;
    }
}