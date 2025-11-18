using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.DTOs;
using Platform.Application.ServiceInterfaces;

namespace Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("initiatePayment")]
        public async Task<IActionResult> InitiatePayment([FromBody] CreatePaymentDTO request)
        {
            var response = await _paymentService.InitiatePaymentAsync(request);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> HandleCallback([FromQuery] Dictionary<string, string> queryParams)
        {

            var success = await _paymentService.ValidatePaymentCallback(System.Text.Json.JsonSerializer.Serialize(queryParams));

            if (!success)
                throw new Exception("Payment validation failed or was unsuccessful."); ;

            return Redirect("http://localhost:4200/my-learning");
        }

    }
}
