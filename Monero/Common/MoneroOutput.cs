using System.Text.Json.Serialization;

namespace Monero.Common;

public class MoneroOutput
{
    private ulong? _amount;
    private ulong? _index;
    private MoneroKeyImage? _keyImage;
    private List<ulong>? _ringOutputIndices;
    [JsonPropertyName("key")]
    public string? StealthPublicKey { get; set; }
    private MoneroTx? _tx;

    protected MoneroOutput(MoneroOutput output)
    {
        _amount = output._amount;
        _index = output._index;
        if (output._ringOutputIndices != null)
        {
            _ringOutputIndices = output._ringOutputIndices;
        }

        StealthPublicKey = output.StealthPublicKey;
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
        _tx = tx;
        return this;
    }

    public MoneroKeyImage? GetKeyImage()
    {
        return _keyImage;
    }

    public MoneroOutput SetKeyImage(MoneroKeyImage? keyImage)
    {
        _keyImage = keyImage;
        return this;
    }

    public ulong? GetAmount()
    {
        return _amount;
    }

    public virtual MoneroOutput SetAmount(ulong? amount)
    {
        _amount = amount;
        return this;
    }

    public ulong? GetIndex()
    {
        return _index;
    }

    public virtual MoneroOutput SetIndex(ulong? index)
    {
        _index = index;
        return this;
    }

    public List<ulong>? GetRingOutputIndices()
    {
        return _ringOutputIndices;
    }

    public virtual MoneroOutput SetRingOutputIndices(List<ulong>? ringOutputIndices)
    {
        _ringOutputIndices = ringOutputIndices;
        return this;
    }

    public string? GetStealthPublicKey()
    {
        return StealthPublicKey;
    }

    public virtual MoneroOutput SetStealthPublicKey(string? stealthPublicKey)
    {
        StealthPublicKey = stealthPublicKey;
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

        // merge txs if they're different, which comes back to merging outputs
        if (GetTx() == output.GetTx())
        {
            return this;
        }

        if (GetTx() == null)
        {
            throw new MoneroError("Cannot merge from null tx");
        }

        GetTx()!.Merge(output.GetTx());

        return this;
    }
}