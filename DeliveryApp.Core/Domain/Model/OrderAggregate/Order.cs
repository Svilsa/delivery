using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public class Order : Aggregate<Guid>
{
    [ExcludeFromCodeCoverage]
    private Order(Guid id) : base(id)
    {
    }

    [ExcludeFromCodeCoverage]
    private Order(Guid id, Location location, int volume) : this(id)
    {
        Location = location;
        Volume = volume;
    }

    public Location Location { get; private set; }
    public int Volume { get; private set; }
    public OrderStatus Status { get; private set; }
    public Guid? CourierId { get; private set; }


    public static Result<Order, Error> Create(Guid id, Location location, int volume)
    {
        if (id == Guid.Empty)
            return GeneralErrors.ValueIsRequired(nameof(id));
        if (location is null)
            return GeneralErrors.ValueIsRequired(nameof(location));
        if (volume <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(volume));


        var order = new Order(id, location, volume)
        {
            Status = OrderStatus.Created
        };

        return order;
    }


    public UnitResult<Error> AssignToCourier(Guid courierId)
    {
        if (courierId == Guid.Empty)
            return GeneralErrors.ValueIsRequired(nameof(courierId));
        if (Status != OrderStatus.Created)
            return Errors.AssignNotCreatedOrder();

        CourierId = courierId;
        Status = OrderStatus.Assigned;

        return UnitResult.Success<Error>();
    }


    public UnitResult<Error> Complete()
    {
        if (Status != OrderStatus.Assigned)
            return Errors.CompleteNotAssignedOrder();

        Status = OrderStatus.Completed;

        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error AssignNotCreatedOrder()
        {
            return new Error("assign.not.created.order",
                "Only created orders can be assigned");
        }

        public static Error CompleteNotAssignedOrder()
        {
            return new Error("complete.not.assigned.order",
                "Only assigned orders can be completed");
        }
    }
}