using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class Courier : Aggregate<Guid>
{
    private const string DefaultStoragePlaceName = "Сумка";
    private const int DefaultStoragePlaceVolume = 10;
    private readonly List<StoragePlace> _storagePlaces = new();

    [ExcludeFromCodeCoverage]
    private Courier() : base(Guid.CreateVersion7())
    {
    }

    [ExcludeFromCodeCoverage]
    private Courier(string name, int speed, Location location) : this()
    {
        Name = name;
        Speed = speed;
        Location = location;
    }

    public string Name { get; private set; }
    public int Speed { get; }
    public Location Location { get; private set; }
    public IReadOnlyList<StoragePlace> StoragePlaces => _storagePlaces.AsReadOnly();


    public static Result<Courier, Error> Create(string name, int speed, Location location)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsRequired(nameof(name));
        if (speed <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(speed));
        if (location is null)
            return GeneralErrors.ValueIsRequired(nameof(location));

        var courier = new Courier(name, speed, location);
        var defaultStoragePlaceRes = StoragePlace.Create(DefaultStoragePlaceName, DefaultStoragePlaceVolume);

        if (defaultStoragePlaceRes.IsFailure)
            return defaultStoragePlaceRes.Error;

        courier._storagePlaces.Add(defaultStoragePlaceRes.Value);

        return courier;
    }


    public UnitResult<Error> AddStoragePlace(string name, int volume)
    {
        var newStoragePlaceRes = StoragePlace.Create(name, volume);

        if (newStoragePlaceRes.IsFailure)
            return newStoragePlaceRes;

        _storagePlaces.Add(newStoragePlaceRes.Value);

        return UnitResult.Success<Error>();
    }


    public UnitResult<Error> CanTakeOrder(int orderVolume)
    {
        var anyFreeAndFitsStorage =
            _storagePlaces.Any(sp => sp.CanStore(orderVolume) is { IsSuccess: true, Value: true });

        return !anyFreeAndFitsStorage ? Errors.AllStoragesOccupiedOrNotEnoughVolume() : UnitResult.Success<Error>();
    }


    public UnitResult<Error> TakeOrder(Guid orderId, int orderVolume)
    {
        var canTakeOrderRes = CanTakeOrder(orderVolume);
        if (canTakeOrderRes.IsFailure)
            return canTakeOrderRes;

        var storagePlace = _storagePlaces.First(sp => sp.CanStore(orderVolume) is { IsSuccess: true, Value: true });

        var storeOrderRes = storagePlace.StoreOrder(orderId, orderVolume);

        return storeOrderRes.IsFailure ? storeOrderRes : UnitResult.Success<Error>();
    }

    public UnitResult<Error> CompleteOrder(Guid orderId)
    {
        var storagePlace = StoragePlaces.FirstOrDefault(p => p.OrderId == orderId);
        if (storagePlace == null)
            return Errors.OrderIsNotFindInStoragePlaces();

        var removeOrderRes = storagePlace.RemoveOrder();

        return removeOrderRes.IsFailure ? removeOrderRes : UnitResult.Success<Error>();
    }

    public Result<int, Error> TicsToDestination(Location target)
    {
        var distance = Location.DistanceTo(target);

        return (int)Math.Ceiling((decimal)distance / Speed);
    }

    public Result<bool, Error> MoveTowardsLocation(Location destination)
    {
        var stepsRemaining = Speed;
        var x = Location.X;
        var y = Location.Y;

        while (stepsRemaining > 0)
        {
            var dx = destination.X - x;
            var dy = destination.Y - y;

            // Если уже на месте, выходим
            if (dx == 0 && dy == 0)
                break;

            // Делаем один шаг в сторону заказа
            if (dx != 0)
                x += Math.Sign(dx);
            else if (dy != 0)
                y += Math.Sign(dy);

            stepsRemaining--;
        }

        var newLocationRes = Location.Create(x, y);
        if (newLocationRes.IsFailure)
            return newLocationRes.Error;

        Location = newLocationRes.Value;
        var reachDestination = Location == destination;

        return reachDestination;
    }


    public static class Errors
    {
        public static Error AllStoragesOccupiedOrNotEnoughVolume()
        {
            return new Error("all.storages.occupied.or.not.enough.volume",
                "All storages are occupied or do not have enough volume");
        }

        public static Error OrderIsNotFindInStoragePlaces()
        {
            return new Error("order.is.not.find.in.storage.places", "Order is not found in storage places");
        }
        
        public static Error AlreadyStayAtGivenLocation()
        {
            return new Error("already.stay.at.given.location", "Already stay at given location");
        }
    }
}