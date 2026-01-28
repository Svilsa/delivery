using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class OrderRepositoryShould : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("courier")
        .WithUsername("username")
        .WithPassword("secret")
        .WithCleanUp(true)
        .Build();

    private ApplicationDbContext _context;

    public async Task InitializeAsync()
    {
        //Стартуем БД (библиотека TestContainers запускает Docker контейнер с Postgres)
        await _postgreSqlContainer.StartAsync();

        //Накатываем миграции и справочники
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); })
            .EnableSensitiveDataLogging().Options;

        _context = new ApplicationDbContext(contextOptions);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task CanAddOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 4).Value;

        // Act
        var orderRepository = new OrderRepository(_context);
        await orderRepository.AddAsync(order);
        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var orderFromDb = await orderRepository.GetByIdAsync(order.Id);
        orderFromDb.Should().NotBeNull();
        orderFromDb.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task CanUpdateOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 4).Value;

        // Act
        var orderRepository = new OrderRepository(_context);
        await orderRepository.AddAsync(order);

        var courierId = Guid.NewGuid();
        order.AssignToCourier(courierId);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var orderFromDb = await orderRepository.GetByIdAsync(order.Id);
        orderFromDb.Should().NotBeNull();
        orderFromDb.Should().BeEquivalentTo(order);
        orderFromDb!.Status.Should().Be(OrderStatus.Assigned);
        orderFromDb!.CourierId.Should().Be(courierId);
    }

    [Fact]
    public async Task CanGetAllAssignedOrders()
    {
        // Arrange
        var order1 = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 2).Value;
        var order2 = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 3).Value;
        var order3 = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 1).Value;

        var orderRepository = new OrderRepository(_context);
        await orderRepository.AddAsync(order1);
        await orderRepository.AddAsync(order2);
        await orderRepository.AddAsync(order3);

        // Присвоим курьеров только первым двум заказам
        order1.AssignToCourier(Guid.NewGuid());
        order2.AssignToCourier(Guid.NewGuid());

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Act
        var assignedOrders = await orderRepository.GetAllAssignedOrdersAsync();

        // Assert
        assignedOrders.Should().HaveCount(2);
        assignedOrders.Should().ContainEquivalentOf(order1);
        assignedOrders.Should().ContainEquivalentOf(order2);
        assignedOrders.Should().NotContain(order3);
    }

    [Fact]
    public async Task CanGetFirstCreatedOrder()
    {
        // Arrange
        var order1 = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 2).Value;
        var order2 = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 3).Value;
        var order3 = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 1).Value;

        var orderRepository = new OrderRepository(_context);
        await orderRepository.AddAsync(order1);
        await orderRepository.AddAsync(order2);
        await orderRepository.AddAsync(order3);

        // Присвоим курьеров только order2 и order3, чтобы order1 остался в статусе Created
        order2.AssignToCourier(Guid.NewGuid());
        order3.AssignToCourier(Guid.NewGuid());

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Act
        var firstCreatedOrder = orderRepository.GetFirstCreatedOrder();

        // Assert
        firstCreatedOrder.Should().NotBeNull();
        firstCreatedOrder!.Id.Should().Be(order1.Id);
        firstCreatedOrder.Status.Should().Be(OrderStatus.Created);
    }
}