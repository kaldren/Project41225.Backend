namespace Project41225.Domain.Entities;

public abstract class BaseEntity<TId>
{
    public TId Id { get; set; }
}
