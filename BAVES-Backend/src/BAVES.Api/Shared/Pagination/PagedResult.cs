namespace BAVES.Api.Shared.Pagination;

/// <summary>
/// Kết quả phân trang chuẩn của API.
/// Body list endpoint vẫn trả về <see cref="Items"/> (mảng) để tương thích client hiện tại;
/// metadata nằm kèm trong DTO này khi dùng nội bộ / header HTTP.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

    public bool HasNext => Page < TotalPages;

    public bool HasPrevious => Page > 1;
}
