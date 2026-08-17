using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Base.Services;

public interface IFxServiceSnapshot<TGet, TCreate> : IFxServiceSnapshot<TGet>
    where TGet : DtoTemplate
    where TCreate : DtoCreateTemplate
{
    Task<TGet> CreateAsync(
        TCreate dto,
        CancellationToken cancellationToken = default);
}

public interface IFxServiceSnapshot<TGet>
    where TGet : DtoTemplate
{
    Task<TGet?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedList<TGet>> GetListAsync(
        PageRequest request,
        CancellationToken cancellationToken = default);
}