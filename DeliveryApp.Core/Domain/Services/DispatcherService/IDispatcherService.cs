using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services.DispatcherService;

public interface IDispatcherService
{
    public Result<Courier, Error> DispatchOrder(Order order, ICollection<Courier> couriers);
}