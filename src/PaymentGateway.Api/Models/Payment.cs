namespace PaymentGateway.Api.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryDate { get; set; }
        public int ExpiryYear { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
        public string Cvv { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
