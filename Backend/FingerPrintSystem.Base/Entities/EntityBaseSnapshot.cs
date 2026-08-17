using FingerPrintSystem.Base.Entities;

namespace FingerPrintSystem.Base.Entities
{
    public abstract class EntityBaseSnapshot<T> : IEntityBase<T>, IAuditableSnapshot<Guid>
    {
        protected EntityBaseSnapshot()
        {
        }

        protected EntityBaseSnapshot(T id)
        {
            Id = id;
        }

        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public T Id { get; set; }
    }
}

public abstract class EntityBaseSnapshot : EntityBaseSnapshot<Guid>
{
    /// <inheritdoc />
    protected EntityBaseSnapshot(Guid id) : base(id)
    {
    }
}