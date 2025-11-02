namespace PaymentGateway.Api.Models.Responses
{
    public class PostBankResponse
    {
        public bool Authorized { get; set; }
        public string? AuthorizationCode { get; set; }
    }
}
