namespace FingerPrintSystem.Base.Entities;

public abstract class EntityBase<T> : IEntityBase<T>, IAuditable
{
    protected EntityBase()
    {
    }

    protected EntityBase(T id)
    {
        Id = id;
    }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public T Id { get; set; } = default!;
}

public abstract class EntityBase : EntityBase<Guid>
{
    /// <inheritdoc />
    protected EntityBase(Guid id) : base(id)
    {
    }
}