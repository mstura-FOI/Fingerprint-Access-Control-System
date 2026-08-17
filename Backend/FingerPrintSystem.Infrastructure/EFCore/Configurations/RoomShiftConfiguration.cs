using FingerPrintSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FingerPrintSystem.Infrastructure.EFCore.Configurations;

public class RoomShiftConfiguration : IEntityTypeConfiguration<RoomShift>
{
    public void Configure(EntityTypeBuilder<RoomShift> b)
    {
        b.HasOne(x => x.Room).WithMany(r => r.RoomShifts)
            .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Shift).WithMany(s => s.RoomShifts)
            .HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Manager).WithMany(u => u.ManagedRoomShifts)
            .HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.Restrict);
    }
}