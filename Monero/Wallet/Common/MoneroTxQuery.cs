using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroTxQuery : MoneroTxWallet
{
    private List<string>? _hashes;
    private bool? _hasPaymentId;
    private ulong? _height;
    private bool? _includeOutputs;
    private MoneroOutputQuery? _inputQuery;
    private bool? _isIncoming;
    private bool? _isOutgoing;
    private ulong? _maxHeight;
    private ulong? _minHeight;
    private MoneroOutputQuery? _outputQuery;
    private List<string>? _paymentIds;
    private MoneroTransferQuery? _transferQuery;

    public MoneroTxQuery()
    {
    }

    public MoneroTxQuery(MoneroTxQuery query) : base(query)
    {
        _isOutgoing = query._isOutgoing;
        _isIncoming = query._isIncoming;
        if (query._hashes != null)
        {
            _hashes = query._hashes;
        }

        _hasPaymentId = query._hasPaymentId;
        if (query._paymentIds != null)
        {
            _paymentIds = query._paymentIds;
        }

        _height = query._height;
        _minHeight = query._minHeight;
        _maxHeight = query._maxHeight;
        _includeOutputs = query._includeOutputs;
        if (query._transferQuery != null)
        {
            SetTransferQuery(new MoneroTransferQuery(query._transferQuery));
        }

        if (query._inputQuery != null)
        {
            SetInputQuery(new MoneroOutputQuery(query._inputQuery));
        }

        if (query._outputQuery != null)
        {
            SetOutputQuery(new MoneroOutputQuery(query._outputQuery));
        }
    }

    public override MoneroTxQuery Clone()
    {
        return new MoneroTxQuery(this);
    }

    public override MoneroTxQuery SetIsLocked(bool? isLocked)
    {
        base.SetIsLocked(isLocked);
        return this;
    }

    public override bool? IsOutgoing()
    {
        return _isOutgoing;
    }

    public override MoneroTxQuery SetIsOutgoing(bool? isOutgoing)
    {
        this._isOutgoing = isOutgoing;
        return this;
    }

    public override bool? IsIncoming()
    {
        return _isIncoming;
    }

    public override MoneroTxQuery SetIsIncoming(bool? isIncoming)
    {
        this._isIncoming = isIncoming;
        return this;
    }

    public override MoneroTxQuery SetHash(string? hash)
    {
        base.SetHash(hash);

        if (hash != null)
        {
            SetHashes([hash]);
        }
        else
        {
            SetHashes(null);
        }

        return this;
    }

    public List<string>? GetHashes()
    {
        return _hashes;
    }

    public MoneroTxQuery SetHashes(List<string>? hashes)
    {
        this._hashes = hashes;
        return this;
    }

    public bool? HasPaymentId()
    {
        return _hasPaymentId;
    }

    public MoneroTxQuery SetHasPaymentId(bool? hasPaymentId)
    {
        this._hasPaymentId = hasPaymentId;
        return this;
    }

    public List<string>? GetPaymentIds()
    {
        return _paymentIds;
    }

    public MoneroTxQuery SetPaymentIds(List<string>? paymentIds)
    {
        this._paymentIds = paymentIds;
        return this;
    }

    public override MoneroTxQuery SetPaymentId(string? paymentId)
    {
        if (paymentId != null)
        {
            SetPaymentIds([paymentId]);
        }
        else
        {
            SetPaymentIds(null);
        }

        return this;
    }

    public override ulong? GetHeight()
    {
        return _height;
    }

    public MoneroTxQuery SetHeight(ulong? height)
    {
        this._height = height;
        return this;
    }

    public ulong? GetMinHeight()
    {
        return _minHeight;
    }

    public MoneroTxQuery SetMinHeight(ulong? minHeight)
    {
        this._minHeight = minHeight;
        return this;
    }

    public ulong? GetMaxHeight()
    {
        return _maxHeight;
    }

    public MoneroTxQuery SetMaxHeight(ulong? maxHeight)
    {
        this._maxHeight = maxHeight;
        return this;
    }

    public override MoneroTxQuery SetUnlockTime(ulong? unlockTime)
    {
        base.SetUnlockTime(unlockTime);
        return this;
    }

    public bool? GetIncludeOutputs()
    {
        return _includeOutputs;
    }

    public MoneroTxQuery SetIncludeOutputs(bool? includeOutputs)
    {
        this._includeOutputs = includeOutputs;
        return this;
    }

    public MoneroTransferQuery? GetTransferQuery()
    {
        return _transferQuery;
    }

    public MoneroTxQuery SetTransferQuery(MoneroTransferQuery? transferQuery)
    {
        this._transferQuery = transferQuery;
        if (transferQuery != null)
        {
            transferQuery.SetTxQuery(this);
        }

        return this;
    }

    public MoneroOutputQuery? GetInputQuery()
    {
        return _inputQuery;
    }

    public MoneroTxQuery SetInputQuery(MoneroOutputQuery? inputQuery)
    {
        this._inputQuery = inputQuery;
        if (inputQuery != null)
        {
            inputQuery.SetTxQuery(this);
        }

        return this;
    }

    public MoneroOutputQuery? GetOutputQuery()
    {
        return _outputQuery;
    }

    public MoneroTxQuery SetOutputQuery(MoneroOutputQuery? outputQuery)
    {
        this._outputQuery = outputQuery;
        if (outputQuery != null)
        {
            outputQuery.SetTxQuery(this);
        }

        return this;
    }

    public bool MeetsCriteria(MoneroTx? tx)
    {
        throw new NotImplementedException(
            "MoneroTxQuery.MeetsCriteria(MoneroTx tx) is not implemented yet. Please implement this method to filter transactions based on the query criteria.");
    }

    public bool MeetsCriteria(MoneroTxWallet? tx)
    {
        throw new NotImplementedException(
            "MoneroTxQuery.MeetsCriteria(MoneroTxWallet tx) is not implemented yet. Please implement this method to filter transactions based on the query criteria.");
    }
}