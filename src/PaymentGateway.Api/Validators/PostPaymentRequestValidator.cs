using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    private static readonly HashSet<string> AllowedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD",
        "EUR",
        "GBP"
    };

    public PostPaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .Length(14, 19).WithMessage("Card number must be between 14 and 19 characters.")
            .Matches(@"^\d+$").WithMessage("Card number must contain only numeric characters.");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12.");

        RuleFor(x => x.ExpiryYear)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year).WithMessage("Expiry year cannot be in the past.");

        RuleFor(x => x)
            .Must(x => IsNotExpired(x.ExpiryMonth, x.ExpiryYear))
            .WithMessage("The card is expired.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.")
            .Matches("^[A-Za-z]{3}$").WithMessage("Currency must contain only letters.")
            .Must(c => !string.IsNullOrWhiteSpace(c) && AllowedCurrencies.Contains(c.ToUpper()))
            .WithMessage("Currency is not supported.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("CVV is required.")
            .Length(3, 4).WithMessage("CVV must be 3 or 4 digits.")
            .Matches(@"^\d+$").WithMessage("CVV must contain only numeric characters.");
    }

    private static bool IsNotExpired(int month, int year)
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiry = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            return expiry >= now;
        }
        catch
        {
            return false;
        }
    }
}
