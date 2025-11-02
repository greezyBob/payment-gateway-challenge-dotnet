
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.UnitTests;

public class PaymentMapperTests
{
    [Fact]
    public void ToPayment_MapsFieldsAndSetsPending()
    {
        var request = new PostPaymentRequest
        {
            CardNumber = "1111222233334444",
            ExpiryMonth = 12,
            ExpiryYear = 2030,
            Currency = "GBP",
            Amount = 1000,
            Cvv = "123"
        };

        var payment = request.ToPayment();

        Assert.NotNull(payment);
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(request.CardNumber, payment.CardNumber);
        Assert.Equal(request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(request.Currency, payment.Currency);
        Assert.Equal(request.Amount, payment.Amount);
        Assert.Equal(request.Cvv, payment.Cvv);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void ToPostBankRequest_FormatsExpiryAndMapsFields()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            CardNumber = "5555444433332222",
            ExpiryMonth = 3,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 2500,
            Cvv = "999",
            Status = PaymentStatus.Authorized
        };

        var bankRequest = payment.ToPostBankRequest();

        Assert.NotNull(bankRequest);
        Assert.Equal("03/27", bankRequest.ExpiryDate);
        Assert.Equal(payment.CardNumber, bankRequest.CardNumber);
        Assert.Equal(payment.Currency, bankRequest.Currency);
        Assert.Equal(payment.Amount, bankRequest.Amount);
        Assert.Equal(payment.Cvv, bankRequest.Cvv);
    }

    [Fact]
    public void ToPostPaymentResponse_ReturnsLastFourDigits()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            CardNumber = "1234567812345678",
            ExpiryMonth = 11,
            ExpiryYear = 2029,
            Currency = "EUR",
            Amount = 5000,
            Cvv = "321",
            Status = PaymentStatus.Authorized
        };

        var response = payment.ToPostPaymentResponse();

        Assert.NotNull(response);
        Assert.Equal("5678", response.CardNumberLastFour);
        Assert.Equal(payment.Id, response.Id);
        Assert.Equal(payment.Status, response.Status);
        Assert.Equal(payment.Amount, response.Amount);
        Assert.Equal(payment.Currency, response.Currency);
    }

    [Fact]
    public void ToPostPaymentResponse_ReturnsFullNumberWhenShorterThanFour()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            CardNumber = "123",
            ExpiryMonth = 1,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 10,
            Cvv = "000",
            Status = PaymentStatus.Declined
        };

        var response = payment.ToPostPaymentResponse();

        Assert.Equal("123", response.CardNumberLastFour);
    }

    [Fact]
    public void ToGetPaymentResponse_ReturnsLastFourAndMapsFields()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            CardNumber = "9999888877776666",
            ExpiryMonth = 6,
            ExpiryYear = 2031,
            Currency = "GBP",
            Amount = 750,
            Cvv = "111",
            Status = PaymentStatus.Authorized
        };

        var response = payment.ToGetPaymentResponse();

        Assert.NotNull(response);
        Assert.Equal("6666", response.CardNumberLastFour);
        Assert.Equal(payment.Id, response.Id);
        Assert.Equal(payment.Status, response.Status);
        Assert.Equal(payment.Amount, response.Amount);
        Assert.Equal(payment.Currency, response.Currency);
    }

    [Fact]
    public void Mapper_ThrowsOnNullInputs()
    {
        PostPaymentRequest? req = null;
        Payment? payment = null;

        Assert.Throws<ArgumentNullException>(() => req!.ToPayment());
        Assert.Throws<ArgumentNullException>(() => payment!.ToPostBankRequest());
        Assert.Throws<ArgumentNullException>(() => payment!.ToPostPaymentResponse());
        Assert.Throws<ArgumentNullException>(() => payment!.ToGetPaymentResponse());
    }
}
