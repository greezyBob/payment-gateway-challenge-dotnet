namespace PaymentGateway.Api.Models.Requests
{
    public class PostBankRequest
    {
        public required string CardNumber { get; set; }
        public required string ExpiryDate { get; set; }
        public required string Currency { get; set; }
        public int Amount { get; set; }
        public required string Cvv { get; set; }
    }
}
