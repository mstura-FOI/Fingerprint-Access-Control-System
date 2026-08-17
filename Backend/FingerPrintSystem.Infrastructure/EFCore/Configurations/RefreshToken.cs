using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FingerPrintSystem.Infrastructure.EFCore.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken> {
    public void Configure(EntityTypeBuilder<RefreshToken> b) {
        b.HasIndex(x => x.TokenHash).IsUnique(); 
        b.HasIndex(x => x.ApplicationUserId); 
        b.Property(x => x.TokenHash).HasMaxLength(128);

        b.HasOne(x => x.ApplicationUser)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}