using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class CourierRepositoryShould : IAsyncLifetime
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
    public async Task CanAddCourier()
    {
        // Arrange
        var courier = Courier.Create("Вася", 3, Location.CreateRandom()).Value;
        courier.TakeOrder(Guid.NewGuid(), 1);

        // Act
        var courierRepository = new CourierRepository(_context);
        await courierRepository.AddAsync(courier);
        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var courierFromDb = await courierRepository.GetByIdAsync(courier.Id);
        courierFromDb.Should().NotBeNull();
        courier.Should().BeEquivalentTo(courierFromDb);
    }

    [Fact]
    public async Task CanUpdateCourier()
    {
        // Arrange
        var courier = Courier.Create("Вася", 3, Location.CreateRandom()).Value;

        // Act
        var courierRepository = new CourierRepository(_context);
        await courierRepository.AddAsync(courier);

        courier.AddStoragePlace("Пакет-майка", 1);
        courier.TakeOrder(Guid.NewGuid(), 1);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var courierFromDb = await courierRepository.GetByIdAsync(courier.Id);
        courierFromDb.Should().NotBeNull();
        courierFromDb.Should().BeEquivalentTo(courier);
        courierFromDb?.StoragePlaces.SingleOrDefault(sp => sp.Name == "Пакет-майка").Should().NotBeNull();
    }

    [Fact]
    public async Task CanGetAllAvailableCouriers()
    {
        // Arrange
        var availableCourier = Courier.Create("Свободный", 2, Location.CreateRandom()).Value;
        availableCourier.AddStoragePlace("Рюкзак", 5);

        var unavailableCourier = Courier.Create("Занятый", 2, Location.CreateRandom()).Value;
        unavailableCourier.AddStoragePlace("Коробка", 5);
        unavailableCourier.TakeOrder(Guid.NewGuid(), 1);

        var courierRepository = new CourierRepository(_context);
        await courierRepository.AddAsync(availableCourier);
        await courierRepository.AddAsync(unavailableCourier);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Act
        var result = await courierRepository.GetAllAvailableCouriersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(availableCourier.Id);
    }
}