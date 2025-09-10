namespace Monero.Common;

public class MoneroTx
{
    public static readonly string DEFAULT_PAYMENT_ID = "0000000000000000";

    private MoneroBlock? block;
    private byte[]? extra; // TODO: switch to string for consistency with MoneroTxWallet?
    private ulong? fee;
    private string? fullHex;
    private string? hash;
    private List<MoneroOutput>? inputs;
    private bool? inTxPool;
    private bool? isConfirmed;
    private bool? isDoubleSpendSeen;
    private bool? isFailed;
    private bool? isKeptByBlock;
    private bool? isMinerTx;
    private bool? isRelayed;
    private string? key;
    private string? lastFailedHash;
    private ulong? lastFailedHeight;
    private ulong? lastRelayedTimestamp;
    private string? maxUsedBlockHash;
    private ulong? maxUsedBlockHeight;
    private string? metadata;
    private ulong? numConfirmations;
    private List<ulong>? outputIndices;
    private List<MoneroOutput>? outputs;
    private string? paymentId;
    private string? prunableHash;
    private string? prunableHex;
    private string? prunedHex;
    private object? rctSignatures; // TODO: implement
    private object? rctSigPrunable; // TODO: implement
    private ulong? receivedTimestamp;
    private bool? relay;
    private uint? ringSize;
    private List<string>? signatures;
    private ulong? size;
    private ulong? unlockTime;
    private uint? version;
    private ulong? weight;

    public MoneroTx()
    {
        // nothing to build
    }

    public MoneroTx(MoneroTx tx)
    {
        hash = tx.hash;
        version = tx.version;
        isMinerTx = tx.isMinerTx;
        paymentId = tx.paymentId;
        fee = tx.fee;
        ringSize = tx.ringSize;
        relay = tx.relay;
        isRelayed = tx.isRelayed;
        isConfirmed = tx.isConfirmed;
        inTxPool = tx.inTxPool;
        numConfirmations = tx.numConfirmations;
        unlockTime = tx.unlockTime;
        lastRelayedTimestamp = tx.lastRelayedTimestamp;
        receivedTimestamp = tx.receivedTimestamp;
        isDoubleSpendSeen = tx.isDoubleSpendSeen;
        key = tx.key;
        fullHex = tx.fullHex;
        prunedHex = tx.prunedHex;
        prunableHex = tx.prunableHex;
        prunableHash = tx.prunableHash;
        size = tx.size;
        weight = tx.weight;
        if (tx.inputs != null)
        {
            inputs = [];
            foreach (MoneroOutput input in tx.inputs)
            {
                inputs.Add(input.Clone().SetTx(this));
            }
        }

        if (tx.outputs != null)
        {
            outputs = [];
            foreach (MoneroOutput output in tx.outputs)
            {
                outputs.Add(output.Clone().SetTx(this));
            }
        }

        if (tx.outputIndices != null)
        {
            outputIndices = new List<ulong>(tx.outputIndices);
        }

        metadata = tx.metadata;
        if (tx.extra != null)
        {
            extra = tx.extra;
        }

        rctSignatures = tx.rctSignatures;
        rctSigPrunable = tx.rctSigPrunable;
        isKeptByBlock = tx.isKeptByBlock;
        isFailed = tx.isFailed;
        lastFailedHeight = tx.lastFailedHeight;
        lastFailedHash = tx.lastFailedHash;
        maxUsedBlockHeight = tx.maxUsedBlockHeight;
        maxUsedBlockHash = tx.maxUsedBlockHash;
        if (tx.signatures != null)
        {
            signatures = new List<string>(tx.signatures);
        }
    }

    public virtual MoneroTx Clone()
    {
        return new MoneroTx(this);
    }

    public MoneroBlock? GetBlock()
    {
        return block;
    }

    public virtual MoneroTx SetBlock(MoneroBlock? block)
    {
        this.block = block;
        return this;
    }

    public virtual ulong? GetHeight()
    {
        return GetBlock()?.GetHeight();
    }

    public virtual string? GetHash()
    {
        return hash;
    }

    public virtual MoneroTx SetHash(string? hash)
    {
        this.hash = hash;
        return this;
    }

    public virtual uint? GetVersion()
    {
        return version;
    }

    public virtual MoneroTx SetVersion(uint? version)
    {
        this.version = version;
        return this;
    }

    public bool? IsMinerTx()
    {
        return isMinerTx;
    }

    public virtual MoneroTx SetIsMinerTx(bool? IsMinerTx)
    {
        isMinerTx = IsMinerTx;
        return this;
    }

    public string? GetPaymentId()
    {
        return paymentId;
    }

    public virtual MoneroTx SetPaymentId(string? paymentId)
    {
        this.paymentId = paymentId;
        return this;
    }

