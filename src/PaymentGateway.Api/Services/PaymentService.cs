using PaymentGateway.Api.External;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{

    public interface IPaymentService
    {   
        Task<PostPaymentResponse> AuthorizePayment(PostPaymentRequest payment);
        GetPaymentResponse? GetPaymentById(Guid id);
    }
    public class PaymentService(IBankClient client, IPaymentsRepository repository) : IPaymentService
    {
        private readonly IBankClient _bankClient = client;
        private readonly IPaymentsRepository _paymentsRepository = repository;

        public GetPaymentResponse? GetPaymentById(Guid id)
        {
           var payment =  _paymentsRepository.Get(id);
           return payment?.ToGetPaymentResponse();
        }

        public async Task<PostPaymentResponse> AuthorizePayment(PostPaymentRequest paymentRequest)
        {
            var payment = paymentRequest.ToPayment();
            
            var bankRequest = payment.ToPostBankRequest();
            var bankReponse = await _bankClient.AuthorizeAsync(bankRequest);

            payment.Status = bankReponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;

            _paymentsRepository.Add(payment);

            return payment.ToPostPaymentResponse();
        }
    }
}
