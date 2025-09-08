
namespace Monero.Common
{
    public class MoneroTx
    {
        public static readonly string DefaultPaymentId = "0000000000000000";

        private MoneroBlock? _block;
        private string? _hash;
        private uint? _version;
        private bool? _isMinerTx;
        private string? _paymentId;
        private ulong? _fee;
        private uint? _ringSize;
        private bool? _relay;
        private bool? _isRelayed;
        private bool? _isConfirmed;
        private bool? _inTxPool;
        private ulong? _numConfirmations;
        private ulong? _unlockTime;
        private ulong? _lastRelayedTimestamp;
        private ulong? _receivedTimestamp;
        private bool? _isDoubleSpendSeen;
        private string? _key;
        private string? _fullHex;
        private string? _prunedHex;
        private string? _prunableHex;
        private string? _prunableHash;
        private ulong? _size;
        private ulong? _weight;
        private List<MoneroOutput>? _inputs;
        private List<MoneroOutput>? _outputs;
        private List<ulong>? _outputIndices;
        private string? _metadata;
        private byte[]? _extra; // TODO: switch to string for consistency with MoneroTxWallet?
        private object? _rctSignatures; // TODO: implement
        private object? _rctSigPrunable;  // TODO: implement
        private bool? _isKeptByBlock;
        private bool? _isFailed;
        private ulong? _lastFailedHeight;
        private string? _lastFailedHash;
        private ulong? _maxUsedBlockHeight;
        private string? _maxUsedBlockHash;
        private List<string>? _signatures;

        public virtual bool Equals(MoneroTx? other, bool checkInputs = true, bool checkOutputs = true)
        {
            if (other == null)
            {
                return false;
            }
            
            if (this == other)
            {
                return true;
            }

            if (checkInputs)
            {
                var inputs = GetInputs() ?? [];
                var otherInputs = other.GetInputs() ?? [];

                if (inputs.Count != otherInputs.Count)
                {
                    return false;
                }
                int i = 0;
                foreach (var input in inputs)
                {
                    var otherInput = otherInputs[i]!;
                    if (!input.Equals(otherInput))
                    {
                        return false;
                    }
                    i++;
                }

            }
            
            if (checkOutputs)
            {
                var outputs = GetOutputs() ?? [];
                var otherOutputs = other.GetOutputs() ?? [];

                if (outputs.Count != otherOutputs.Count)
                {
                    return false;
                }
                int i = 0;
                foreach (var output in outputs)
                {
                    var otherOutput = otherOutputs[i]!;
                    if (!output.Equals(otherOutput))
                    {
                        return false;
                    }
                    i++;
                }
            }

            var signatures = GetSignatures() ?? [];
            var otherSignatures = other.GetSignatures() ?? [];

            if (signatures.Count != otherSignatures.Count)
            {
                return false;
            }

            int j = 0;

            foreach (var signature in signatures)
            {
                if (signature != otherSignatures[j])
                {
                    return false;
                }
                j++;
            }

            var indices = GetOutputIndices() ?? [];
            var otherIndices = other.GetOutputIndices() ?? [];

            if (indices.Count != otherIndices.Count)
            {
                return false;
            }

            int k = 0;

            foreach (var index in indices)
            {
                if (index != otherIndices[k])
                {
                    return false;
                }
                k++;
            }
            
            return GetHash() == other.GetHash() &&
                   GetVersion() == other.GetVersion() &&
                   IsMinerTx() == other.IsMinerTx() &&
                   GetPaymentId() == other.GetPaymentId() &&
                   GetFee() == other.GetFee() &&
                   GetRingSize() == other.GetRingSize() &&
                   GetRelay() == other.GetRelay() &&
                   IsRelayed() == other.IsRelayed() &&
                   IsConfirmed() == other.IsConfirmed() &&
                   InTxPool() == other.InTxPool() &&
                   GetNumConfirmations() == other.GetNumConfirmations() &&
                   GetUnlockTime() == other.GetUnlockTime() &&
                   GetLastRelayedTimestamp() == other.GetLastRelayedTimestamp() &&
                   GetReceivedTimestamp() == other.GetReceivedTimestamp() &&
                   IsDoubleSpendSeen() == other.IsDoubleSpendSeen() &&
                   GetKey() == other.GetKey() &&
                   GetFullHex() == other.GetFullHex() &&
                   GetPrunedHex() == other.GetPrunedHex() &&
                   GetPrunableHash() == other.GetPrunableHash() &&
                   GetSize() == other.GetSize() &&
                   GetWeight() == other.GetWeight() &&
                   GetMetadata() == other.GetMetadata() &&
                   GetExtra() == other.GetExtra() &&
                   GetRctSignatures() == other.GetRctSignatures() &&
                   GetRctSigPrunable() == other.GetRctSigPrunable() &&
                   IsKeptByBlock() == other.IsKeptByBlock() &&
                   IsFailed() == other.IsFailed() &&
                   GetLastFailedHeight() == other.GetLastFailedHeight() &&
                   GetLastFailedHash() == other.GetLastFailedHash() &&
                   GetMaxUsedBlockHeight() == other.GetMaxUsedBlockHeight() &&
                   GetMaxUsedBlockHash() == other.GetMaxUsedBlockHash();
        }
        
