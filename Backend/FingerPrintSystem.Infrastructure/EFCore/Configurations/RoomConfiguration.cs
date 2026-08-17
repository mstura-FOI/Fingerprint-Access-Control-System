using FingerPrintSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FingerPrintSystem.Infrastructure.EFCore.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable($"{nameof(Room)}s");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasMany(x => x.Devices)
            .WithOne(x => x.Room)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.RoomShifts)
            .WithOne(x => x.Room)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}