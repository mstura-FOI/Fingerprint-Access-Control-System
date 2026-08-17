using FingerPrintSystem.Base.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.Base.Services;

public abstract class FxServiceBaseSnapshot<TEntity, TGet, TCreate, TContext>(TContext context) : FxServiceBaseSnapshot<TEntity, TGet, TContext>(context), IFxServiceSnapshot<TGet, TCreate>
    where TEntity : class
    where TContext : DbContext
    where TGet : DtoTemplate
    where TCreate : DtoCreateTemplate 
{

    public virtual async Task<TGet> CreateAsync(
        TCreate dto,
        CancellationToken cancellationToken = default)
    {
        var entity = CreateEntity(dto);

        await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return await ProjectToGet(DbSet.Where(e => e == entity))
            .FirstAsync(cancellationToken);
    }

    #region Abstract mapping

    protected abstract TEntity CreateEntity(TCreate dto);

    #endregion
}

public abstract class FxServiceBaseSnapshot<TEntity, TGet, TContext>(TContext context) : IFxServiceSnapshot<TGet>
    where TEntity : class
    where TContext : DbContext
    where TGet : DtoTemplate
{
    protected readonly TContext Context = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public virtual async Task<TGet?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await ProjectToGet(DbSet)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public virtual async Task<PagedList<TGet>> GetListAsync(
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);

        var items = await ProjectToGet(query)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<TGet>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        };
    }


    #region Abstract mapping
    protected abstract IQueryable<TGet> ProjectToGet(IQueryable<TEntity> query);

    #endregion
}