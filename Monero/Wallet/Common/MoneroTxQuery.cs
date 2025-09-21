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
        _isOutgoing = isOutgoing;
        return this;
    }

    public override bool? IsIncoming()
    {
        return _isIncoming;
    }

    public override MoneroTxQuery SetIsIncoming(bool? isIncoming)
    {
        _isIncoming = isIncoming;
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
        _hashes = hashes;
        return this;
    }

    public bool? HasPaymentId()
    {
        return _hasPaymentId;
    }

    public MoneroTxQuery SetHasPaymentId(bool? hasPaymentId)
    {
        _hasPaymentId = hasPaymentId;
        return this;
    }

    public List<string>? GetPaymentIds()
    {
        return _paymentIds;
    }

    public MoneroTxQuery SetPaymentIds(List<string>? paymentIds)
    {
        _paymentIds = paymentIds;
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
        _height = height;
        return this;
    }

    public ulong? GetMinHeight()
    {
        return _minHeight;
    }

    public MoneroTxQuery SetMinHeight(ulong? minHeight)
    {
        _minHeight = minHeight;
        return this;
    }

    public ulong? GetMaxHeight()
    {
        return _maxHeight;
    }

    public MoneroTxQuery SetMaxHeight(ulong? maxHeight)
    {
        _maxHeight = maxHeight;
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
        _includeOutputs = includeOutputs;
        return this;
    }

    public MoneroTransferQuery? GetTransferQuery()
    {
        return _transferQuery;
    }

    public MoneroTxQuery SetTransferQuery(MoneroTransferQuery? transferQuery)
    {
        _transferQuery = transferQuery;
        if (transferQuery != null)
        {
            transferQuery.SetTxQuery(this, false);
        }

        return this;
    }

    public MoneroOutputQuery? GetInputQuery()
    {
        return _inputQuery;
    }

    public MoneroTxQuery SetInputQuery(MoneroOutputQuery? inputQuery)
    {
        _inputQuery = inputQuery;
        if (inputQuery != null)
        {
            inputQuery.SetTxQuery(this, false);
        }

        return this;
    }

    public MoneroOutputQuery? GetOutputQuery()
    {
        return _outputQuery;
    }

    public MoneroTxQuery SetOutputQuery(MoneroOutputQuery? outputQuery)
    {
        _outputQuery = outputQuery;
        if (outputQuery != null)
        {
            outputQuery.SetTxQuery(this, false);
        }

        return this;
    }

    public bool MeetsCriteria(MoneroTxWallet? tx)
    {
        return MeetsCriteria(tx, true);
    }

    public bool MeetsCriteria(MoneroTxWallet? tx, bool queryChildren)
    {
        if (tx == null)
        {
            throw new MoneroError("No tx given to MoneroTxQuery.MeetsCriteria()");
        }

        // filter on tx
        if (GetHash() != null && !GetHash()!.Equals(tx.GetHash()))
        {
            return false;
        }

        if (GetPaymentId() != null && !GetPaymentId()!.Equals(tx.GetPaymentId()))
        {
            return false;
        }

        if (IsConfirmed() != null && IsConfirmed() != tx.IsConfirmed())
        {
            return false;
        }

        if (InTxPool() != null && InTxPool() != tx.InTxPool())
        {
            return false;
        }

        if (GetRelay() != null && GetRelay() != tx.GetRelay())
        {
            return false;
        }

        if (IsRelayed() != null && IsRelayed() != tx.IsRelayed())
        {
            return false;
        }

        if (IsFailed() != null && IsFailed() != tx.IsFailed())
        {
            return false;
        }

        if (IsMinerTx() != null && IsMinerTx() != tx.IsMinerTx())
        {
            return false;
        }

        if (IsLocked() != null && IsLocked() != tx.IsLocked())
        {
            return false;
        }

        // filter on having a payment id
        if (HasPaymentId() != null)
        {
            if (HasPaymentId() == true && tx.GetPaymentId() == null)
            {
                return false;
            }

            if (HasPaymentId() != true && tx.GetPaymentId() != null)
            {
                return false;
            }
        }

        // filter on incoming
        if (IsIncoming() != null && IsIncoming() != (tx.IsIncoming() == true))
        {
            return false;
        }

        // filter on outgoing
        if (IsOutgoing() != null && IsOutgoing() != (tx.IsOutgoing() == true))
        {
            return false;
        }

        // filter on remaining fields
        ulong? txHeight = tx.GetBlock() == null ? null : tx.GetBlock()!.GetHeight();
        if (GetHashes() != null && !GetHashes()!.Contains(tx.GetHash()!))
        {
            return false;
        }

        if (GetPaymentIds() != null && !GetPaymentIds()!.Contains(tx.GetPaymentId()!))
        {
            return false;
        }

        if (GetHeight() != null && !GetHeight().Equals(txHeight))
        {
            return false;
        }
        if (GetMinHeight() != null && txHeight != null && txHeight < GetMinHeight())
        {
            return false; // do not filter unconfirmed
        }

        if (GetMaxHeight() != null && (txHeight == null || txHeight > GetMaxHeight()))
        {
            return false;
        }

        // done if not querying transfers or outputs
        if (!queryChildren)
        {
            return true;
        }

        // at least one transfer must meet transfer query if defined
        if (GetTransferQuery() != null)
        {
            bool matchFound = false;
            if (tx.GetOutgoingTransfer() != null && GetTransferQuery()!.MeetsCriteria(tx.GetOutgoingTransfer(), false))
            {
                matchFound = true;
            }
            else if (tx.GetIncomingTransfers() != null)
            {
                foreach (MoneroIncomingTransfer incomingTransfer in tx.GetIncomingTransfers()!)
                {
                    if (GetTransferQuery()!.MeetsCriteria(incomingTransfer, false))
                    {
                        matchFound = true;
                        break;
                    }
                }
            }

            if (!matchFound)
            {
                return false;
            }
        }

        // at least one input must meet input query if defined
        if (GetInputQuery() != null)
        {
            if (tx.GetInputs() == null || tx.GetInputs()!.Count == 0)
            {
                return false;
            }
            bool matchFound = false;
            foreach (MoneroOutputWallet input in tx.GetInputsWallet())
            {
                if (GetInputQuery()!.MeetsCriteria(input))
                {
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
            {
                return false;
            }
        }

        // at least one output must meet output query if defined
        if (GetOutputQuery() != null)
        {
            if (tx.GetOutputs() == null || tx.GetOutputs()!.Count == 0)
            {
                return false;
            }
            bool matchFound = false;
            foreach (MoneroOutputWallet output in tx.GetOutputsWallet())
            {
                if (GetOutputQuery()!.MeetsCriteria(output))
                {
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
            {
                return false;
            }
        }

        return true;  // transaction meets query criteria
    }
}