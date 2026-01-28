using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports;

public interface ICourierRepository : IRepository<Courier, Guid>;