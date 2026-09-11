using AirlineBooking.Domain.Common;

namespace AirlineBooking.Application.Common.Interfaces;

/// <summary>
/// Thin generic CRUD surface shared by every repository. Deliberately minimal - no
/// IQueryable escaping the interface, so services can't build ad-hoc LINQ queries that quietly
/// couple them to EF Core. Anything more specific than "get by id" / "list" / "add" / "remove"
/// belongs on the entity-specific repository interface (e.g. IFlightRepository.SearchAsync),
/// which keeps the query logic testable and out of the service layer.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