    public ulong? GetFee()
    {
        return fee;
    }

    public virtual MoneroTx SetFee(ulong? fee)
    {
        this.fee = fee;
        return this;
    }

    public uint? GetRingSize()
    {
        return ringSize;
    }

    public virtual MoneroTx SetRingSize(uint? ringSize)
    {
        this.ringSize = ringSize;
        return this;
    }

    public bool? GetRelay()
    {
        return relay;
    }

    public virtual MoneroTx SetRelay(bool? relay)
    {
        this.relay = relay;
        return this;
    }

    public bool? IsRelayed()
    {
        return isRelayed;
    }

    public virtual MoneroTx SetIsRelayed(bool? IsRelayed)
    {
        isRelayed = IsRelayed;
        return this;
    }

    public bool? IsConfirmed()
    {
        return isConfirmed;
    }

    public virtual MoneroTx SetIsConfirmed(bool? IsConfirmed)
    {
        isConfirmed = IsConfirmed;
        return this;
    }

    public bool? InTxPool()
    {
        return inTxPool;
    }

    public virtual MoneroTx SetInTxPool(bool? inTxPool)
    {
        this.inTxPool = inTxPool;
        return this;
    }

    public ulong? GetNumConfirmations()
    {
        return numConfirmations;
    }

    public virtual MoneroTx SetNumConfirmations(ulong? numConfirmations)
    {
        this.numConfirmations = numConfirmations;
        return this;
    }

    public ulong? GetUnlockTime()
    {
        return unlockTime;
    }

    public virtual MoneroTx SetUnlockTime(ulong? unlockTime)
    {
        this.unlockTime = unlockTime;
        return this;
    }

    public ulong? GetLastRelayedTimestamp()
    {
        return lastRelayedTimestamp;
    }

    public virtual MoneroTx SetLastRelayedTimestamp(ulong? lastRelayedTimestamp)
    {
        this.lastRelayedTimestamp = lastRelayedTimestamp;
        return this;
    }

    public ulong? GetReceivedTimestamp()
    {
        return receivedTimestamp;
    }

    public virtual MoneroTx SetReceivedTimestamp(ulong? receivedTimestamp)
    {
        this.receivedTimestamp = receivedTimestamp;
        return this;
    }

    public bool? IsDoubleSpendSeen()
    {
        return isDoubleSpendSeen;
    }

    public virtual MoneroTx SetIsDoubleSpendSeen(bool? IsDoubleSpend)
    {
        isDoubleSpendSeen = IsDoubleSpend;
        return this;
    }

    public string? GetKey()
    {
        return key;
    }

    public virtual MoneroTx SetKey(string? key)
    {
        this.key = key;
        return this;
    }

    public string? GetFullHex()
    {
        return fullHex;
    }

    public virtual MoneroTx SetFullHex(string? fullHex)
    {
        this.fullHex = fullHex;
        return this;
    }

    public string? GetPrunedHex()
    {
        return prunedHex;
    }

    public virtual MoneroTx SetPrunedHex(string? prunedHex)
    {
        this.prunedHex = prunedHex;
        return this;
    }

    public string? GetPrunableHex()
    {
        return prunableHex;
    }

    public virtual MoneroTx SetPrunableHex(string? prunableHex)
    {
        this.prunableHex = prunableHex;
        return this;
    }

    public string? GetPrunableHash()
    {
        return prunableHash;
    }

    public virtual MoneroTx SetPrunableHash(string? prunableHash)
    {
        this.prunableHash = prunableHash;
        return this;
    }

    public ulong? GetSize()
    {
        return size;
    }

    public virtual MoneroTx SetSize(ulong? size)
    {
        this.size = size;
        return this;
    }

    public ulong? GetWeight()
    {
        return weight;
    }

    public virtual MoneroTx SetWeight(ulong? weight)
    {
        this.weight = weight;
        return this;
    }

    public List<MoneroOutput>? GetInputs()
    {
        return inputs;
    }

    public virtual MoneroTx SetInputs(List<MoneroOutput>? inputs)
    {
        this.inputs = inputs;
        return this;
    }

    public List<MoneroOutput>? GetOutputs()
    {
        return outputs;
    }

    public virtual MoneroTx SetOutputs(List<MoneroOutput>? outputs)
    {
        this.outputs = outputs;
        return this;
    }

    public List<ulong>? GetOutputIndices()
    {
        return outputIndices;
    }

    public virtual MoneroTx SetOutputIndices(List<ulong>? outputIndices)
    {
        this.outputIndices = outputIndices;
        return this;
    }

    public string? GetMetadata()
    {
        return metadata;
    }

