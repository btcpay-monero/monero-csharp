namespace Monero.Common;

public class MoneroInvalidPaymentIdError : MoneroError
{
    private readonly string _paymentId;

    public MoneroInvalidPaymentIdError(string paymentId) : base("Invalid payment id provided: " + paymentId)
    {
        _paymentId = paymentId;
    }

    public string GetPaymentId() { return _paymentId; }
}