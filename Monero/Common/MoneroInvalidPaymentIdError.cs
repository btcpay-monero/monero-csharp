
namespace Monero.Common;

public class MoneroInvalidPaymentIdError : MoneroError
{
    private readonly string paymentId;

    public MoneroInvalidPaymentIdError(string paymentId) : base("Invalid payment id provided: " + paymentId)
    {
        this.paymentId = paymentId;
    }

    public string GetPaymentId() { return paymentId; }

}