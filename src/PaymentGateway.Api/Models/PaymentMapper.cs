using System;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models
{
 public static class PaymentMapper
 {
     public static Payment ToPayment(this PostPaymentRequest request)
     {
         if (request == null) throw new ArgumentNullException(nameof(request));
       
         return new Payment
             {
                 Id = Guid.NewGuid(),
                 CardNumber = request.CardNumber,
                 ExpiryDate = request.ExpiryMonth,
                 ExpiryYear = request.ExpiryYear,
                 Currency = request.Currency,
                 Amount = request.Amount,
                 Cvv = request.Cvv
             };
     }

     public static PostBankRequest ToPostBankRequest(this PostPaymentRequest request)
     {
         if (request == null) throw new ArgumentNullException(nameof(request));

         var expiry = $"{request.ExpiryMonth:D2}/{request.ExpiryYear % 100:D2}";

         return new PostBankRequest
             {
                 CardNumber = request.CardNumber,
                 ExpiryDate = expiry,
                 Currency = request.Currency,
                 Amount = request.Amount,
                 Cvv = request.Cvv
             };
     }

     public static PostPaymentResponse ToPostPaymentResponse(this Payment payment, PostBankResponse bankResponse)
     {
         if (payment == null) throw new ArgumentNullException(nameof(payment));
         if (bankResponse == null) throw new ArgumentNullException(nameof(bankResponse));

         return new PostPaymentResponse
             {
                 Id = payment.Id,
                 Status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
                 CardNumberLastFour = payment.CardNumber.ToString(),
                 ExpiryMonth = payment.ExpiryDate,
                 ExpiryYear = payment.ExpiryYear,
                 Currency = payment.Currency,
                 Amount = payment.Amount
             };
     }

     public static GetPaymentResponse ToGetPaymentResponse(this Payment payment)
     {
         if (payment == null) throw new ArgumentNullException(nameof(payment));


         return new GetPaymentResponse
             {
                 Id = payment.Id,
                 Status = payment.Status,
                 CardNumberLastFour = payment.CardNumber.ToString(),
                 ExpiryMonth = payment.ExpiryDate,
                 ExpiryYear = payment.ExpiryYear,
                 Currency = payment.Currency,
                 Amount = payment.Amount
             };
     }
 }
}
