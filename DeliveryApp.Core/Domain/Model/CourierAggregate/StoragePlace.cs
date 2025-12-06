using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class StoragePlace : Entity<Guid>
{
    [ExcludeFromCodeCoverage]
    private StoragePlace() : base(Guid.CreateVersion7())
    {
    }

    [ExcludeFromCodeCoverage]
    private StoragePlace(string name, int totalVolume) : this()
    {
        Name = name;
        TotalVolume = totalVolume;
    }

    public string Name { get; private set; }
    public int TotalVolume { get; }
    public Guid? OrderId { get; private set; }

    [MemberNotNullWhen(false, nameof(OrderId))]
    public bool IsEmpty => !OrderId.HasValue;

    public static Result<StoragePlace, Error> Create(string name, int totalVolume)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsRequired(nameof(name));

        if (totalVolume <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(totalVolume));

        return new StoragePlace(name, totalVolume);
    }

    // TODO: Переделать на UnitResult<Error>, ибо вся логика метода в том, что не может быть просто Result.Value = false,
    // без ошибки
    public Result<bool, Error> CanStore(int orderVolume)
    {
        if (orderVolume <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(orderVolume));
        if (!IsEmpty)
            return Errors.StorageIsOccupied();
        if (orderVolume > TotalVolume)
            return Errors.TooLargeOrderVolume();

        return true;
    }

    public UnitResult<Error> StoreOrder(Guid orderId, int orderVolume)
    {
        var canStoreResult = CanStore(orderVolume);
        if (!canStoreResult.IsSuccess)
            return canStoreResult;

        if (orderId == Guid.Empty)
            return GeneralErrors.ValueIsRequired(nameof(orderId));

        OrderId = orderId;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveOrder()
    {
        if (IsEmpty)
            return Errors.StorageIsEmpty();

        OrderId = null;

        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error StorageIsEmpty()
        {
            return new Error($"{nameof(StoragePlace).ToLowerInvariant()}.is.empty",
                "The storage is empty");
        }

        public static Error StorageIsOccupied()
        {
            return new Error($"{nameof(StoragePlace).ToLowerInvariant()}.is.occupied",
                "The storage is taken by another order");
        }

        public static Error TooLargeOrderVolume()
        {
            return new Error(
                "order.volume.is.too.large", "Storage volume is not enough to store the order volume");
        }
    }
}