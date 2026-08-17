using FingerPrintSystem.Base.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public abstract class FxControllerBaseCrud<
    TEntity,
    TGet,
    TCreate,
    TUpdate,
    TDelete,
    TService,
    TContext>
    : FxControllerBaseSnapshot<
        TEntity,
        TGet,
        TCreate,
        TService,
        TContext>
    where TEntity : class
    where TContext : DbContext
    where TService : FxServiceBaseCrud<
        TEntity,
        TGet,
        TCreate,
        TUpdate,
        TDelete,
        TContext>
    where TGet : DtoTemplate
    where TCreate : DtoCreateTemplate
    where TUpdate : DtoTemplate
    where TDelete : DtoTemplate
{
    protected FxControllerBaseCrud(TService service)
        : base(service)
    {
    }

    [HttpPut]
    public virtual async Task<IActionResult> Update(
        [FromBody] TUpdate dto,
        CancellationToken cancellationToken = default)
    {
        var updated = await Service.UpdateAsync(dto, cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete]
    public virtual async Task<IActionResult> Delete(
        [FromBody] TDelete dto,
        CancellationToken cancellationToken = default)
    {
        var deleted = await Service.DeleteAsync(dto, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}