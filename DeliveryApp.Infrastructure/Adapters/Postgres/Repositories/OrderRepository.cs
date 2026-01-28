using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class OrderRepository(ApplicationDbContext dbContext)
    : BaseRepository<Order, Guid>(dbContext), IOrderRepository
{
    public Order? GetFirstCreatedOrder()
    {
        return DbContext.Orders.FirstOrDefault(o => o.Status.Name == OrderStatus.Created.Name);
    }

    public async Task<IReadOnlyCollection<Order>> GetAllAssignedOrdersAsync()
    {
        return (await DbContext.Orders
                .Where(o => o.Status.Name == OrderStatus.Assigned.Name)
                .ToListAsync())
            .AsReadOnly();
    }
}