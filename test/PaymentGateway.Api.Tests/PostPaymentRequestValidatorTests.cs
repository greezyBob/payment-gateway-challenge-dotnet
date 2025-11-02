using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.UnitTests;

public class PostPaymentRequestValidatorTests
{
    private static PostPaymentRequest CreateValidRequest()
    {
        var now = DateTime.UtcNow;
        return new PostPaymentRequest
        {
            CardNumber = "4242424242424242",
            ExpiryMonth = now.Month == 12 ? 1 : now.Month + 1,
            ExpiryYear = now.Year + 1,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };
    }

    [Fact]
    public void Validate_ValidRequest_IsValid()
    {
        var validator = new PostPaymentRequestValidator();
        var req = CreateValidRequest();

        var result = validator.Validate(req);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abcd")]
    public void Validate_InvalidCardNumber_Fails(string cardNumber)
    {
        var validator = new PostPaymentRequestValidator();
        var req = CreateValidRequest();
        req.CardNumber = cardNumber;

        var result = validator.Validate(req);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber");
    }

    [Fact]
    public void Validate_ExpiredCard_Fails()
    {
        var validator = new PostPaymentRequestValidator();
        var now = DateTime.UtcNow;
        var req = CreateValidRequest();
        // set expiry to last month
        var lastMonth = now.AddMonths(-1);
        req.ExpiryMonth = lastMonth.Month;
        req.ExpiryYear = lastMonth.Year;

        var result = validator.Validate(req);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("expired") || e.PropertyName == "ExpiryYear" || e.PropertyName == "ExpiryMonth");
    }

    [Fact]
    public void Validate_UnsupportedCurrency_Fails()
    {
        var validator = new PostPaymentRequestValidator();
        var req = CreateValidRequest();
        req.Currency = "ABC";

        var result = validator.Validate(req);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_NonPositiveAmount_Fails(int amount)
    {
        var validator = new PostPaymentRequestValidator();
        var req = CreateValidRequest();
        req.Amount = amount;

        var result = validator.Validate(req);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Theory]
    [InlineData("")]
    [InlineData("12a")]
    [InlineData("12")] // too short
    public void Validate_InvalidCvv_Fails(string cvv)
    {
        var validator = new PostPaymentRequestValidator();
        var req = CreateValidRequest();
        req.Cvv = cvv;

        var result = validator.Validate(req);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Cvv");
    }
}
