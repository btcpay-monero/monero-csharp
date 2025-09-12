namespace Monero.Common;

public class MoneroBlock : MoneroBlockHeader
{
    private string? _hex;
    private MoneroTx? _minerTx;
    private List<string>? _txHashes;
    private List<MoneroTx>? _txs;

    public MoneroBlock()
    {
    }

    public MoneroBlock(MoneroBlockHeader header) : base(header)
    {
        _hex = null;
        _minerTx = null;
        _txs = [];
        _txHashes = [];
    }

    public MoneroBlock(MoneroBlock block) : base(block)
    {
        _hex = block.GetHex();
        if (block._minerTx != null)
        {
            _minerTx = block._minerTx.Clone().SetBlock(this);
        }

        if (block._txs != null)
        {
            _txs = [];
            foreach (MoneroTx tx in block._txs)
            {
                _txs.Add(tx.Clone().SetBlock(this));
            }
        }

        if (block.GetTxHashes() != null)
        {
            _txHashes = new List<string>(block.GetTxHashes()!);
        }
    }

    public string? GetHex()
    {
        return _hex;
    }

    public MoneroBlock SetHex(string? hex)
    {
        this._hex = hex;
        return this;
    }

    public MoneroTx? GetMinerTx()
    {
        return _minerTx;
    }

    public MoneroBlock SetMinerTx(MoneroTx? minerTx)
    {
        this._minerTx = minerTx;
        return this;
    }

    public List<MoneroTx>? GetTxs()
    {
        return _txs;
    }

    public MoneroBlock SetTxs(List<MoneroTx>? txs)
    {
        this._txs = txs;
        if (txs != null)
        {
            foreach (MoneroTx tx in txs)
            {
                tx.SetBlock(this);
            }
        }

        return this;
    }

    public MoneroBlock SetTxs(MoneroTx? tx)
    {
        if (tx == null)
        {
            throw new ArgumentNullException(nameof(tx), "Transaction cannot be null");
        }

        return SetTxs([tx]);
    }

    public MoneroBlock AddTx(MoneroTx? tx)
    {
        if (tx == null)
        {
            throw new ArgumentNullException(nameof(tx), "Transaction cannot be null");
        }

        if (_txs == null)
        {
            _txs = [];
        }

        _txs.Add(tx.SetBlock(this));
        return this;
    }

    public List<string>? GetTxHashes()
    {
        return _txHashes;
    }

    public MoneroBlock SetTxHashes(List<string>? txHashes)
    {
        this._txHashes = txHashes;
        return this;
    }

    public MoneroBlock Clone()
    {
        return new MoneroBlock(this);
    }

    #region Override Base Methods

    public override MoneroBlock SetHash(string? hash)
    {
        base.SetHash(hash);
        return this;
    }

    public override MoneroBlock SetHeight(ulong? height)
    {
        base.SetHeight(height);
        return this;
    }

    public override MoneroBlock SetTimestamp(ulong? timestamp)
    {
        base.SetTimestamp(timestamp);
        return this;
    }

    public override MoneroBlock SetSize(ulong? size)
    {
        base.SetSize(size);
        return this;
    }

    public override MoneroBlock SetWeight(ulong? weight)
    {
        base.SetWeight(weight);
        return this;
    }

    public override MoneroBlock SetLongTermWeight(ulong? longTermWeight)
    {
        base.SetLongTermWeight(longTermWeight);
        return this;
    }

    public override MoneroBlock SetDepth(ulong? depth)
    {
        base.SetDepth(depth);
        return this;
    }

    public override MoneroBlock SetDifficulty(ulong? difficulty)
    {
        base.SetDifficulty(difficulty);
        return this;
    }

    public override MoneroBlock SetCumulativeDifficulty(ulong? cumulativeDifficulty)
    {
        base.SetCumulativeDifficulty(cumulativeDifficulty);
        return this;
    }

    public override MoneroBlock SetMajorVersion(uint? majorVersion)
    {
        base.SetMajorVersion(majorVersion);
        return this;
    }

    public override MoneroBlock SetMinorVersion(uint? minorVersion)
    {
        base.SetMinorVersion(minorVersion);
        return this;
    }

    public override MoneroBlock SetNonce(ulong? nonce)
    {
        base.SetNonce(nonce);
        return this;
    }

    public override MoneroBlock SetMinerTxHash(string? minerTxHash)
    {
        base.SetMinerTxHash(minerTxHash);
        return this;
    }

    public override MoneroBlock SetNumTxs(uint? numTxs)
    {
        base.SetNumTxs(numTxs);
        return this;
    }

    public override MoneroBlock SetOrphanStatus(bool? orphanStatus)
    {
        base.SetOrphanStatus(orphanStatus);
        return this;
    }

    public override MoneroBlock SetPrevHash(string? prevHash)
    {
        base.SetPrevHash(prevHash);
        return this;
    }

    public override MoneroBlock SetReward(ulong? reward)
    {
        base.SetReward(reward);
        return this;
    }

    public override MoneroBlock SetPowHash(string? powHash)
    {
        base.SetPowHash(powHash);
        return this;
    }

    #endregion
}