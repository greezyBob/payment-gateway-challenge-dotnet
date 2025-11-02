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
    public class PaymentService(IBankClient client, PaymentsRepository repository) : IPaymentService
    {
        private readonly IBankClient _bankClient = client;
        private readonly PaymentsRepository _paymentsRepository = repository;

        public GetPaymentResponse? GetPaymentById(Guid id)
        {
           var payment =  _paymentsRepository.Get(id);
            //convert to GetPaymentResponse
            return new GetPaymentResponse()
            {
                Id = payment.Id,
                Status = payment.Status,
                CardNumberLastFour = payment.CardNumberLastFour,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount
            };
        }

        public async Task<PostPaymentResponse> AuthorizePayment(PostPaymentRequest payment)
        {
            //convert PostPaymentRequest to Payment object

            //convert Payment object to PostBankRequest object

            var bankReponse = await _bankClient.AuthorizeAsync(new PostBankRequest());

            //update payment status based on BankResponse
            //store Payment object in repository
            _paymentsRepository.Add(new Payment());
            //convert Payment object PostPaymentResponse object
            var paymentResponse = new PostPaymentResponse()
            {
                CardNumberLastFour = payment.CardNumber,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount,
                Id = bankResponse!.Id,
                Status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            };

            return paymentResponse;
        }
    }
}
