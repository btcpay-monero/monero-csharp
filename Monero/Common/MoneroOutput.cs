
namespace Monero.Common;

public class MoneroOutput
{
    private MoneroTx? tx;
    private MoneroKeyImage? keyImage;
    private ulong? amount;
    private ulong? index;
    private List<ulong>? ringOutputIndices;
    private string? stealthPublicKey;

    public MoneroOutput()
    {
        // nothing to build
    }

    public MoneroOutput(MoneroOutput output)
    {
        if (output.keyImage != null) this.keyImage = output.keyImage.Clone();
        amount = output.amount;
        index = output.index;
        if (output.ringOutputIndices != null) ringOutputIndices = new List<ulong>(output.ringOutputIndices);
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
        if (!(output is MoneroOutput)) throw new MoneroError("Cannot merge outputs of different types");
        if (this == output) return this;

        // merge txs if they're different which comes back to merging outputs
        if (this.GetTx() != output.GetTx()) this.GetTx().Merge(output.GetTx());

        // otherwise merge output fields
        else
        {
            if (this.GetKeyImage() == null) this.SetKeyImage(output.GetKeyImage());
            else if (output.GetKeyImage() != null) this.GetKeyImage().Merge(output.GetKeyImage());
            this.SetAmount(GenUtils.Reconcile(this.GetAmount(), output.GetAmount()));
            this.SetIndex(GenUtils.Reconcile(this.GetIndex(), output.GetIndex()));
        }

        return this;
    }
}