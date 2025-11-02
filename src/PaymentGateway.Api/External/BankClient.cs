using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.External
{
    public interface IBankClient
    {
        public Task<PostBankResponse> AuthorizeAsync(PostBankRequest request);
    }
    public class BankClient(HttpClient client): IBankClient
    {
        private readonly HttpClient _httpClient = client;

        public async Task<PostBankResponse> AuthorizeAsync(PostBankRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/payment", request);

            response.EnsureSuccessStatusCode();

            var bankResponse = await response.Content.ReadFromJsonAsync<PostBankResponse>();

            if (bankResponse is null)
            {
                throw new InvalidOperationException("Bank response body was null.");
            }

            return bankResponse;
        }
    }
}
