using JazeeraBooking.Application.Common.Interfaces;
using JazeeraBooking.Domain.Common;
using JazeeraBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JazeeraBooking.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly JazeeraBookingDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(JazeeraBookingDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity) => DbSet.Update(entity);

    public virtual void Remove(T entity) => DbSet.Remove(entity);
}
