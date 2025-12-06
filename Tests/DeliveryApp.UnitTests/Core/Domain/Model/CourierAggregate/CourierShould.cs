using System;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Model.CourierAggregate;

public class CourierShould
{
    private const string ValidName = "Иван";
    private const int ValidSpeed = 5;
    private readonly Location _validLocation = Location.Create(4, 3).Value;

    [Theory]
    [InlineData("Алексей", 5)]
    [InlineData("Мария", 7)]
    [InlineData("Павел", 3)]
    public void BeCorrectWhenParamsAreCorrectOnCreated(string name, int speed)
    {
        // Act
        var res = Courier.Create(name, speed, _validLocation);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Name.Should().Be(name);
        res.Value.Speed.Should().Be(speed);
        res.Value.Location.Should().Be(_validLocation);
        res.Value.StoragePlaces.Should().HaveCount(1);
        res.Value.StoragePlaces[0].Name.Should().Be("Сумка");
        res.Value.StoragePlaces[0].TotalVolume.Should().Be(10);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 1)]
    [InlineData("   ", 3)]
    [InlineData("Алексей", -3)]
    [InlineData("Павел", 0)]
    public void ReturnErrorWhenNameOrSpeedAreNotCorrectOnCreated(string name, int speed)
    {
        // Act
        var res = Courier.Create(name, speed, _validLocation);

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorWhenLocationIsNullOnCreated()
    {
        // Arrange
        Location location = null;

        // Act
        var res = Courier.Create(ValidName, ValidSpeed, location);

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData("Рюкзак", 4)]
    [InlineData("Багажник велосипеда", 15)]
    [InlineData("Термосумка", 8)]
    public void AddStoragePlaceWhenParamsAreCorrect(string name, int volume)
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        var initialCount = courier.StoragePlaces.Count;

        // Act
        var res = courier.AddStoragePlace(name, volume);

        // Assert
        res.IsSuccess.Should().BeTrue();
        courier.StoragePlaces.Should().HaveCount(initialCount + 1);
        courier.StoragePlaces.Should().Contain(x => x.Name == name && x.TotalVolume == volume);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 10)]
    [InlineData("   ", 10)]
    [InlineData("Рюкзак", 0)]
    [InlineData("Багажник велосипеда", -256)]
    [InlineData("Термосумка", -10)]
    public void ReturnErrorToAddStoragePlaceWhenNameOrVolumeIsInvalid(string name, int volume)
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        var initialCount = courier.StoragePlaces.Count;

        // Act
        var res = courier.AddStoragePlace(name, volume);

        // Assert
        res.IsFailure.Should().BeTrue();
        courier.StoragePlaces.Should().HaveCount(initialCount);
    }


    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void TakeOrderWhenVolumeIsSufficient(int orderVolume)
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        courier.AddStoragePlace("Багажник", 20);

        // Act
        var res = courier.CanTakeOrder(orderVolume);

        // Assert
        res.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(30)]
    [InlineData(50)]
    [InlineData(100)]
    public void ReturnErrorToCanTakeOrderWhenVolumeIsTooLarge(int orderVolume)
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        courier.AddStoragePlace("Багажник", 20);

        // Act
        var res = courier.CanTakeOrder(orderVolume);

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorToCanTakeOrderWhenAllStoragesAreOccupied()
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        var orderId = Guid.NewGuid();
        courier.TakeOrder(orderId, 10);

        // Act
        var res = courier.CanTakeOrder(5);

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TakeOrderWhenStorageIsAvailable()
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        var orderId = Guid.NewGuid();
        var orderVolume = 8;

        // Act
        var res = courier.TakeOrder(orderId, orderVolume);

        // Assert
        res.IsSuccess.Should().BeTrue();
        courier.StoragePlaces[0].OrderId.Should().Be(orderId);
        courier.StoragePlaces[0].IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void ReturnErrorToTakeOrderWhenVolumeIsTooLarge()
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        var orderId = Guid.NewGuid();
        const int orderVolume = 15;

        // Act
        var res = courier.TakeOrder(orderId, orderVolume);

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorToTakeOrderWhenAllStoragesAreOccupied()
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        var firstOrderId = Guid.NewGuid();
        courier.TakeOrder(firstOrderId, 10);
        var secondOrderId = Guid.NewGuid();

        // Act
        var res = courier.TakeOrder(secondOrderId, 5);

        // Assert
        res.IsFailure.Should().BeTrue();
        res.Error.Code.Should().Be(Courier.Errors.AllStoragesOccupiedOrNotEnoughVolume().Code);
    }

    [Fact]
    public void CompleteOrderWhenOrderExists()
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        var orderId = Guid.NewGuid();
        courier.TakeOrder(orderId, 8);

        // Act
        var res = courier.CompleteOrder(orderId);

        // Assert
        res.IsSuccess.Should().BeTrue();
        courier.StoragePlaces[0].IsEmpty.Should().BeTrue();
        courier.StoragePlaces[0].OrderId.Should().BeNull();
    }

    [Fact]
    public void ReturnErrorToCompleteOrderWhenOrderDoesNotExist()
    {
        // Arrange
        var courier = Courier.Create(ValidName, ValidSpeed, _validLocation).Value;
        var orderId = Guid.NewGuid();

        // Act
        var res = courier.CompleteOrder(orderId);

        // Assert
        res.IsFailure.Should().BeTrue();
        res.Error.Code.Should().Be(Courier.Errors.OrderIsNotFindInStoragePlaces().Code);
    }

    [Theory]
    [InlineData(1, 1, 5, 5, 2, 4)]
    [InlineData(1, 1, 5, 6, 2, 5)]
    public void CalculateTicsToDestinationCorrectly(int startX, int startY, int targetX, int targetY, int speed,
        int expectedTics)
    {
        // Arrange
        var startLocation = Location.Create(startX, startY).Value;
        var targetLocation = Location.Create(targetX, targetY).Value;
        var courier = Courier.Create(ValidName, speed, startLocation).Value;

        // Act
        var res = courier.TicsToDestination(targetLocation);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(expectedTics);
    }

    [Fact]
    public void StopAtDestinationWhenReachedAndDoAsMuchStepsAsCalculated()
    {
        // Arrange
        var startLocation = Location.Create(1, 1).Value;
        var targetLocation = Location.Create(5, 6).Value;
        var courier = Courier.Create(ValidName, 2, startLocation).Value;

        // Act
        var calculatedSteps = courier.TicsToDestination(targetLocation).Value;
        var actualSteps = 0;
        Result<bool, Error> reachDestinationRes;
        do
        {
            reachDestinationRes = courier.MoveTowardsLocation(targetLocation);
            actualSteps++;
        } while (!reachDestinationRes.Value);

        // Assert
        reachDestinationRes.IsSuccess.Should().BeTrue();
        courier.Location.X.Should().Be(targetLocation.X);
        courier.Location.Y.Should().Be(targetLocation.Y);
        actualSteps.Should().Be(calculatedSteps);
    }
}