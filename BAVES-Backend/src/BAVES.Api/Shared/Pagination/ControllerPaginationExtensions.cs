using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Shared.Pagination;

public static class ControllerPaginationExtensions
{
    public const string HeaderTotalCount = "X-Total-Count";
    public const string HeaderPage = "X-Page";
    public const string HeaderPageSize = "X-Page-Size";
    public const string HeaderTotalPages = "X-Total-Pages";

    public static readonly string[] ExposedHeaders =
    [
        HeaderTotalCount,
        HeaderPage,
        HeaderPageSize,
        HeaderTotalPages
    ];

    public static void WritePaginationHeaders<T>(this ControllerBase controller, PagedResult<T> page)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(page);

        controller.Response.Headers[HeaderTotalCount] = page.Total.ToString();
        controller.Response.Headers[HeaderPage] = page.Page.ToString();
        controller.Response.Headers[HeaderPageSize] = page.PageSize.ToString();
        controller.Response.Headers[HeaderTotalPages] = page.TotalPages.ToString();
    }

    /// <summary>
    /// Phân trang list và trả body là mảng items (không đổi shape FE).
    /// Metadata: X-Total-Count, X-Page, X-Page-Size, X-Total-Pages.
    /// Không gửi page/pageSize → trả toàn bộ danh sách (tương thích client hiện tại).
    /// </summary>
    public static ActionResult<IReadOnlyList<T>> OkPaged<T>(
        this ControllerBase controller,
        IEnumerable<T> source,
        int? page,
        int? pageSize)
    {
        var paged = Pagination.CreateOptional(source, page, pageSize);
        controller.WritePaginationHeaders(paged);
        return controller.Ok(paged.Items);
    }

    public static IActionResult OkPagedAction<T>(
        this ControllerBase controller,
        IEnumerable<T> source,
        int? page,
        int? pageSize)
    {
        var paged = Pagination.CreateOptional(source, page, pageSize);
        controller.WritePaginationHeaders(paged);
        return controller.Ok(paged.Items);
    }
}
