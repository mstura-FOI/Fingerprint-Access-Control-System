using System;
using System.Collections.Generic;
using System.Text;
using FingerPrintSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FingerPrintSystem.Infrastructure.EFCore.Configurations {
    public class DeviceConfiguration : IEntityTypeConfiguration<Device> {
        public void Configure(EntityTypeBuilder<Device> b) {
            b.HasIndex(x => x.CertThumbprint).IsUnique(); 
            b.HasIndex(x => x.SerialNumber).IsUnique();

            b.HasOne(x => x.Room)
                .WithMany(r => r.Devices)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
