﻿
namespace Monero.Wallet.Common
{
    public class MoneroMultisigInfo
    {
        private bool _isMultisig;
        private bool _isReady;
        private int _threshold;
        private int _numParticipants;

        public MoneroMultisigInfo(bool isMultisig = false, bool isReady = false, int threshold = 0, int numParticipants = 0)
        {
            _isMultisig = isMultisig;
            _isReady = isReady;
            _threshold = threshold;
            _numParticipants = numParticipants;
        }

        public MoneroMultisigInfo(MoneroMultisigInfo multisigInfo)
        {
            _isMultisig = multisigInfo._isMultisig;
            _isReady = multisigInfo._isReady;
            _threshold = multisigInfo._threshold;
            _numParticipants = multisigInfo._numParticipants;
        }

        public bool IsMultisig()
        {
            return _isMultisig;
        }

        public MoneroMultisigInfo SetIsMultisig(bool isMultisig)
        {
            _isMultisig = isMultisig;
            return this;
        }

        public bool IsReady()
        {
            return _isReady;
        }

        public MoneroMultisigInfo SetIsReady(bool isReady)
        {
            _isReady = isReady;
            return this;
        }

        public int GetThreshold()
        {
            return _threshold;
        }

        public MoneroMultisigInfo SetThreshold(int threshold)
        {
            _threshold = threshold;
            return this;
        }

        public int GetNumParticipants()
        {
            return _numParticipants;
        }

        public MoneroMultisigInfo SetNumParticipants(int numParticipants)
        {
            _numParticipants = numParticipants;
            return this;
        }

        public MoneroMultisigInfo Clone()
        {
            return new MoneroMultisigInfo(this);
        }
    }
}
