using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.External;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();

    private static (HttpClient client, IPaymentsRepository repository) CreateClientWithRepo(IPaymentsRepository? repo = null)
    {
        var paymentsRepository = repo ?? new PaymentsRepository();

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IPaymentsRepository>(paymentsRepository);

                var bankMockUrl = Environment.GetEnvironmentVariable("BANK_MOCK_URL");
                if (!string.IsNullOrWhiteSpace(bankMockUrl))
                {
                    // Replace any existing IBankClient registration with an instance pointing at the mock bank URL
                    services.RemoveAll<IBankClient>();

                    var http = new HttpClient { BaseAddress = new Uri(bankMockUrl) };
                    http.DefaultRequestHeaders.Add("Accept", "application/json");

                    services.AddSingleton<IBankClient>(new BankClient(http));
                }
            }))
            .CreateClient();

        return (client, paymentsRepository);
    }

    private static PostPaymentRequest CreateValidPaymentRequest(int amount = 1000, string cardNumber = "123456789012345")
    {
        return new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "GBP",
            Amount = amount,
            Cvv = "123"
        };
    }

    private static PostPaymentRequest CreateInvalidPaymentRequest()
    {
        return new PostPaymentRequest
        {
            CardNumber = "4242424242424242",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "GBP",
            Amount = 0,
            Cvv = "123"
        };
    }

    [Fact]
    public async Task Get_Payments_ById_ReturnsOkWithPayment()
    {
        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Amount = 100,
            CardNumber = "1234567890123456",
            Currency = "GBP",
            Cvv = "123",
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var (client, _) = CreateClientWithRepo(paymentsRepository);

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Get_Payments_ByUnknownId_ReturnsNotFound()
    {
        // Arrange
        var (client, _) = CreateClientWithRepo();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Payments_ValidRequest_ReturnsOkAndSaved()
    {
        // Arrange
        var paymentsRepository = new PaymentsRepository();
        var (client, repository) = CreateClientWithRepo(paymentsRepository);

        var request = CreateValidPaymentRequest();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var postResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(postResponse);
        Assert.Equal(PaymentStatus.Authorized, postResponse!.Status);

        var saved = repository.Get(postResponse.Id);
        Assert.NotNull(saved);
        Assert.Equal(PaymentStatus.Authorized, saved!.Status);
    }

    [Fact]
    public async Task Post_Payments_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var (client, _) = CreateClientWithRepo();

        var request = CreateInvalidPaymentRequest();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Payments_ValidRequest_ThenGetById_ReturnsSamePayment()
    {
        // Arrange
        var paymentsRepository = new PaymentsRepository();
        var (client, repository) = CreateClientWithRepo(paymentsRepository);

        var request = CreateValidPaymentRequest(amount: 2000, cardNumber: "4242424242424243");

        // Act
        var postResponse = await client.PostAsJsonAsync("/api/Payments", request);
        var created = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();

        var getResponse = await client.GetAsync($"/api/Payments/{created!.Id}");
        var retrieved = await getResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(retrieved);
        Assert.Equal(created!.Id, retrieved!.Id);
        Assert.Equal(created.Amount, retrieved.Amount);
        Assert.Equal(created.Currency, retrieved.Currency);
    }
}