    public virtual MoneroTx SetMetadata(string? metadata)
    {
        this.metadata = metadata;
        return this;
    }

    public byte[]? GetExtra()
    {
        return extra;
    }

    public virtual MoneroTx SetExtra(byte[]? extra)
    {
        this.extra = extra;
        return this;
    }

    public object? GetRctSignatures()
    {
        return rctSignatures;
    }

    public virtual MoneroTx SetRctSignatures(object? rctSignatures)
    {
        this.rctSignatures = rctSignatures;
        return this;
    }

    public object? GetRctSigPrunable()
    {
        return rctSigPrunable;
    }

    public virtual MoneroTx SetRctSigPrunable(object? rctSigPrunable)
    {
        this.rctSigPrunable = rctSigPrunable;
        return this;
    }

    public bool? IsKeptByBlock()
    {
        return isKeptByBlock;
    }

    public virtual MoneroTx SetIsKeptByBlock(bool? IsKeptByBlock)
    {
        isKeptByBlock = IsKeptByBlock;
        return this;
    }

    public bool? IsFailed()
    {
        return isFailed;
    }

    public virtual MoneroTx SetIsFailed(bool? IsFailed)
    {
        isFailed = IsFailed;
        return this;
    }

    public ulong? GetLastFailedHeight()
    {
        return lastFailedHeight;
    }

    public virtual MoneroTx SetLastFailedHeight(ulong? lastFailedHeight)
    {
        this.lastFailedHeight = lastFailedHeight;
        return this;
    }

    public string? GetLastFailedHash()
    {
        return lastFailedHash;
    }

    public virtual MoneroTx SetLastFailedHash(string? lastFailedHash)
    {
        this.lastFailedHash = lastFailedHash;
        return this;
    }

    public ulong? GetMaxUsedBlockHeight()
    {
        return maxUsedBlockHeight;
    }

    public virtual MoneroTx SetMaxUsedBlockHeight(ulong? maxUsedBlockHeight)
    {
        this.maxUsedBlockHeight = maxUsedBlockHeight;
        return this;
    }

    public string? GetMaxUsedBlockHash()
    {
        return maxUsedBlockHash;
    }

    public virtual MoneroTx SetMaxUsedBlockHash(string? maxUsedBlockHash)
    {
        this.maxUsedBlockHash = maxUsedBlockHash;
        return this;
    }

    public List<string>? GetSignatures()
    {
        return signatures;
    }

    public virtual MoneroTx SetSignatures(List<string>? signatures)
    {
        this.signatures = signatures;
        return this;
    }

