namespace Monero.Wallet.Common;

public class MoneroMultisigSignResponse
{
    public string? SignedMultisigTxHex { get; set; }

    public List<string>? TxHashes { get; set; }
}