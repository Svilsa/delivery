using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class CourierRepository(ApplicationDbContext dbContext)
    : BaseRepository<Courier, Guid>(dbContext), ICourierRepository
{
    public async Task<IReadOnlyCollection<Courier>> GetAllAvailableCouriersAsync()
    {
        return (await DbContext.Couriers
                .Where(c => c.StoragePlaces.All(sp => sp.OrderId == null))
                .ToListAsync())
            .AsReadOnly();
    }
}