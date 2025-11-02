using System;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models
{
 public static class PaymentMapper
 {
     public static Payment ToPayment(this PostPaymentRequest request)
     {
        ArgumentNullException.ThrowIfNull(request);

        return new Payment
         {
            Id = Guid.NewGuid(),
            CardNumber = request.CardNumber,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv,
            Status = PaymentStatus.Pending
          };
     }

     public static PostBankRequest ToPostBankRequest(this Payment payment)
     {
        ArgumentNullException.ThrowIfNull(payment);

        var expiry = $"{payment.ExpiryMonth:D2}/{payment.ExpiryYear % 100:D2}";

         return new PostBankRequest
          {
            CardNumber = payment.CardNumber,
            ExpiryDate = expiry,
            Currency = payment.Currency,
            Amount = payment.Amount,
            Cvv = payment.Cvv
          };
     }

     public static PostPaymentResponse ToPostPaymentResponse(this Payment payment)
     {
         ArgumentNullException.ThrowIfNull(payment);
         
         return new PostPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status,
                CardNumberLastFour = payment.CardNumber.ToString(),
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount
            };
     }

     public static GetPaymentResponse ToGetPaymentResponse(this Payment payment)
     {
         ArgumentNullException.ThrowIfNull(payment);

         var lastFour = payment.CardNumber.Length >= 4
             ? payment.CardNumber[^4..]
             : payment.CardNumber;

         return new GetPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status,
                CardNumberLastFour = lastFour,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount
            };
     }
 }
}
