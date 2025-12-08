using System;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Model.OrderAggregate;

public class OrderShould
{
    private readonly Guid _validId = Guid.NewGuid();
    private readonly Location _validLocation = Location.Create(5, 3).Value;
    private readonly int _validVolume = 10;

    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange

        // Act
        var res = Order.Create(_validId, _validLocation, _validVolume);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Id.Should().Be(_validId);
        res.Value.Location.Should().Be(_validLocation);
        res.Value.Volume.Should().Be(_validVolume);
        res.Value.Status.Should().Be(OrderStatus.Created);
        res.Value.CourierId.Should().BeNull();
    }

    [Fact]
    public void ReturnErrorWhenIdIsEmptyOnCreated()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var res = Order.Create(id, _validLocation, _validVolume);

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorWhenLocationIsNullOnCreated()
    {
        // Arrange
        Location location = null;

        // Act
        var res = Order.Create(_validId, location, _validVolume);

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ReturnErrorWhenVolumeIsInvalidOnCreated(int volume)
    {
        // Act
        var res = Order.Create(_validId, _validLocation, volume);

        // Assert
        res.IsFailure.Should().BeTrue();
        res.Error.Code.Should().Be(GeneralErrors.ValueIsInvalid(nameof(volume)).Code);
    }

    [Fact]
    public void AssignToCourierWhenOrderIsCreated()
    {
        // Arrange
        var order = Order.Create(_validId, _validLocation, _validVolume).Value;
        var courierId = Guid.NewGuid();

        // Act
        var res = order.AssignToCourier(courierId);

        // Assert
        res.IsSuccess.Should().BeTrue();
        order.CourierId.Should().Be(courierId);
        order.Status.Should().Be(OrderStatus.Assigned);
    }

    [Fact]
    public void ReturnErrorToAssignWhenCourierIdIsEmpty()
    {
        // Arrange
        var order = Order.Create(_validId, _validLocation, _validVolume).Value;
        var courierId = Guid.Empty;

        // Act
        var res = order.AssignToCourier(courierId);

        // Assert
        res.IsFailure.Should().BeTrue();
        order.CourierId.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Created);
    }

    [Fact]
    public void ReturnErrorToAssignWhenOrderIsAlreadyAssigned()
    {
        // Arrange
        var order = Order.Create(_validId, _validLocation, _validVolume).Value;

        // Act
        order.AssignToCourier(Guid.NewGuid());
        var res = order.AssignToCourier(Guid.NewGuid());

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorToAssignWhenOrderIsCompeted()
    {
        // Arrange
        var order = Order.Create(_validId, _validLocation, _validVolume).Value;
        order.AssignToCourier(Guid.NewGuid());
        order.Complete();

        // Act
        var res = order.AssignToCourier(Guid.NewGuid());

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CompleteWhenOrderIsAssigned()
    {
        // Arrange
        var order = Order.Create(_validId, _validLocation, _validVolume).Value;
        order.AssignToCourier(Guid.NewGuid());

        // Act
        var res = order.Complete();

        // Assert
        res.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void ReturnErrorToCompleteWhenOrderIsNotAssigned()
    {
        // Arrange
        var order = Order.Create(_validId, _validLocation, _validVolume).Value;

        // Act
        var res = order.Complete();

        // Assert
        res.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorToCompleteWhenOrderIsAlreadyCompleted()
    {
        // Arrange
        var order = Order.Create(_validId, _validLocation, _validVolume).Value;
        order.AssignToCourier(Guid.NewGuid());
        order.Complete();

        // Act
        var res = order.Complete();

        // Assert
        res.IsFailure.Should().BeTrue();
    }
}