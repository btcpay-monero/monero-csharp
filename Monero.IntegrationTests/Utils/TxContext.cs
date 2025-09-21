using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.IntegrationTests.Utils;

public class TxContext
{
    public IMoneroWallet? Wallet;
    public MoneroTxConfig? Config;
    public bool? HasOutgoingTransfer;
    public bool? HasIncomingTransfers;
    public bool? HasDestinations;
    public bool? IsCopy;                 // indicates if a copy is being tested which means back references won't be the same
    public bool? IncludeOutputs;
    public bool? IsSendResponse;
    public bool? IsSweepResponse;
    public bool? IsSweepOutputResponse;  // TODO monero-wallet-rpc: this only necessary because sweep_output does not return account index

    public TxContext() { }

    public TxContext(TxContext? ctx)
    {
        if (ctx == null)
        {
            return;
        }
        Wallet = ctx.Wallet;
        Config = ctx.Config;
        HasOutgoingTransfer = ctx.HasOutgoingTransfer;
        HasIncomingTransfers = ctx.HasIncomingTransfers;
        HasDestinations = ctx.HasDestinations;
        IsCopy = ctx.IsCopy;
        IncludeOutputs = ctx.IncludeOutputs;
        IsSendResponse = ctx.IsSendResponse;
        IsSweepResponse = ctx.IsSweepResponse;
        IsSweepOutputResponse = ctx.IsSweepOutputResponse;
    }

    public TxContext Clone()
    {
        return new TxContext(this);
    }
}