        public MoneroTx()
        {
            // nothing to build
        }

        public MoneroTx(MoneroTx tx)
        {
            
            _hash = tx._hash;
            _version = tx._version;
            _isMinerTx = tx._isMinerTx;
            _paymentId = tx._paymentId;
            _fee = tx._fee;
            _ringSize = tx._ringSize;
            _relay = tx._relay;
            _isRelayed = tx._isRelayed;
            _isConfirmed = tx._isConfirmed;
            _inTxPool = tx._inTxPool;
            _numConfirmations = tx._numConfirmations;
            _unlockTime = tx._unlockTime;
            _lastRelayedTimestamp = tx._lastRelayedTimestamp;
            _receivedTimestamp = tx._receivedTimestamp;
            _isDoubleSpendSeen = tx._isDoubleSpendSeen;
            _key = tx._key;
            _fullHex = tx._fullHex;
            _prunedHex = tx._prunedHex;
            _prunableHex = tx._prunableHex;
            _prunableHash = tx._prunableHash;
            _size = tx._size;
            _weight = tx._weight;
            if (tx._inputs != null)
            {
                _inputs = new List<MoneroOutput>();
                foreach (MoneroOutput input in tx._inputs)
                {
                    _inputs.Add(input.Clone().SetTx(this));
                }
            }
            if (tx._outputs != null)
            {
                _outputs = new List<MoneroOutput>();
                foreach (MoneroOutput output in tx._outputs)
                {
                    _outputs.Add(output.Clone().SetTx(this));
                }
            }

            if (tx._outputIndices != null)
            {
                _outputIndices = [..tx._outputIndices];
            }
            _metadata = tx._metadata;
            if (tx._extra != null)
            {
                _extra = tx._extra;
            }
            _rctSignatures = tx._rctSignatures;
            _rctSigPrunable = tx._rctSigPrunable;
            _isKeptByBlock = tx._isKeptByBlock;
            _isFailed = tx._isFailed;
            _lastFailedHeight = tx._lastFailedHeight;
            _lastFailedHash = tx._lastFailedHash;
            _maxUsedBlockHeight = tx._maxUsedBlockHeight;
            _maxUsedBlockHash = tx._maxUsedBlockHash;
            if (tx._signatures != null)
            {
                _signatures = [..tx._signatures];
            }
        }

        public virtual MoneroTx Clone()
        {
            return new MoneroTx(this);
        }

        public MoneroBlock? GetBlock()
        {
            return _block;
        }

        public virtual MoneroTx SetBlock(MoneroBlock? block)
        {
            _block = block;
            return this;
        }

        public virtual ulong? GetHeight()
        {
            return GetBlock()?.GetHeight();
        }

        public virtual string? GetHash()
        {
            return _hash;
        }

        public virtual MoneroTx SetHash(string? hash)
        {
            _hash = hash;
            return this;
        }

        public virtual uint? GetVersion()
        {
            return _version;
        }

        public virtual MoneroTx SetVersion(uint? version)
        {
            _version = version;
            return this;
        }

        public bool? IsMinerTx()
        {
            return _isMinerTx;
        }

        public virtual MoneroTx SetIsMinerTx(bool? isMinerTx)
        {
            _isMinerTx = isMinerTx;
            return this;
        }

        public string? GetPaymentId()
        {
            return _paymentId;
        }

        public virtual MoneroTx SetPaymentId(string? paymentId)
        {
            _paymentId = paymentId;
            return this;
        }

        public ulong? GetFee()
        {
            return _fee;
        }

        public virtual MoneroTx SetFee(ulong? fee)
        {
            _fee = fee;
            return this;
        }

        public uint? GetRingSize()
        {
            return _ringSize;
        }

