namespace BAVES.Api.Shared.Pagination;

public static class Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        var safePage = page < 1 ? DefaultPage : page;
        var safePageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        return (safePage, safePageSize);
    }

    public static PagedResult<T> Create<T>(IReadOnlyList<T> allItems, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(allItems);

        var (safePage, safePageSize) = Normalize(page, pageSize);
        var total = allItems.Count;
        if (total == 0)
        {
            return new PagedResult<T>(Array.Empty<T>(), safePage, safePageSize, 0);
        }

        var skip = (safePage - 1) * safePageSize;
        if (skip >= total)
        {
            return new PagedResult<T>(Array.Empty<T>(), safePage, safePageSize, total);
        }

        var items = allItems.Skip(skip).Take(safePageSize).ToList();
        return new PagedResult<T>(items, safePage, safePageSize, total);
    }

    public static PagedResult<T> Create<T>(IEnumerable<T> source, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        IReadOnlyList<T> list = source as IReadOnlyList<T> ?? source.ToList();
        return Create(list, page, pageSize);
    }

    /// <summary>
    /// Nếu client không gửi page/pageSize → trả toàn bộ (tương thích FE hiện tại).
    /// Nếu có gửi → cắt trang theo chuẩn.
    /// </summary>
    public static PagedResult<T> CreateOptional<T>(IEnumerable<T> source, int? page, int? pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        IReadOnlyList<T> list = source as IReadOnlyList<T> ?? source.ToList();

        if (page is null && pageSize is null)
        {
            var size = list.Count == 0 ? DefaultPageSize : list.Count;
            return new PagedResult<T>(list, DefaultPage, size, list.Count);
        }

        return Create(list, page ?? DefaultPage, pageSize ?? DefaultPageSize);
    }
}
