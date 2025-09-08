
namespace Monero.Common
{
    public class MoneroOutput
    {
        private MoneroTx? _tx;
        private MoneroKeyImage? _keyImage;
        private ulong? _amount;
        private ulong? _index;
        private List<ulong>? _ringOutputIndices;
        private string? _stealthPublicKey;

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
                _ringOutputIndices = [..output._ringOutputIndices];
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
            _tx = tx;
            return this;
        }

        public MoneroKeyImage? GetKeyImage()
        {
            return _keyImage;
        }

        public virtual MoneroOutput SetKeyImage(MoneroKeyImage? keyImage)
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
            return _stealthPublicKey;
        }

        public virtual MoneroOutput SetStealthPublicKey(string? stealthPublicKey)
        {
            _stealthPublicKey = stealthPublicKey;
            return this;
        }

        public MoneroOutput Merge(MoneroOutput? output)
        {
            if (output == null)
            {
                throw new MoneroError("Output is null");
            }

            if (this == output)
            {
                return this;
            }

            // merge txs if they're different which comes back to merging outputs
            if (GetTx() != output.GetTx())
            {
                GetTx()!.Merge(output.GetTx()!);
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
        
        public bool Equals(MoneroOutput? other) 
        {
            if (this == other)
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }
            if (_amount == null) 
            {
                if (other._amount != null)
                {
                    return false;
                }
            } else if (_amount != other._amount)
            {
                return false;
            }
            if (_index == null) 
            {
                if (other._index != null)
                {
                    return false;
                }
            } else if (_index != other._index)
            {
                return false;
            }
            if (_keyImage == null) 
            {
                if (other._keyImage != null)
                {
                    return false;
                }
            } else if (!_keyImage.Equals(other._keyImage))
            {
                return false;
            }
            if (_ringOutputIndices == null)
            {
                if (other._ringOutputIndices != null)
                {
                    return false;
                }
            } else if (!_ringOutputIndices.Equals(other._ringOutputIndices))
            {
                return false;
            }
            if (_stealthPublicKey == null) 
            {
                if (other._stealthPublicKey != null)
                {
                    return false;
                }
            } else if (!_stealthPublicKey.Equals(other._stealthPublicKey))
            {
                return false;
            }
            
            return true;
        }
    }
}
