namespace StoreHub.API.Helpers;

public class PagedResponse<T>
{
    public PagedResponse(IEnumerable<T> items, int count, int pageNumber, int pageSize, bool fromCache = false)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        FromCache = fromCache;
    }

    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool FromCache { get; set; }
}