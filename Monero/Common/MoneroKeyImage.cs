
namespace Monero.Common
{
    public class MoneroKeyImage
    {
        private string? _hex;
        private string? _signature;

        public enum SpentStatus
        {
            NotSpent,
            Confirmed,
            TxPool
        }

        public MoneroKeyImage(string? hex = null, string? signature = null)
        {
            _hex = hex;
            _signature = signature;
        }

        public MoneroKeyImage(MoneroKeyImage keyImage)
        {
            _hex = keyImage._hex;
            _signature = keyImage._signature;
        }

        public MoneroKeyImage Clone()
        {
            return new MoneroKeyImage(this);
        }

        public static SpentStatus ParseStatus(int status)
        {
            if (status == 1) return SpentStatus.NotSpent;
            else if (status == 2) return SpentStatus.Confirmed;
            else if (status == 3) return SpentStatus.TxPool;

            throw new MoneroError("Invalid integer value for spent status: " + status);
        }

        public string? GetHex()
        {
            return _hex;
        }

        public MoneroKeyImage SetHex(string? hex)
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

        public MoneroKeyImage Merge(MoneroKeyImage? keyImage)
        {
            if (keyImage == null) throw new MoneroError("Cannot merge: key image is null");
            if (keyImage == this) return this;
            SetHex(GenUtils.Reconcile(GetHex(), keyImage.GetHex()));
            SetSignature(GenUtils.Reconcile(GetSignature(), keyImage.GetSignature()));
            return this;
        }

        public bool Equals(MoneroKeyImage? other)
        {
            if (other == null) return false;
            return _hex == other._hex && _signature == other._signature;
        }
    }
}
