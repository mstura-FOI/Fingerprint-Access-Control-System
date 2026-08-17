using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FingerPrintSystem.Infrastructure.EFCore.Configurations;

public class OneTimeCodeConfiguration : IEntityTypeConfiguration<OneTimeCode>
{
    public void Configure(EntityTypeBuilder<OneTimeCode> b)
    {
        b.HasIndex(x => x.CodeHash);
        b.HasIndex(x => x.ExpiresAt);
        b.HasIndex(x => new { x.ApplicationUserId, x.Purpose });
        b.Property(x => x.CodeHash).HasMaxLength(128);

        b.HasOne<ApplicationUser>()
            .WithMany(u => u.OneTimeCodes)
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}