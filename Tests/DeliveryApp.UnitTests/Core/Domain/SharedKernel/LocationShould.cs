using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Core.Domain.SharedKernel;

public class LocationShould
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 7)]
    [InlineData(10, 10)]
    public void BeCorrectWhenParamsAreCorrectOnCreated(int x, int y)
    {
        // Arrange

        // Act
        var location = Location.Create(x, y);

        // Assert
        location.IsSuccess.Should().BeTrue();
        location.Value.X.Should().Be(x);
        location.Value.Y.Should().Be(y);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(11, 5)]
    [InlineData(5, 0)]
    [InlineData(5, 11)]
    public void ReturnErrorWhenParamsAreNotCorrectOnCreated(int x, int y)
    {
        // Arrange

        // Act
        var location = Location.Create(x, y);

        // Assert
        location.IsSuccess.Should().BeFalse();
        location.Error.Should().NotBeNull();
    }

    [Fact]
    public void BeEqualWhenAllPropertiesIsEqual()
    {
        // Arrange
        var l1 = Location.Create(3, 4).Value;
        var l2 = Location.Create(3, 4).Value;

        // Act
        var areEqual = l1.Equals(l2);

        // Assert
        areEqual.Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenOneOfPropertiesIsNotEqual()
    {
        // Arrange
        var l1 = Location.Create(3, 4).Value;
        var l2 = Location.Create(3, 5).Value;

        // Act
        var areEqual = l1.Equals(l2);

        // Assert
        areEqual.Should().BeFalse();
    }

    [Fact]
    public void ReturnCorrectManhattanDistance()
    {
        // Arrange
        var from = Location.Create(2, 3).Value;
        var to = Location.Create(5, 1).Value;

        // Act
        var distance = from.DistanceTo(to);

        // Assert
        distance.Should().Be(5, "|5-2| + |1-3| = 3 + 2 = 5");
    }
}