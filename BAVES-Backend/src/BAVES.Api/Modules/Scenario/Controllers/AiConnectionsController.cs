using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Requests;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Scenario.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AiConnectionsController(IAiRuntimeClient aiRuntimeClient) : ControllerBase
{
    /// <summary>
    /// [Endpoint 20] Xác thực thông tin kết nối API Key BYOK (Bring Your Own Key) từ client.
    /// Cho phép nhận API Key từ Request Body hoặc từ HTTP Headers (X-AI-Provider, X-AI-Model, X-AI-Key).
    /// Tuyệt đối KHÔNG lưu trữ API Key vào cơ sở dữ liệu hoặc ghi vào log.
    /// </summary>
    [HttpPost("connections/validate")]
    [ProducesResponseType<ValidateAiConnectionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ValidateAiConnectionDto>> ValidateConnection(
        [FromBody] ValidateAiConnectionRequest? request,
        [FromHeader(Name = "X-AI-Provider")] string? headerProvider,
        [FromHeader(Name = "X-AI-Model")] string? headerModel,
        [FromHeader(Name = "X-AI-Key")] string? headerApiKey,
        CancellationToken cancellationToken
    )
    {
        // Ưu tiên lấy từ Headers theo đúng chuẩn BYOK đã cam kết, nếu không có thì lấy từ Body
        var effectiveProvider = !string.IsNullOrWhiteSpace(headerProvider) ? headerProvider : request?.Provider;
        var effectiveModel = !string.IsNullOrWhiteSpace(headerModel) ? headerModel : request?.Model;
        var effectiveApiKey = !string.IsNullOrWhiteSpace(headerApiKey) ? headerApiKey : request?.ApiKey;

        var validationRequest = new ValidateAiConnectionRequest
        {
            Provider = effectiveProvider,
            Model = effectiveModel,
            ApiKey = effectiveApiKey
        };

        var result = await aiRuntimeClient.ValidateConnectionAsync(validationRequest, cancellationToken);
        return Ok(result);
    }
}
