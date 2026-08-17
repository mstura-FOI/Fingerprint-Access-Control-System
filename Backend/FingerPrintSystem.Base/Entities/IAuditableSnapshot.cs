namespace FingerPrintSystem.Base.Entities;

public interface IAuditableSnapshot<T>
{
    DateTime CreatedAt { get; set; }
    T? CreatedBy { get; set; }
}

public interface IAuditableSnapshot : IAuditableSnapshot<Guid>;
