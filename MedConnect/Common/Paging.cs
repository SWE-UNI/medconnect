namespace MedConnect.Common;

public static class Paging
{
    public const int MaxPageSize = 200;

    public static int NormalizePageNumber(int pageNumber) => Math.Max(1, pageNumber);

    public static int NormalizePageSize(int pageSize) => Math.Clamp(pageSize, 1, MaxPageSize);
}