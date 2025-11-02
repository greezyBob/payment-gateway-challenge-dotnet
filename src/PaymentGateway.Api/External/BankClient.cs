using System.Text.Json;
using System.Text.Json.Serialization;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.External
{
    public interface IBankClient
    {
        public Task<PostBankResponse> AuthorizeAsync(PostBankRequest request);
    }
    public class BankClient(HttpClient client) : IBankClient
    {
        private readonly HttpClient _httpClient = client;
        private readonly JsonSerializerOptions _jsonOptions = new ()
          {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
          };

        public async Task<PostBankResponse> AuthorizeAsync(PostBankRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/payments", request, _jsonOptions);

            response.EnsureSuccessStatusCode();

            var bankResponse = await response.Content.ReadFromJsonAsync<PostBankResponse>(_jsonOptions);

            if (bankResponse is null)
            {
                throw new InvalidOperationException("Bank response body was null.");
            }

            return bankResponse;
        }
    }
}