    public MoneroTx Merge(MoneroTx? tx)
    {
        if (tx == null)
        {
            throw new MoneroError("Cannot merge null tx");
        }

        if (this == tx)
        {
            return this;
        }

        // merge blocks if they're different
        if (GetBlock() != tx.GetBlock())
        {
            if (GetBlock() == null)
            {
                SetBlock(tx.GetBlock());
                //this.GetBlock().GetTxs().Set(this.GetBlock().GetTxs().IndexOf(tx), this); // update block to point to this tx
            }
            else if (tx.GetBlock() != null)
            {
                GetBlock().Merge(tx.GetBlock()); // comes back to merging txs
                return this;
            }
        }

        // otherwise merge tx fields
        SetHash(GenUtils.Reconcile(GetHash(), tx.GetHash()));
        SetVersion(GenUtils.Reconcile(GetVersion(), tx.GetVersion()));
        SetPaymentId(GenUtils.Reconcile(GetPaymentId(), tx.GetPaymentId()));
        SetFee(GenUtils.Reconcile(GetFee(), tx.GetFee()));
        SetRingSize(GenUtils.Reconcile(GetRingSize(), tx.GetRingSize()));
        SetIsConfirmed(GenUtils.Reconcile(IsConfirmed(), tx.IsConfirmed(), null, true)); // tx can become confirmed
        SetIsMinerTx(GenUtils.Reconcile(IsMinerTx(), tx.IsMinerTx()));
        SetRelay(GenUtils.Reconcile(GetRelay(), tx.GetRelay(), null, true)); // tx can become relayed
        SetIsRelayed(GenUtils.Reconcile(IsRelayed(), tx.IsRelayed(), null, true)); // tx can become relayed
        SetIsDoubleSpendSeen(
            GenUtils.Reconcile(IsDoubleSpendSeen(), tx.IsDoubleSpendSeen(), null,
                true)); // double spend can become seen
        SetKey(GenUtils.Reconcile(GetKey(), tx.GetKey()));
        SetFullHex(GenUtils.Reconcile(GetFullHex(), tx.GetFullHex()));
        SetPrunedHex(GenUtils.Reconcile(GetPrunedHex(), tx.GetPrunedHex()));
        SetPrunableHex(GenUtils.Reconcile(GetPrunableHex(), tx.GetPrunableHex()));
        SetPrunableHash(GenUtils.Reconcile(GetPrunableHash(), tx.GetPrunableHash()));
        SetSize(GenUtils.Reconcile(GetSize(), tx.GetSize()));
        SetWeight(GenUtils.Reconcile(GetWeight(), tx.GetWeight()));
        SetOutputIndices(GenUtils.Reconcile(GetOutputIndices(), tx.GetOutputIndices()));
        SetMetadata(GenUtils.Reconcile(GetMetadata(), tx.GetMetadata()));
        SetExtra(GenUtils.ReconcileByteArrays(GetExtra(), tx.GetExtra()));
        SetRctSignatures(GenUtils.Reconcile(GetRctSignatures(), tx.GetRctSignatures()));
        SetRctSigPrunable(GenUtils.Reconcile(GetRctSigPrunable(), tx.GetRctSigPrunable()));
        SetIsKeptByBlock(GenUtils.Reconcile(IsKeptByBlock(), tx.IsKeptByBlock()));
        SetIsFailed(GenUtils.Reconcile(IsFailed(), tx.IsFailed(), null, true));
        SetLastFailedHeight(GenUtils.Reconcile(GetLastFailedHeight(), tx.GetLastFailedHeight()));
        SetLastFailedHash(GenUtils.Reconcile(GetLastFailedHash(), tx.GetLastFailedHash()));
        SetMaxUsedBlockHeight(GenUtils.Reconcile(GetMaxUsedBlockHeight(), tx.GetMaxUsedBlockHeight()));
        SetMaxUsedBlockHash(GenUtils.Reconcile(GetMaxUsedBlockHash(), tx.GetMaxUsedBlockHash()));
        SetSignatures(GenUtils.Reconcile(GetSignatures(), tx.GetSignatures()));
        SetUnlockTime(GenUtils.Reconcile(GetUnlockTime(), tx.GetUnlockTime()));
        SetNumConfirmations(GenUtils.Reconcile(GetNumConfirmations(), tx.GetNumConfirmations(), null, null,
            true)); // num confirmations can increase

        // merge inputs
        if (tx.GetInputs() != null)
        {
            foreach (MoneroOutput merger in tx.GetInputs())
            {
                bool merged = false;
                merger.SetTx(this);
                if (GetInputs() == null)
                {
                    SetInputs([]);
                }

                foreach (MoneroOutput mergee in GetInputs())
                {
                    if (mergee.GetKeyImage().GetHex().Equals(merger.GetKeyImage().GetHex()))
                    {
                        mergee.Merge(merger);
                        merged = true;
                        break;
                    }
                }

                if (!merged)
                {
                    GetInputs().Add(merger);
                }
            }
        }

        // merge outputs
        if (tx.GetOutputs() != null)
        {
            foreach (MoneroOutput output in tx.GetOutputs())
            {
                output.SetTx(this);
            }

            if (GetOutputs() == null)
            {
                SetOutputs(tx.GetOutputs());
            }
            else
            {
                // merge outputs if key image or stealth public key present, otherwise append
                foreach (MoneroOutput merger in tx.GetOutputs())
                {
                    bool merged = false;
                    merger.SetTx(this);
                    foreach (MoneroOutput mergee in GetOutputs())
                    {
                        if ((merger.GetKeyImage() != null &&
                             mergee.GetKeyImage().GetHex().Equals(merger.GetKeyImage().GetHex())) ||
                            (merger.GetStealthPublicKey() != null &&
                             mergee.GetStealthPublicKey().Equals(merger.GetStealthPublicKey())))
                        {
                            mergee.Merge(merger);
                            merged = true;
                            break;
                        }
                    }

                    if (!merged)
                    {
                        GetOutputs().Add(merger); // append output
                    }
                }
            }
        }

        // handle unrelayed -> relayed -> confirmed
        if (IsConfirmed() == true)
        {
            SetInTxPool(false);
            SetReceivedTimestamp(null);
            SetLastRelayedTimestamp(null);
        }
        else
        {
            SetInTxPool(GenUtils.Reconcile(InTxPool(), tx.InTxPool(), null, true)); // unrelayed -> tx pool
            SetReceivedTimestamp(GenUtils.Reconcile(GetReceivedTimestamp(), tx.GetReceivedTimestamp(), null,
                null, false)); // take earliest receive time
            SetLastRelayedTimestamp(GenUtils.Reconcile(GetLastRelayedTimestamp(),
                tx.GetLastRelayedTimestamp(), null, null, true)); // take latest relay time
        }

        return this; // for chaining
    }
}