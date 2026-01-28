using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate;

public class CourierEntityTypeConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable("couriers");

        entityTypeBuilder.HasKey(entity => entity.Id);

        entityTypeBuilder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .IsRequired();

        entityTypeBuilder
            .Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(128)
            .IsRequired();

        entityTypeBuilder
            .Property(entity => entity.Speed)
            .HasColumnName("speed")
            .IsRequired();

        entityTypeBuilder
            .OwnsOne(entity => entity.Location, l =>
            {
                l.Property(o => o.X)
                    .HasColumnName("location_x")
                    .IsRequired();
                l.Property(o => o.Y)
                    .HasColumnName("location_y")
                    .IsRequired();
                l.WithOwner();
            });

        entityTypeBuilder.Navigation(entity => entity.Location).IsRequired();
    }
}