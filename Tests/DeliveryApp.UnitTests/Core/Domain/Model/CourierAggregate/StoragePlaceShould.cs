using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Model.CourierAggregate;

public class StoragePlaceShould
{
    [Theory]
    [InlineData("Рюкзак", 10)]
    [InlineData("Багажник велосипеда", 15)]
    [InlineData("Багажник машины", 30)]
    public void BeCorrectWhenParamsAreCorrectOnCreated(string name, int totalVolume)
    {
        // Act
        var res = StoragePlace.Create(name, totalVolume);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().NotBeNull();
        res.Value.Name.Should().Be(name);
        res.Value.TotalVolume.Should().Be(totalVolume);
        res.Value.IsEmpty.Should().BeTrue();
    }

    [Theory]
    [InlineData(null, 5)]
    [InlineData("", 5)]
    [InlineData(" ", 5)]
    [InlineData("Коробка", -5)]
    public void ReturnErrorWhenParamsAreNotCorrectOnCreated(string name, int totalVolume)
    {
        // Act
        var res = StoragePlace.Create(name, totalVolume);

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorWhenCanStoreTooLargeOrder()
    {
        // Arrange
        var sp = StoragePlace.Create("Багажник", 35).Value;

        // Act
        var canPlace = sp.CanStore(40);

        // Assert
        canPlace.IsSuccess.Should().BeFalse();
        canPlace.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnErrorWhenStoreTooLargeOrder()
    {
        // Arrange
        var sp = StoragePlace.Create("Конверт", 2).Value;
        var orderId = Guid.CreateVersion7();
        const int orderVolume = 3;

        // Act
        var result = sp.StoreOrder(orderId, orderVolume);

        // Assert
        result.IsFailure.Should().BeTrue();
        sp.IsEmpty.Should().BeTrue();
        sp.OrderId.Should().BeNull();
    }

    [Fact]
    public void PlaceOrderWhenEmptyAndVolumeFits()
    {
        // Arrange
        var sp = StoragePlace.Create("Рюкзак", 8).Value;
        var orderId = Guid.CreateVersion7();
        const int orderVolume = 5;

        // Act
        var result = sp.StoreOrder(orderId, orderVolume);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sp.IsEmpty.Should().BeFalse();
        sp.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void ReturnErrorWhenPlaceOrderWhenNotEmpty()
    {
        // Arrange
        var sp = StoragePlace.Create("Рюкзак", 8).Value;

        var firstOrder = Guid.CreateVersion7();
        const int firstOrderVolume = 5;

        var secondOrderId = Guid.CreateVersion7();
        const int secondOrderVolume = 3;

        // Act
        sp.StoreOrder(firstOrder, firstOrderVolume);
        var result = sp.StoreOrder(secondOrderId, secondOrderVolume);

        // Assert
        result.IsFailure.Should().BeTrue();
        sp.OrderId.Should().Be(firstOrder);
    }
}