        public virtual MoneroTx SetRingSize(uint? ringSize)
        {
            _ringSize = ringSize;
            return this;
        }

        public bool? GetRelay()
        {
            return _relay;
        }

        public virtual MoneroTx SetRelay(bool? relay)
        {
            _relay = relay;
            return this;
        }

        public bool? IsRelayed()
        {
            return _isRelayed;
        }

        public virtual MoneroTx SetIsRelayed(bool? isRelayed)
        {
            _isRelayed = isRelayed;
            return this;
        }

        public bool? IsConfirmed()
        {
            return _isConfirmed;
        }

        public virtual MoneroTx SetIsConfirmed(bool? isConfirmed)
        {
            _isConfirmed = isConfirmed;
            return this;
        }

        public bool? InTxPool()
        {
            return _inTxPool;
        }

        public virtual MoneroTx SetInTxPool(bool? inTxPool)
        {
            _inTxPool = inTxPool;
            return this;
        }

        public ulong? GetNumConfirmations()
        {
            return _numConfirmations;
        }

        public virtual MoneroTx SetNumConfirmations(ulong? numConfirmations)
        {
            _numConfirmations = numConfirmations;
            return this;
        }

        public ulong? GetUnlockTime()
        {
            return _unlockTime;
        }

        public virtual MoneroTx SetUnlockTime(ulong? unlockTime)
        {
            _unlockTime = unlockTime;
            return this;
        }

        public ulong? GetLastRelayedTimestamp()
        {
            return _lastRelayedTimestamp;
        }

        public virtual MoneroTx SetLastRelayedTimestamp(ulong? lastRelayedTimestamp)
        {
            _lastRelayedTimestamp = lastRelayedTimestamp;
            return this;
        }

        public ulong? GetReceivedTimestamp()
        {
            return _receivedTimestamp;
        }

        public virtual MoneroTx SetReceivedTimestamp(ulong? receivedTimestamp)
        {
            _receivedTimestamp = receivedTimestamp;
            return this;
        }

        public bool? IsDoubleSpendSeen()
        {
            return _isDoubleSpendSeen;
        }

        public virtual MoneroTx SetIsDoubleSpendSeen(bool? isDoubleSpend)
        {
            _isDoubleSpendSeen = isDoubleSpend;
            return this;
        }

        public string? GetKey()
        {
            return _key;
        }

        public virtual MoneroTx SetKey(string? key)
        {
            _key = key;
            return this;
        }

        public string? GetFullHex()
        {
            return _fullHex;
        }

        public virtual MoneroTx SetFullHex(string? fullHex)
        {
            _fullHex = fullHex;
            return this;
        }

        public string? GetPrunedHex()
        {
            return _prunedHex;
        }

        public virtual MoneroTx SetPrunedHex(string? prunedHex)
        {
            _prunedHex = prunedHex;
            return this;
        }

        public string? GetPrunableHex()
        {
            return _prunableHex;
        }

        public virtual MoneroTx SetPrunableHex(string? prunableHex)
        {
            _prunableHex = prunableHex;
            return this;
        }

        public string? GetPrunableHash()
        {
            return _prunableHash;
        }

        public virtual MoneroTx SetPrunableHash(string? prunableHash)
        {
            _prunableHash = prunableHash;
            return this;
        }

        public ulong? GetSize()
        {
            return _size;
        }

        public virtual MoneroTx SetSize(ulong? size)
        {
            _size = size;
            return this;
        }

        public ulong? GetWeight()
        {
            return _weight;
        }

        public virtual MoneroTx SetWeight(ulong? weight)
        {
            _weight = weight;
            return this;
        }

        public List<MoneroOutput>? GetInputs()
        {
            return _inputs;
        }

        public virtual MoneroTx SetInputs(List<MoneroOutput>? inputs)
        {
            _inputs = inputs;
            return this;
        }

        public List<MoneroOutput>? GetOutputs()
        {
            return _outputs;
        }

        public virtual MoneroTx SetOutputs(List<MoneroOutput>? outputs)
        {
            _outputs = outputs;
            return this;
        }

        public List<ulong>? GetOutputIndices()
        {
            return _outputIndices;
        }

        public virtual MoneroTx SetOutputIndices(List<ulong>? outputIndices)
        {
            _outputIndices = outputIndices;
            return this;
        }

        public string? GetMetadata()
        {
            return _metadata;
        }

        public virtual MoneroTx SetMetadata(string? metadata)
        {
            _metadata = metadata;
            return this;
        }

        public byte[]? GetExtra()
        {
            return _extra;
        }

