namespace Monero.Common;

public class MoneroOutput
{
    private ulong? _amount;
    private ulong? _index;
    private MoneroKeyImage? _keyImage;
    private List<ulong>? _ringOutputIndices;
    private string? _stealthPublicKey;
    private MoneroTx? _tx;

    public MoneroOutput()
    {
        // nothing to build
    }

    public MoneroOutput(MoneroOutput output)
    {
        if (output._keyImage != null)
        {
            _keyImage = output._keyImage.Clone();
        }

        _amount = output._amount;
        _index = output._index;
        if (output._ringOutputIndices != null)
        {
            _ringOutputIndices = new List<ulong>(output._ringOutputIndices);
        }

        _stealthPublicKey = output._stealthPublicKey;
    }

    public virtual MoneroOutput Clone()
    {
        return new MoneroOutput(this);
    }

    public virtual MoneroTx? GetTx()
    {
        return _tx;
    }

    public virtual MoneroOutput SetTx(MoneroTx? tx)
    {
        this._tx = tx;
        return this;
    }

    public MoneroKeyImage? GetKeyImage()
    {
        return _keyImage;
    }

    public MoneroOutput SetKeyImage(MoneroKeyImage? keyImage)
    {
        this._keyImage = keyImage;
        return this;
    }

    public ulong? GetAmount()
    {
        return _amount;
    }

    public virtual MoneroOutput SetAmount(ulong? amount)
    {
        this._amount = amount;
        return this;
    }

    public ulong? GetIndex()
    {
        return _index;
    }

    public virtual MoneroOutput SetIndex(ulong? index)
    {
        this._index = index;
        return this;
    }

    public List<ulong>? GetRingOutputIndices()
    {
        return _ringOutputIndices;
    }

    public virtual MoneroOutput SetRingOutputIndices(List<ulong>? ringOutputIndices)
    {
        this._ringOutputIndices = ringOutputIndices;
        return this;
    }

    public string? GetStealthPublicKey()
    {
        return _stealthPublicKey;
    }

    public virtual MoneroOutput SetStealthPublicKey(string? stealthPublicKey)
    {
        this._stealthPublicKey = stealthPublicKey;
        return this;
    }

    public MoneroOutput Merge(MoneroOutput? output)
    {
        if (output == null)
        {
            throw new MoneroError("Cannot merge null output");
        }

        if (this == output)
        {
            return this;
        }

        // merge txs if they're different which comes back to merging outputs
        if (GetTx() != output.GetTx())
        {
            if (GetTx() == null)
            {
                throw new MoneroError("Cannot merge from null tx");
            }

            GetTx()!.Merge(output.GetTx());
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
                GetKeyImage()!.Merge(output.GetKeyImage());
            }

            SetAmount(GenUtils.Reconcile(GetAmount(), output.GetAmount()));
            SetIndex(GenUtils.Reconcile(GetIndex(), output.GetIndex()));
        }

        return this;
    }
}