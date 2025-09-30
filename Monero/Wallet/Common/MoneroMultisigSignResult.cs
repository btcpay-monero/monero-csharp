namespace Monero.Wallet.Common;

public class MoneroMultisigSignResult
{
    public string? SignedMultisigTxHex { get; set; }

    public List<string>? TxHashes { get; set; }
}