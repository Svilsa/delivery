using CSharpFunctionalExtensions;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public sealed class OrderStatus : ValueObject
{
    public static OrderStatus Created = new(nameof(Created));
    public static OrderStatus Assigned = new(nameof(Assigned));
    public static OrderStatus Completed = new(nameof(Completed));
    
    private OrderStatus(string name)
    {
        Name = name;
    }

    public string Name { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}