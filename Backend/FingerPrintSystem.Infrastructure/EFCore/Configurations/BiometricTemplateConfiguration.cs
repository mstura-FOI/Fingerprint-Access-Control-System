using FingerPrintSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FingerPrintSystem.Infrastructure.EFCore.Configurations;

public class BiometricTemplateConfiguration : IEntityTypeConfiguration<BiometricTemplate>
{
    public void Configure(EntityTypeBuilder<BiometricTemplate> b)
    {
        b.HasIndex(x => x.ApplicationUserId);
        b.Property(x => x.KekKeyId).HasMaxLength(256);

        b.HasOne(x => x.ApplicationUser)
            .WithMany(u => u.BiometricTemplates)
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}