namespace MedConnect.Common;

public record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}