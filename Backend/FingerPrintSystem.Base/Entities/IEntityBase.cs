namespace FingerPrintSystem.Base.Entities;

public interface IEntityBase<T>
{
    T Id { get; set; }
}

public interface IEntityBase : IEntityBase<Guid>;