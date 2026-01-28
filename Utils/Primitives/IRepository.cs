namespace Primitives;

public interface IRepository<TEntity, in TId> where TEntity : Aggregate<TId> where TId : IComparable<TId>
{
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(TId id);
}