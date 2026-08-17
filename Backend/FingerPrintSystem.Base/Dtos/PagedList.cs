namespace FingerPrintSystem.Base.Dtos;

public class PagedList<T> where T : DtoTemplate {
    public IReadOnlyList<T> Items { get; set; } = [];

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int Skip => (Page - 1) * PageSize;

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;
}