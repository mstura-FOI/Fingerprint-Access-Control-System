namespace FingerPrintSystem.Base.Entities;

public interface IAuditable<T>
{
    DateTime CreatedAt { get; set; }
    T? CreatedBy { get; set; }
    DateTime? ModifiedAt { get; set; }
    T? ModifiedBy { get; set; }
}

public interface IAuditable : IAuditable<Guid?>;