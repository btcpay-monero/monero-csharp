﻿
namespace Monero.Daemon.Common
{
    public class MoneroOutputHistogramEntry
    {
        private ulong? _amount;
        private ulong? _numInstances;
        private ulong? _numUnlockedInstances;
        private ulong? _numRecentInstances;

        public ulong? GetAmount()
        {
            return _amount;
        }

        public void SetAmount(ulong? amount)
        {
            this._amount = amount;
        }

        public ulong? GetNumInstances()
        {
            return _numInstances;
        }

        public void SetNumInstances(ulong? numInstances)
        {
            this._numInstances = numInstances;
        }

        public ulong? GetNumUnlockedInstances()
        {
            return _numUnlockedInstances;
        }

        public void SetNumUnlockedInstances(ulong? numUnlockedInstances)
        {
            this._numUnlockedInstances = numUnlockedInstances;
        }

        public ulong? GetNumRecentInstances()
        {
            return _numRecentInstances;
        }

        public void SetNumRecentInstances(ulong? numRecentInstances)
        {
            this._numRecentInstances = numRecentInstances;
        }
    }
}
