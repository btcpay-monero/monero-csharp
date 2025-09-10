
namespace Monero.Common;

public class MoneroTx
{
    public static readonly string DEFAULT_PAYMENT_ID = "0000000000000000";

    private MoneroBlock? block;
    private string? hash;
    private uint? version;
    private bool? isMinerTx;
    private string? paymentId;
    private ulong? fee;
    private uint? ringSize;
    private bool? relay;
    private bool? isRelayed;
    private bool? isConfirmed;
    private bool? inTxPool;
    private ulong? numConfirmations;
    private ulong? unlockTime;
    private ulong? lastRelayedTimestamp;
    private ulong? receivedTimestamp;
    private bool? isDoubleSpendSeen;
    private string? key;
    private string? fullHex;
    private string? prunedHex;
    private string? prunableHex;
    private string? prunableHash;
    private ulong? size;
    private ulong? weight;
    private List<MoneroOutput>? inputs;
    private List<MoneroOutput>? outputs;
    private List<ulong>? outputIndices;
    private string? metadata;
    private byte[]? extra; // TODO: switch to string for consistency with MoneroTxWallet?
    private object? rctSignatures; // TODO: implement
    private object? rctSigPrunable;  // TODO: implement
    private bool? isKeptByBlock;
    private bool? isFailed;
    private ulong? lastFailedHeight;
    private string? lastFailedHash;
    private ulong? maxUsedBlockHeight;
    private string? maxUsedBlockHash;
    private List<string>? signatures;

    public MoneroTx()
    {
        // nothing to build
    }

