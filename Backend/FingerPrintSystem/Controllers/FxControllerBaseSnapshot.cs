using FingerPrintSystem.Base.Controllers;
using FingerPrintSystem.Base.Dtos;
using FingerPrintSystem.Base.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.WebApi.Controllers;

public class
    FxControllerBaseSnapshot<TEntity, TGet, TCreate, TService, TContext> : FxControllerBaseSnapshot<TEntity, TGet,
    TService, TContext>
    where TService : FxServiceBaseSnapshot<TEntity, TGet, TCreate, TContext>
    where TEntity : class
    where TContext : DbContext
    where TGet : DtoTemplate
    where TCreate : DtoCreateTemplate 
{
    protected FxControllerBaseSnapshot(TService service) : base(service)
    {
    }

    [HttpPost]
    public virtual async Task<ActionResult<TGet>> Create(
        [FromBody] TCreate dto,
        CancellationToken cancellationToken = default)
    {
        var result = await Service.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Id },
            result);
    }
}

public class FxControllerBaseSnapshot<TEntity, TGet, TService, TContext> : FxControllerBase
    where TService : FxServiceBaseSnapshot<TEntity, TGet, TContext>
    where TEntity : class
    where TContext : DbContext
    where TGet : DtoTemplate
{
    protected FxControllerBaseSnapshot(TService service)
    {
        Service = service;
    }

    protected TService Service { get; }

    [HttpGet("{id:guid}")]
    public virtual async Task<ActionResult<TGet>> Get(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await Service.GetAsync(id, cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public virtual async Task<ActionResult<PagedList<TGet>>> GetList(
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await Service.GetListAsync(request, cancellationToken);

        return Ok(result);
    }
}