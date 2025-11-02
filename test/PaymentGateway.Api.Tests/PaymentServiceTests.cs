using PaymentGateway.Api.External;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentServiceTests
{
    private class FakeBankClient : IBankClient
    {
        private readonly PostBankResponse _response;

        public FakeBankClient(PostBankResponse response)
        {
            _response = response;
        }

        public Task<PostBankResponse> AuthorizeAsync(PostBankRequest request)
        {
            return Task.FromResult(_response);
        }
    }

    [Fact]
    public async Task AuthorizePayment_SetsStatusToAuthorized_WhenBankAuthorizes()
    {
        // Arrange
        var bankResponse = new PostBankResponse { Authorized = true, AuthorizationCode = "auth" };
        var bankClient = new FakeBankClient(bankResponse);
        var repository = new PaymentsRepository();
        var service = new PaymentService(bankClient, repository);

        var request = new PostPaymentRequest
        {
            CardNumber = "4242424242424241",
            ExpiryMonth = 12,
            ExpiryYear = 2026,
            Currency = "GBP",
            Amount = 1000,
            Cvv = "123"
        };

        // Act
        var result = await service.AuthorizePayment(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Authorized, result.Status);

        var saved = repository.Get(result.Id);
        Assert.NotNull(saved);
        Assert.Equal(PaymentStatus.Authorized, saved!.Status);
        Assert.Equal(request.Amount, saved.Amount);
        Assert.Equal(request.Currency, saved.Currency);
    }

    [Fact]
    public async Task AuthorizePayment_SetsStatusToDeclined_WhenBankDeclines()
    {
        // Arrange
        var bankResponse = new PostBankResponse { Authorized = false, AuthorizationCode = string.Empty };
        var bankClient = new FakeBankClient(bankResponse);
        var repository = new PaymentsRepository();
        var service = new PaymentService(bankClient, repository);

        var request = new PostPaymentRequest
        {
            CardNumber = "4242424242424242",
            ExpiryMonth = 1,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 500,
            Cvv = "321"
        };

        // Act
        var result = await service.AuthorizePayment(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Declined, result.Status);

        var saved = repository.Get(result.Id);
        Assert.NotNull(saved);
        Assert.Equal(PaymentStatus.Declined, saved!.Status);
    }

    [Fact]
    public void GetPaymentById_ReturnsMappedResponse_WhenPaymentExists()
    {
        // Arrange
        var repository = new PaymentsRepository();
        var payment = new Payment
        {
            Id = System.Guid.NewGuid(),
            CardNumber = "5555444433332222",
            ExpiryMonth = 6,
            ExpiryYear = 2027,
            Currency = "USD",
            Amount = 2500,
            Cvv = "999",
            Status = PaymentStatus.Authorized
        };

        repository.Add(payment);

        var service = new PaymentService(new FakeBankClient(new PostBankResponse { Authorized = false }), repository);

        // Act
        var response = service.GetPaymentById(payment.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(payment.Id, response!.Id);
        Assert.Equal(payment.Status, response.Status);
        Assert.Equal(payment.Amount, response.Amount);
        Assert.Equal(payment.Currency, response.Currency);
    }
}