    public MoneroTx(MoneroTx tx)
    {
        this.hash = tx.hash;
        this.version = tx.version;
        this.isMinerTx = tx.isMinerTx;
        this.paymentId = tx.paymentId;
        this.fee = tx.fee;
        this.ringSize = tx.ringSize;
        this.relay = tx.relay;
        this.isRelayed = tx.isRelayed;
        this.isConfirmed = tx.isConfirmed;
        this.inTxPool = tx.inTxPool;
        this.numConfirmations = tx.numConfirmations;
        this.unlockTime = tx.unlockTime;
        this.lastRelayedTimestamp = tx.lastRelayedTimestamp;
        this.receivedTimestamp = tx.receivedTimestamp;
        this.isDoubleSpendSeen = tx.isDoubleSpendSeen;
        this.key = tx.key;
        this.fullHex = tx.fullHex;
        this.prunedHex = tx.prunedHex;
        this.prunableHex = tx.prunableHex;
        this.prunableHash = tx.prunableHash;
        this.size = tx.size;
        this.weight = tx.weight;
        if (tx.inputs != null)
        {
            this.inputs = new List<MoneroOutput>();
            foreach (MoneroOutput input in tx.inputs) inputs.Add(input.Clone().SetTx(this));
        }
        if (tx.outputs != null)
        {
            this.outputs = new List<MoneroOutput>();
            foreach (MoneroOutput output in tx.outputs) outputs.Add(output.Clone().SetTx(this));
        }
        if (tx.outputIndices != null) this.outputIndices = new List<ulong>(tx.outputIndices);
        this.metadata = tx.metadata;
        if (tx.extra != null) this.extra = tx.extra;
        this.rctSignatures = tx.rctSignatures;
        this.rctSigPrunable = tx.rctSigPrunable;
        this.isKeptByBlock = tx.isKeptByBlock;
        this.isFailed = tx.isFailed;
        this.lastFailedHeight = tx.lastFailedHeight;
        this.lastFailedHash = tx.lastFailedHash;
        this.maxUsedBlockHeight = tx.maxUsedBlockHeight;
        this.maxUsedBlockHash = tx.maxUsedBlockHash;
        if (tx.signatures != null) this.signatures = new List<string>(tx.signatures);
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
        this.isMinerTx = IsMinerTx;
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
        this.isConfirmed = IsConfirmed;
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
        this.isDoubleSpendSeen = IsDoubleSpend;
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

    public List<MoneroOutput> GetInputs()
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
        this.isFailed = IsFailed;
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
        if (this == tx) return this;

        // merge blocks if they're different
        if (this.GetBlock() != tx.GetBlock())
        {
            if (this.GetBlock() == null)
            {
                this.SetBlock(tx.GetBlock());
                //this.GetBlock().GetTxs().Set(this.GetBlock().GetTxs().IndexOf(tx), this); // update block to point to this tx
            }
            else if (tx.GetBlock() != null)
            {
                this.GetBlock().Merge(tx.GetBlock()); // comes back to merging txs
                return this;
            }
        }

        // otherwise merge tx fields
        this.SetHash(GenUtils.Reconcile(this.GetHash(), tx.GetHash()));
        this.SetVersion(GenUtils.Reconcile(this.GetVersion(), tx.GetVersion()));
        this.SetPaymentId(GenUtils.Reconcile(this.GetPaymentId(), tx.GetPaymentId()));
        this.SetFee(GenUtils.Reconcile(this.GetFee(), tx.GetFee()));
        this.SetRingSize(GenUtils.Reconcile(this.GetRingSize(), tx.GetRingSize()));
        this.SetIsConfirmed(GenUtils.Reconcile(this.IsConfirmed(), tx.IsConfirmed(), null, true, null));  // tx can become confirmed
        this.SetIsMinerTx(GenUtils.Reconcile(this.IsMinerTx(), tx.IsMinerTx(), null, null, null));
        this.SetRelay(GenUtils.Reconcile(this.GetRelay(), tx.GetRelay(), null, true, null));        // tx can become relayed
        this.SetIsRelayed(GenUtils.Reconcile(this.IsRelayed(), tx.IsRelayed(), null, true, null));  // tx can become relayed
        this.SetIsDoubleSpendSeen(GenUtils.Reconcile(this.IsDoubleSpendSeen(), tx.IsDoubleSpendSeen(), null, true, null)); // double spend can become seen
        this.SetKey(GenUtils.Reconcile(this.GetKey(), tx.GetKey()));
        this.SetFullHex(GenUtils.Reconcile(this.GetFullHex(), tx.GetFullHex()));
        this.SetPrunedHex(GenUtils.Reconcile(this.GetPrunedHex(), tx.GetPrunedHex()));
        this.SetPrunableHex(GenUtils.Reconcile(this.GetPrunableHex(), tx.GetPrunableHex()));
        this.SetPrunableHash(GenUtils.Reconcile(this.GetPrunableHash(), tx.GetPrunableHash()));
        this.SetSize(GenUtils.Reconcile(this.GetSize(), tx.GetSize()));
        this.SetWeight(GenUtils.Reconcile(this.GetWeight(), tx.GetWeight()));
        this.SetOutputIndices(GenUtils.Reconcile(this.GetOutputIndices(), tx.GetOutputIndices()));
        this.SetMetadata(GenUtils.Reconcile(this.GetMetadata(), tx.GetMetadata()));
        this.SetExtra(GenUtils.ReconcileByteArrays(this.GetExtra(), tx.GetExtra()));
        this.SetRctSignatures(GenUtils.Reconcile(this.GetRctSignatures(), tx.GetRctSignatures()));
        this.SetRctSigPrunable(GenUtils.Reconcile(this.GetRctSigPrunable(), tx.GetRctSigPrunable()));
        this.SetIsKeptByBlock(GenUtils.Reconcile(this.IsKeptByBlock(), tx.IsKeptByBlock()));
        this.SetIsFailed(GenUtils.Reconcile(this.IsFailed(), tx.IsFailed(), null, true, null));
        this.SetLastFailedHeight(GenUtils.Reconcile(this.GetLastFailedHeight(), tx.GetLastFailedHeight()));
        this.SetLastFailedHash(GenUtils.Reconcile(this.GetLastFailedHash(), tx.GetLastFailedHash()));
        this.SetMaxUsedBlockHeight(GenUtils.Reconcile(this.GetMaxUsedBlockHeight(), tx.GetMaxUsedBlockHeight()));
        this.SetMaxUsedBlockHash(GenUtils.Reconcile(this.GetMaxUsedBlockHash(), tx.GetMaxUsedBlockHash()));
        this.SetSignatures(GenUtils.Reconcile(this.GetSignatures(), tx.GetSignatures()));
        this.SetUnlockTime(GenUtils.Reconcile(this.GetUnlockTime(), tx.GetUnlockTime()));
        this.SetNumConfirmations(GenUtils.Reconcile(this.GetNumConfirmations(), tx.GetNumConfirmations(), null, null, true)); // num confirmations can increase

        // merge inputs
        if (tx.GetInputs() != null)
        {
            foreach (MoneroOutput merger in tx.GetInputs())
            {
                bool merged = false;
                merger.SetTx(this);
                if (this.GetInputs() == null) this.SetInputs([]);
                foreach (MoneroOutput mergee in this.GetInputs())
                {
                    if (mergee.GetKeyImage().GetHex().Equals(merger.GetKeyImage().GetHex()))
                    {
                        mergee.Merge(merger);
                        merged = true;
                        break;
                    }
                }
                if (!merged) this.GetInputs().Add(merger);
            }
        }

        // merge outputs
        if (tx.GetOutputs() != null)
        {
            foreach (MoneroOutput output in tx.GetOutputs()) output.SetTx(this);
            if (this.GetOutputs() == null) this.SetOutputs(tx.GetOutputs());
            else
            {

                // merge outputs if key image or stealth public key present, otherwise append
                foreach (MoneroOutput merger in tx.GetOutputs())
                {
                    bool merged = false;
                    merger.SetTx(this);
                    foreach (MoneroOutput mergee in this.GetOutputs())
                    {
                        if ((merger.GetKeyImage() != null && mergee.GetKeyImage().GetHex().Equals(merger.GetKeyImage().GetHex())) ||
                            (merger.GetStealthPublicKey() != null && mergee.GetStealthPublicKey().Equals(merger.GetStealthPublicKey())))
                        {
                            mergee.Merge(merger);
                            merged = true;
                            break;
                        }
                    }
                    if (!merged) this.GetOutputs().Add(merger); // append output
                }
            }
        }

        // handle unrelayed -> relayed -> confirmed
        if (this.IsConfirmed() == true)
        {
            this.SetInTxPool(false);
            this.SetReceivedTimestamp(null);
            this.SetLastRelayedTimestamp(null);
        }
        else
        {
            this.SetInTxPool(GenUtils.Reconcile(this.InTxPool(), tx.InTxPool(), null, true, null)); // unrelayed -> tx pool
            this.SetReceivedTimestamp(GenUtils.Reconcile(this.GetReceivedTimestamp(), tx.GetReceivedTimestamp(), null, null, false)); // take earliest receive time
            this.SetLastRelayedTimestamp(GenUtils.Reconcile(this.GetLastRelayedTimestamp(), tx.GetLastRelayedTimestamp(), null, null, true));  // take latest relay time
        }

        return this;  // for chaining
    }
}
