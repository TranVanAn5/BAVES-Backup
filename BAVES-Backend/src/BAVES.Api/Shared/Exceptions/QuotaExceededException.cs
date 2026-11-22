namespace BAVES.Api.Shared.Exceptions;

public class QuotaExceededException : Exception
{
    public QuotaExceededException(string message) : base(message)
    {
    }

    public QuotaExceededException(string quotaType, int remaining)
        : base($"Hạn mức '{quotaType}' của bạn đã hết (còn lại: {remaining}). Vui lòng mua thêm gói cước để tiếp tục.")
    {
    }
}
