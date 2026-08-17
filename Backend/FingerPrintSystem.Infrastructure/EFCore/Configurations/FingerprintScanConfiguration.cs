using System;
using System.Collections.Generic;
using System.Text;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FingerPrintSystem.Infrastructure.EFCore.Configurations {
    public class FingerprintScanConfiguration : IEntityTypeConfiguration<FingerprintScan> {
        public void Configure(EntityTypeBuilder<FingerprintScan> b) {
            b.HasIndex(x => x.ScanTime);
            b.HasIndex(x => new { x.DeviceId, x.ScanTime });
            b.Property(x => x.DenialReason).HasMaxLength(256);

            b.HasOne<ApplicationUser>()                        
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<Device>()
                .WithMany(d => d.FingerprintScans)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