        public virtual MoneroTx SetExtra(byte[]? extra)
        {
            _extra = extra;
            return this;
        }

        public object? GetRctSignatures()
        {
            return _rctSignatures;
        }

        public virtual MoneroTx SetRctSignatures(object? rctSignatures)
        {
            _rctSignatures = rctSignatures;
            return this;
        }

        public object? GetRctSigPrunable()
        {
            return _rctSigPrunable;
        }

        public virtual MoneroTx SetRctSigPrunable(object? rctSigPrunable)
        {
            _rctSigPrunable = rctSigPrunable;
            return this;
        }

        public bool? IsKeptByBlock()
        {
            return _isKeptByBlock;
        }

        public virtual MoneroTx SetIsKeptByBlock(bool? isKeptByBlock)
        {
            _isKeptByBlock = isKeptByBlock;
            return this;
        }

        public bool? IsFailed()
        {
            return _isFailed;
        }

        public virtual MoneroTx SetIsFailed(bool? isFailed)
        {
            _isFailed = isFailed;
            return this;
        }

        public ulong? GetLastFailedHeight()
        {
            return _lastFailedHeight;
        }

        public virtual MoneroTx SetLastFailedHeight(ulong? lastFailedHeight)
        {
            _lastFailedHeight = lastFailedHeight;
            return this;
        }

        public string? GetLastFailedHash()
        {
            return _lastFailedHash;
        }

        public virtual MoneroTx SetLastFailedHash(string? lastFailedHash)
        {
            _lastFailedHash = lastFailedHash;
            return this;
        }

        public ulong? GetMaxUsedBlockHeight()
        {
            return _maxUsedBlockHeight;
        }

        public virtual MoneroTx SetMaxUsedBlockHeight(ulong? maxUsedBlockHeight)
        {
            _maxUsedBlockHeight = maxUsedBlockHeight;
            return this;
        }

        public string? GetMaxUsedBlockHash()
        {
            return _maxUsedBlockHash;
        }

        public virtual MoneroTx SetMaxUsedBlockHash(string? maxUsedBlockHash)
        {
            _maxUsedBlockHash = maxUsedBlockHash;
            return this;
        }

        public List<string>? GetSignatures()
        {
            return _signatures;
        }

        public virtual MoneroTx SetSignatures(List<string>? signatures)
        {
            _signatures = signatures;
            return this;
        }

