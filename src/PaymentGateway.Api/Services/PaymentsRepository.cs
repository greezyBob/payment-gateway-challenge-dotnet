using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    public void Add(Payment payment);
    public Payment? Get(Guid id);
}


public class PaymentsRepository : IPaymentsRepository
{
    public List<Payment> Payments = new();

    public void Add(Payment payment)
    {
        Payments.Add(payment);
    }

    public Payment? Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}