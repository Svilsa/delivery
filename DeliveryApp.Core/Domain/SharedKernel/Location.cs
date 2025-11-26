using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.SharedKernel;

public sealed class Location : ValueObject
{
    private const int Min = 1;
    private const int Max = 10;

    [ExcludeFromCodeCoverage]
    private Location(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }
    public int Y { get; }

    public static Result<Location, Error> Create(int x, int y)
    {
        if (x is < Min or > Max)
            return new Error("value.is.out.of.range", $"X must be between {Min} and {Max}");

        if (y is < Min or > Max)
            return new Error("value.is.out.of.range", $"Y must be between {Min} and {Max}");

        return new Location(x, y);
    }

    /// <summary>
    ///     Манхэттенское расстояние между двумя точками.
    /// </summary>
    public int DistanceTo(Location other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    }

    public static Location CreateRandom()
    {
        var x = Random.Shared.Next(Min, Max + 1);
        var y = Random.Shared.Next(Min, Max + 1);

        return new Location(x, y);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}