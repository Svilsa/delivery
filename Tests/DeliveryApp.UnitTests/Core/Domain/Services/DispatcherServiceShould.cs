using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services.DispatcherService;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.Services;

public class DispatcherServiceShould
{
    private readonly DispatcherService _service = new();

    private readonly Courier[] _validCouriers =
    [
        Courier.Create("Алексей", 5, Location.Create(1, 1).Value).Value,
        Courier.Create("Павел", 3, Location.Create(5, 5).Value).Value,
        Courier.Create("Мария", 2, Location.Create(10, 10).Value).Value
    ];

    private readonly Order _validOrder = Order.Create(Guid.CreateVersion7(), Location.Create(5, 3).Value, 10).Value;

    [Fact]
    public void ReturnErrorWhenOrderIsNull()
    {
        // Arrange

        // Act
        var result = _service.DispatchOrder(null!, _validCouriers);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorWhenCouriersCollectionIsNull()
    {
        // Arrange

        // Act
        var result = _service.DispatchOrder(_validOrder, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorWhenCouriersCollectionIsEmpty()
    {
        // Arrange

        // Act
        var result = _service.DispatchOrder(_validOrder, new List<Courier>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorWhenOrderStatusIsNotCreated()
    {
        // Arrange
        _validOrder.AssignToCourier(Guid.CreateVersion7());

        // Act
        var result = _service.DispatchOrder(_validOrder, new List<Courier>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DispatcherService.Errors.WrongOrderStatus().Code);
    }

    [Fact]
    public void ReturnErrorWhenNoCourierCanTakeOrder()
    {
        // Arrange
        foreach (var courier in _validCouriers)
            courier.TakeOrder(Guid.CreateVersion7(), 1);

        // Act
        var result = _service.DispatchOrder(_validOrder, _validCouriers);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DispatcherService.Errors.NoCourierFound().Code);
    }

    [Fact]
    public void SelectCourierWithMinimumTics()
    {
        // Arrange

        // Act
        var result = _service.DispatchOrder(_validOrder, _validCouriers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(_validCouriers[1]);
        result.Value.StoragePlaces.Any(sp => sp.OrderId == _validOrder.Id).Should().BeTrue();
        _validOrder.Status.Should().Be(OrderStatus.Assigned);
    }
}