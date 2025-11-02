using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService) : Controller
{
    private readonly IPaymentService _paymentService = paymentService;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetPaymentResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var paymentResponse = _paymentService.GetPaymentById(id);

        if (paymentResponse is null)
        {
            return NotFound();
        }

        return new OkObjectResult(paymentResponse);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest paymentRequest)
    {
        var payment = await _paymentService.AuthorizePayment(paymentRequest);
        return new OkObjectResult(payment);
    }
}