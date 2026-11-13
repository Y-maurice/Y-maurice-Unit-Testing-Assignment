namespace DriveNow.Domain.Abstractions;

public interface IPaymentGateway
{
    string Charge(int customerId, decimal amount, string description);

    void Refund(int customerId, decimal amount, string description);
}