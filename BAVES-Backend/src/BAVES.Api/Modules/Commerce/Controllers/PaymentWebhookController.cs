using System.Text.Json;
using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Commerce.Controllers;

[ApiController]
[Route("api/payments")]
[AllowAnonymous]
[Tags("Commerce - Webhook")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentWebhookService _webhookService;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(IPaymentWebhookService webhookService, ILogger<PaymentWebhookController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    /// <summary>
    /// Điểm tiếp nhận Webhook kết quả thanh toán từ PayOS (Tự động mở khóa kịch bản).
    /// </summary>
    [HttpPost("payos/webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandlePayOsWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return BadRequest(new { code = "01", message = "Request body is empty" });
        }

        try
        {
            var payload = JsonSerializer.Deserialize<PayOsWebhookPayload>(rawBody);
            if (payload == null)
            {
                return BadRequest(new { code = "01", message = "Invalid JSON structure" });
            }

            var result = await _webhookService.ProcessPayOsWebhookAsync(payload, rawBody, ct);
            if (!result.Success)
            {
                _logger.LogWarning("PayOS webhook processed with error: {Message}", result.Message);
                return BadRequest(new { code = "01", message = result.Message });
            }

            return Ok(new { code = "00", message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing PayOS webhook");
            return StatusCode(StatusCodes.Status500InternalServerError, new { code = "99", message = ex.Message });
        }
    }
}
