using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Configuration;

public class BankOptions
{
    [Required]
    public string BankUrl { get; set; } = string.Empty;
}

