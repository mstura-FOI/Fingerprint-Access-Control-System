namespace FingerPrintSystem.Base.Dtos;

public class PageRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public int Skip => (Page - 1) * PageSize;
}