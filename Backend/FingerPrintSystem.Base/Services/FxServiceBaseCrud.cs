using FingerPrintSystem.Base.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.Base.Services;

public abstract class
    FxServiceBaseCrud<TEntity, TGet, TCreate, TUpdate, TDelete, TContext>
    : FxServiceBaseSnapshot<TEntity, TGet, TCreate, TContext>, IFxServiceCrud<TGet, TCreate, TUpdate, TDelete>
    where TEntity : class
    where TContext : DbContext
    where TGet : DtoTemplate
    where TCreate : DtoCreateTemplate
    where TUpdate : DtoTemplate
    where TDelete : DtoTemplate
{
    protected FxServiceBaseCrud(TContext context) : base(context)
    {
    }

    public virtual async Task<bool> UpdateAsync(
        TUpdate dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync([dto.Id], cancellationToken);
        ;

        if (entity is null)
            return false;

        UpdateEntity(entity, dto);

        await Context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public virtual async Task<bool> DeleteAsync(
        TDelete dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync([dto.Id], cancellationToken);
        ;

        if (entity is null)
            return false;

        DbSet.Remove(entity);

        await Context.SaveChangesAsync(cancellationToken);

        return true;
    }

    protected abstract void UpdateEntity(TEntity entity, TUpdate dto);
}