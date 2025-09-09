using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroTxWallet : MoneroTx
{
    private MoneroTxSet txSet;
    private bool? isIncoming;
    private bool? isOutgoing;
    private List<MoneroIncomingTransfer> incomingTransfers;
    private MoneroOutgoingTransfer outgoingTransfer;
    private string note;
    private bool? isLocked;
    private ulong inputSum;
    private ulong outputSum;
    private string changeAddress;
    private ulong changeAmount;
    private uint numDummyOutputs;
    private string extraHex;  // TODO: refactor MoneroTx to only use extra as hex string

    public MoneroTxWallet()
    {
        // nothing to initialize
    }

    public MoneroTxWallet(MoneroTxWallet tx) : base(tx)
    {
        this.txSet = tx.txSet;
        this.isIncoming = tx.isIncoming;
        this.isOutgoing = tx.isOutgoing;
        if (tx.incomingTransfers != null)
        {
            this.incomingTransfers = new List<MoneroIncomingTransfer>();
            foreach (MoneroIncomingTransfer transfer in tx.incomingTransfers)
            {
                this.incomingTransfers.Add(transfer.Clone().SetTx(this));
            }
        }
        if (tx.outgoingTransfer != null) this.outgoingTransfer = tx.outgoingTransfer.Clone().SetTx(this);
        this.note = tx.note;
        this.isLocked = tx.isLocked;
        this.inputSum = tx.inputSum;
        this.outputSum = tx.outputSum;
        this.changeAddress = tx.changeAddress;
        this.changeAmount = tx.changeAmount;
        this.numDummyOutputs = tx.numDummyOutputs;
        this.extraHex = tx.extraHex;
    }

    public override MoneroTxWallet Clone()
    {
        return new MoneroTxWallet(this);
    }

    public MoneroTxSet GetTxSet()
    {
        return txSet;
    }

    public virtual MoneroTxWallet SetTxSet(MoneroTxSet txSet)
    {
        this.txSet = txSet;
        return this;
    }

    public virtual bool? IsIncoming()
    {
        return isIncoming;
    }

    public virtual MoneroTxWallet SetIsIncoming(bool? isIncoming)
    {
        this.isIncoming = isIncoming;
        return this;
    }

    public virtual bool? IsOutgoing()
    {
        return isOutgoing;
    }

    public virtual MoneroTxWallet SetIsOutgoing(bool? isOutgoing)
    {
        this.isOutgoing = isOutgoing;
        return this;
    }

    public ulong? GetIncomingAmount()
    {
        if (GetIncomingTransfers() == null) return null;
        ulong incomingAmt = 0;
        foreach (MoneroTransfer transfer in this.GetIncomingTransfers()) incomingAmt += (ulong)transfer.GetAmount();
        return incomingAmt;
    }

    public ulong? GetOutgoingAmount()
    {
        return GetOutgoingTransfer() != null ? GetOutgoingTransfer().GetAmount() : null;
    }

    public List<MoneroTransfer> GetTransfers()
    {
        return GetTransfers(null);
    }

    public List<MoneroTransfer> GetTransfers(MoneroTransferQuery query)
    {
        List<MoneroTransfer> transfers = new List<MoneroTransfer>();
        if (GetOutgoingTransfer() != null && (query == null || query.MeetsCriteria(GetOutgoingTransfer()))) transfers.Add(GetOutgoingTransfer());
        if (GetIncomingTransfers() != null)
        {
            foreach (MoneroTransfer transfer in GetIncomingTransfers())
            {
                if (query == null || query.MeetsCriteria(transfer)) transfers.Add(transfer);
            }
        }
        return transfers;
    }

    public List<MoneroTransfer> filterTransfers(MoneroTransferQuery query)
    {
        List<MoneroTransfer> transfers = new List<MoneroTransfer>();

        // collect outgoing transfer or erase if filtered
        if (GetOutgoingTransfer() != null && (query == null || query.MeetsCriteria(GetOutgoingTransfer()))) transfers.Add(GetOutgoingTransfer());
        else SetOutgoingTransfer(null);

        // collect incoming transfers or erase if filtered
        if (GetIncomingTransfers() != null)
        {
            List<MoneroTransfer> toRemoves = new List<MoneroTransfer>();
            foreach (MoneroTransfer transfer in GetIncomingTransfers())
            {
                if (query == null || query.MeetsCriteria(transfer)) transfers.Add(transfer);
                else toRemoves.Add(transfer);
            }

            // TODO GetIncomingTransfers().RemoveAll(toRemoves);
            if (GetIncomingTransfers().Count == 0) SetIncomingTransfers(null);
        }

        return transfers;
    }

    public List<MoneroIncomingTransfer> GetIncomingTransfers()
    {
        return incomingTransfers;
    }

    public virtual MoneroTxWallet SetIncomingTransfers(List<MoneroIncomingTransfer> incomingTransfers)
    {
        this.incomingTransfers = incomingTransfers;
        return this;
    }

    public MoneroOutgoingTransfer GetOutgoingTransfer()
    {
        return outgoingTransfer;
    }

    public virtual MoneroTxWallet SetOutgoingTransfer(MoneroOutgoingTransfer outgoingTransfer)
    {
        this.outgoingTransfer = outgoingTransfer;
        return this;
    }


    public override MoneroTxWallet SetInputs(List<MoneroOutput> inputs)
    {

        // Validate that all inputs are wallet inputs
        if (inputs != null)
        {
            foreach (MoneroOutput input in inputs)
            {
                MoneroOutputWallet inputw = (MoneroOutputWallet)input;
                if (inputw == null) throw new MoneroError("Wallet transaction inputs must be of type MoneroOutputWallet");
            }
        }

        base.SetInputs(inputs);
        return this;
    }

    public virtual MoneroTxWallet SetInputsWallet(List<MoneroOutputWallet> inputs)
    {
        return SetInputs(new List<MoneroOutput>(inputs));
    }

    public List<MoneroOutputWallet> GetInputsWallet()
    {
        return GetInputsWallet(null);
    }

    public List<MoneroOutputWallet> GetInputsWallet(MoneroOutputQuery query)
    {
        List<MoneroOutputWallet> inputsWallet = new List<MoneroOutputWallet>();
        List<MoneroOutput> inputs = GetInputs();
        if (inputs == null) return inputsWallet;
        foreach (MoneroOutput output in inputs)
        {
            if (query == null || query.MeetsCriteria((MoneroOutputWallet)output)) inputsWallet.Add((MoneroOutputWallet)output);
        }
        return inputsWallet;
    }

    public override MoneroTxWallet SetOutputs(List<MoneroOutput> outputs)
    {

        // Validate that all outputs are wallet outputs
        if (outputs != null)
        {
            foreach (MoneroOutput output in outputs)
            {
                MoneroOutputWallet outw = (MoneroOutputWallet)output;
                if (outw == null) throw new MoneroError("Wallet transaction outputs must be of type MoneroOutputWallet");
            }
        }
        base.SetOutputs(outputs);
        return this;
    }


    public virtual MoneroTxWallet SetOutputsWallet(List<MoneroOutputWallet> outputs)
    {
        return SetOutputs([.. outputs]);
    }

    public List<MoneroOutputWallet> GetOutputsWallet()
    {
        return GetOutputsWallet(null);
    }

    public List<MoneroOutputWallet> GetOutputsWallet(MoneroOutputQuery query)
    {
        List<MoneroOutputWallet> outputsWallet = new List<MoneroOutputWallet>();
        List<MoneroOutput> outputs = GetOutputs();
        if (outputs == null) return outputsWallet;
        foreach (MoneroOutput output in outputs)
        {
            if (query == null || query.MeetsCriteria((MoneroOutputWallet)output)) outputsWallet.Add((MoneroOutputWallet)output);
        }
        return outputsWallet;
    }

    public List<MoneroOutputWallet> filterOutputsWallet(MoneroOutputQuery query)
    {
        List<MoneroOutputWallet> outputs = new List<MoneroOutputWallet>();
        if (GetOutputs() != null)
        {
            List<MoneroOutput> toRemoves = new List<MoneroOutput>();
            foreach (MoneroOutput output in GetOutputs())
            {
                if (query == null || query.MeetsCriteria((MoneroOutputWallet)output)) outputs.Add((MoneroOutputWallet)output);
                else toRemoves.Add(output);
            }
            // TODO GetOutputs().RemoveAll(toRemoves);
            if (GetOutputs().Count == 0) SetOutputs(null);
        }
        return outputs;
    }

    public string GetNote()
    {
        return note;
    }

    public virtual MoneroTxWallet SetNote(string note)
    {
        this.note = note;
        return this;
    }

    public bool? IsLocked()
    {
        return isLocked;
    }

    public virtual MoneroTxWallet SetIsLocked(bool? isLocked)
    {
        this.isLocked = isLocked;
        return this;
    }

    public ulong GetInputSum()
    {
        return inputSum;
    }

    public virtual MoneroTxWallet SetInputSum(ulong inputSum)
    {
        this.inputSum = inputSum;
        return this;
    }

    public ulong GetOutputSum()
    {
        return outputSum;
    }

    public virtual MoneroTxWallet SetOutputSum(ulong outputSum)
    {
        this.outputSum = outputSum;
        return this;
    }

    public string GetChangeAddress()
    {
        return changeAddress;
    }

    public virtual MoneroTxWallet SetChangeAddress(string changeAddress)
    {
        this.changeAddress = changeAddress;
        return this;
    }

    public ulong GetChangeAmount()
    {
        return changeAmount;
    }

    public virtual MoneroTxWallet SetChangeAmount(ulong changeAmount)
    {
        this.changeAmount = changeAmount;
        return this;
    }

    public uint GetNumDummyOutputs()
    {
        return numDummyOutputs;
    }

    public virtual MoneroTxWallet SetNumDummyOutputs(uint numDummyOutputs)
    {
        this.numDummyOutputs = numDummyOutputs;
        return this;
    }

    public string GetExtraHex()
    {
        return extraHex;
    }

    public virtual MoneroTxWallet SetExtraHex(string extraHex)
    {
        this.extraHex = extraHex;
        return this;
    }
}