        public virtual MoneroTx Merge(MoneroTx? tx)
        {
            if (tx == null)
            {
                throw new MoneroError("Cannot merge null transaction");
            }
            if (this == tx)
            {
                return this;
            }
            var block = GetBlock();
            
            // merge blocks if they're different
            if (block != null && !block.Equals(tx.GetBlock()))
            {
                if (GetBlock() == null)
                {
                    SetBlock(tx.GetBlock());
                    var blockTxs = GetBlock()!.GetTxs() ?? [];
                    var txIndex = blockTxs.IndexOf(tx);
                    blockTxs[txIndex] = this; // update block to point to this tx
                }
                else if (tx.GetBlock() != null)
                {
                    GetBlock()!.Merge(tx.GetBlock()); // comes back to merging txs
                    return this;
                }
            }

            // otherwise merge tx fields
            SetHash(GenUtils.Reconcile(GetHash(), tx.GetHash()));
            SetVersion(GenUtils.Reconcile(GetVersion(), tx.GetVersion()));
            SetPaymentId(GenUtils.Reconcile(GetPaymentId(), tx.GetPaymentId()));
            SetFee(GenUtils.Reconcile(GetFee(), tx.GetFee()));
            SetRingSize(GenUtils.Reconcile(GetRingSize(), tx.GetRingSize()));
            SetIsConfirmed(GenUtils.Reconcile(IsConfirmed(), tx.IsConfirmed(), null, true, null));  // tx can become confirmed
            SetIsMinerTx(GenUtils.Reconcile(IsMinerTx(), tx.IsMinerTx(), null, null, null));
            SetRelay(GenUtils.Reconcile(GetRelay(), tx.GetRelay(), null, true, null));        // tx can become relayed
            SetIsRelayed(GenUtils.Reconcile(IsRelayed(), tx.IsRelayed(), null, true, null));  // tx can become relayed
            SetIsDoubleSpendSeen(GenUtils.Reconcile(IsDoubleSpendSeen(), tx.IsDoubleSpendSeen(), null, true, null)); // double spend can become seen
            SetKey(GenUtils.Reconcile(GetKey(), tx.GetKey()));
            SetFullHex(GenUtils.Reconcile(GetFullHex(), tx.GetFullHex()));
            SetPrunedHex(GenUtils.Reconcile(GetPrunedHex(), tx.GetPrunedHex()));
            SetPrunableHex(GenUtils.Reconcile(GetPrunableHex(), tx.GetPrunableHex()));
            SetPrunableHash(GenUtils.Reconcile(GetPrunableHash(), tx.GetPrunableHash()));
            SetSize(GenUtils.Reconcile(GetSize(), tx.GetSize()));
            SetWeight(GenUtils.Reconcile(GetWeight(), tx.GetWeight()));
            SetOutputIndices(GenUtils.Reconcile(GetOutputIndices(), tx.GetOutputIndices()));
            SetMetadata(GenUtils.Reconcile(GetMetadata(), tx.GetMetadata()));
            SetExtra(GenUtils.ReconcileByteArrays(GetExtra()!, tx.GetExtra()!));
            SetRctSignatures(GenUtils.Reconcile(GetRctSignatures(), tx.GetRctSignatures()));
            SetRctSigPrunable(GenUtils.Reconcile(GetRctSigPrunable(), tx.GetRctSigPrunable()));
            SetIsKeptByBlock(GenUtils.Reconcile(IsKeptByBlock(), tx.IsKeptByBlock()));
            SetIsFailed(GenUtils.Reconcile(IsFailed(), tx.IsFailed(), null, true, null));
            SetLastFailedHeight(GenUtils.Reconcile(GetLastFailedHeight(), tx.GetLastFailedHeight()));
            SetLastFailedHash(GenUtils.Reconcile(GetLastFailedHash(), tx.GetLastFailedHash()));
            SetMaxUsedBlockHeight(GenUtils.Reconcile(GetMaxUsedBlockHeight(), tx.GetMaxUsedBlockHeight()));
            SetMaxUsedBlockHash(GenUtils.Reconcile(GetMaxUsedBlockHash(), tx.GetMaxUsedBlockHash()));
            SetSignatures(GenUtils.Reconcile(GetSignatures(), tx.GetSignatures()));
            SetUnlockTime(GenUtils.Reconcile(GetUnlockTime(), tx.GetUnlockTime()));
            SetNumConfirmations(GenUtils.Reconcile(GetNumConfirmations(), tx.GetNumConfirmations(), null, null, true)); // num confirmations can increase

            // merge inputs
            if (tx.GetInputs() != null)
            {
                foreach (MoneroOutput merger in tx.GetInputs()!)
                {
                    bool merged = false;
                    merger.SetTx(this);
                    if (GetInputs() == null)
                    {
                        SetInputs([]);
                    }
                    foreach (MoneroOutput mergee in GetInputs()!)
                    {
                        if (mergee.GetKeyImage()!.GetHex()!.Equals(merger.GetKeyImage()!.GetHex()))
                        {
                            mergee.Merge(merger);
                            merged = true;
                            break;
                        }
                    }

                    if (!merged)
                    {
                        GetInputs()!.Add(merger);
                    }
                }
            }

            // merge outputs
            if (tx.GetOutputs() != null)
            {
                foreach (MoneroOutput output in tx.GetOutputs()!)
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
                    foreach (MoneroOutput merger in tx.GetOutputs()!)
                    {
                        bool merged = false;
                        merger.SetTx(this);
                        foreach (MoneroOutput mergee in GetOutputs()!)
                        {
                            if ((merger.GetKeyImage() != null && mergee.GetKeyImage()!.GetHex()!.Equals(merger.GetKeyImage()!.GetHex())) ||
                                (merger.GetStealthPublicKey() != null && mergee.GetStealthPublicKey()!.Equals(merger.GetStealthPublicKey())))
                            {
                                mergee.Merge(merger);
                                merged = true;
                                break;
                            }
                        }
                        if (!merged) {
                            GetOutputs()!.Add(merger); // append output
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
                SetInTxPool(GenUtils.Reconcile(InTxPool(), tx.InTxPool(), null, true, null)); // unrelayed -> tx pool
                SetReceivedTimestamp(GenUtils.Reconcile(GetReceivedTimestamp(), tx.GetReceivedTimestamp(), null, null, false)); // take earliest receive time
                SetLastRelayedTimestamp(GenUtils.Reconcile(GetLastRelayedTimestamp(), tx.GetLastRelayedTimestamp(), null, null, true));  // take latest relay time
            }

            return this;  // for chaining
        }
    }
}
