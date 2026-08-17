using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FingerPrintSystem.Infrastructure.EFCore.Configurations;

public class AccessRightConfiguration : IEntityTypeConfiguration<AccessRight>
{
    public void Configure(EntityTypeBuilder<AccessRight> b)
    {
        b.HasIndex(x => new { x.ApplicationUserId, x.RoomId }).IsUnique();

        b.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<Room>()
            .WithMany(r => r.AccessRights)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}