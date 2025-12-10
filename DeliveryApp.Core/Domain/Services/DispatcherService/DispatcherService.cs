using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services.DispatcherService;

public class DispatcherService : IDispatcherService
{
    public Result<Courier, Error> DispatchOrder(Order order, ICollection<Courier> couriers)
    {
        if (order == null)
            return GeneralErrors.ValueIsRequired(nameof(order));
        if (order.Status != OrderStatus.Created)
            return Errors.WrongOrderStatus();

        if (couriers is null)
            return GeneralErrors.ValueIsRequired(nameof(couriers));
        if (couriers.Count == 0)
            return Errors.NoCourierFound();

        var candidates = couriers
            .Where(c => c.CanTakeOrder(order.Volume).IsSuccess)
            .Select(c => new
            {
                Courier = c,
                TicsRes = c.TicsToDestination(order.Location)
            })
            .Where(c => c.TicsRes.IsSuccess)
            .Select(c => new
            {
                c.Courier,
                Tics = c.TicsRes.Value
            })
            .ToList();

        if (candidates.Count == 0)
            return Errors.NoCourierFound();

        var winner = candidates.MinBy(x => x.Tics).Courier;

        var assignRes = order.AssignToCourier(winner.Id);
        if (assignRes.IsFailure)
            return assignRes.Error;

        var takeRes = winner.TakeOrder(order.Id, order.Volume);
        if (takeRes.IsFailure)
            return takeRes.Error;

        return winner;
    }


    public static class Errors
    {
        public static Error WrongOrderStatus()
        {
            return new Error("dispatch.no.created.order", "Order is not in Created status to dispatch");
        }

        public static Error NoCourierFound()
        {
            return new Error("dispatch.no.couriers", "Курьер не найден");
        }
    }
}