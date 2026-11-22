using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BAVES.Api.Modules.Commerce.Gateways;

public class PayOsGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<PayOsGateway> _logger;

    private readonly string _clientId;
    private readonly string _apiKey;
    private readonly string _checksumKey;

    public string ProviderName => "PayOS";

    public PayOsGateway(HttpClient httpClient, IConfiguration config, ILogger<PayOsGateway> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;

        _clientId = _config["PayOS:ClientId"] ?? string.Empty;
        _apiKey = _config["PayOS:ApiKey"] ?? string.Empty;
        _checksumKey = _config["PayOS:ChecksumKey"] ?? string.Empty;
    }

    public async Task<PaymentLinkResult> CreatePaymentLinkAsync(
        Order order,
        string returnUrl,
        string cancelUrl,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_clientId) || string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("PayOS credentials not configured. Falling back to Mock link.");
            return new PaymentLinkResult(
                $"http://localhost:5000/mock-checkout?orderCode={order.OrderCode}",
                $"vietqr://mock?orderCode={order.OrderCode}",
                $"mock-{order.OrderCode}");
        }

        var amountInt = (int)order.Amount;
        var description = $"Don hang {order.OrderCode}";
        if (description.Length > 25)
        {
            description = description[..25];
        }

        // Tính chữ ký tạo link: amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}
        var rawData = $"amount={amountInt}&cancelUrl={cancelUrl}&description={description}&orderCode={order.OrderCode}&returnUrl={returnUrl}";
        var signature = ComputeHmacSha256(rawData, _checksumKey);

        var requestPayload = new PayOsPaymentLinkRequest
        {
            OrderCode = order.OrderCode,
            Amount = amountInt,
            Description = description,
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl,
            Signature = signature,
            ExpiredAt = order.ExpiresAt.ToUnixTimeSeconds()
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api-merchant.payos.vn/v2/payment-requests")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json")
        };

        requestMessage.Headers.Add("x-client-id", _clientId);
        requestMessage.Headers.Add("x-api-key", _apiKey);

        var response = await _httpClient.SendAsync(requestMessage, ct);
        var responseContent = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("PayOS CreatePaymentLink failed with status {Status}: {Body}", response.StatusCode, responseContent);
            throw new InvalidOperationException($"Lỗi kết nối cổng thanh toán PayOS: {responseContent}");
        }

        var resObj = JsonSerializer.Deserialize<PayOsPaymentLinkResponse>(responseContent);
        if (resObj?.Code != "00" || resObj.Data == null)
        {
            throw new InvalidOperationException($"PayOS trả mã lỗi: {resObj?.Code} - {resObj?.Desc}");
        }

        return new PaymentLinkResult(
            resObj.Data.CheckoutUrl ?? string.Empty,
            resObj.Data.QrCode,
            resObj.Data.PaymentLinkId);
    }

    public bool VerifyWebhookSignature(PayOsWebhookPayload payload)
    {
        if (payload.Data == null || string.IsNullOrWhiteSpace(payload.Signature))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_checksumKey))
        {
            // Nếu chưa cấu hình checksum key (môi trường dev/test), bỏ qua kiểm tra
            return true;
        }

        var d = payload.Data;
        // Chuỗi ký tự PayOS webhook: sắp xếp các thuộc tính theo alphabet
        // amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&reference={reference}&returnUrl={returnUrl}
        var parts = new SortedDictionary<string, string>
        {
            ["amount"] = d.Amount.ToString(),
            ["code"] = d.Code ?? string.Empty,
            ["desc"] = d.Desc ?? string.Empty,
            ["description"] = d.Description ?? string.Empty,
            ["orderCode"] = d.OrderCode.ToString(),
            ["reference"] = d.Reference ?? string.Empty,
            ["transactionDateTime"] = d.TransactionDateTime ?? string.Empty
        };

        var dataToSign = string.Join("&", parts.Where(p => !string.IsNullOrEmpty(p.Value)).Select(p => $"{p.Key}={p.Value}"));
        var expectedSig = ComputeHmacSha256(dataToSign, _checksumKey);

        return string.Equals(expectedSig, payload.Signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToHexStringLower(hash);
    }
}
