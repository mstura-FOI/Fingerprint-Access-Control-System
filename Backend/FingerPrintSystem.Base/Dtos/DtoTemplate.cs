namespace FingerPrintSystem.Base.Dtos;

public abstract class DtoTemplate<T>
{
    public T Id { get; set; }
}

public abstract class DtoTemplate : DtoTemplate<Guid>;