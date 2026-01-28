using Microsoft.EntityFrameworkCore;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public abstract class BaseRepository<TEntity, TId>(ApplicationDbContext dbContext) : IRepository<TEntity, TId>
    where TEntity : Aggregate<TId>
    where TId : IComparable<TId>
{
    protected readonly ApplicationDbContext DbContext = dbContext;

    public virtual async Task AddAsync(TEntity entity)
    {
        await DbContext.Set<TEntity>().AddAsync(entity);
    }

    public virtual Task UpdateAsync(TEntity entity)
    {
        DbContext.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {
        return await DbContext.Set<TEntity>().FirstOrDefaultAsync(e => e.Id.CompareTo(id) == 0);
    }
}