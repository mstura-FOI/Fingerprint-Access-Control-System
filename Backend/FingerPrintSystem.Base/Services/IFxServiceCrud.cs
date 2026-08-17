using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Base.Services;

public interface IFxServiceCrud<TGet, TCreate, TUpdate, TDelete>
    : IFxServiceSnapshot<TGet, TCreate>
    where TGet : DtoTemplate
    where TCreate : DtoCreateTemplate
    where TUpdate : DtoTemplate
    where TDelete : DtoTemplate
{
    Task<bool> UpdateAsync(
        TUpdate dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        TDelete dto,
        CancellationToken cancellationToken = default);
}