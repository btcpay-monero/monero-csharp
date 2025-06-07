﻿
namespace Monero.Common
{
    public class MoneroKeyImage
    {
        private string _hex;
        private string? _signature;

        public enum SpentStatus
        {
            NOT_SPENT,
            CONFIRMED,
            TX_POOL
        }

        public MoneroKeyImage(string hex, string? signature = null)
        {
            _hex = hex;
            _signature = signature;
        }

        public MoneroKeyImage(MoneroKeyImage keyImage)
        {
            _hex = keyImage._hex;
            _signature = keyImage._signature;
        }

        public static SpentStatus ParseStatus(int status)
        {
            if (status == 1) return SpentStatus.NOT_SPENT;
            else if (status == 2) return SpentStatus.CONFIRMED;
            else if (status == 3) return SpentStatus.TX_POOL;

            throw new MoneroError("Invalid integer value for spent status: " + status);
        }

        public string GetHex()
        {
            return _hex;
        }

        public MoneroKeyImage SetHex(string hex)
        {
            _hex = hex;
            return this;
        }

        public string? GetSignature()
        {
            return _signature;
        }

        public MoneroKeyImage SetSignature(string? signature)
        {
            _signature = signature;
            return this;
        }

        public MoneroKeyImage Merge(MoneroKeyImage keyImage)
        {
            if (keyImage == this) return this;

            return this;
        }
    }
}
