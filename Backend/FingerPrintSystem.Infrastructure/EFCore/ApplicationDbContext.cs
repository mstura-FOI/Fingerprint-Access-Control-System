using System.Reflection;
using FingerPrintSystem.Base.Entities;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.Infrastructure.EFCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options) {
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<RoomShift> RoomShifts => Set<RoomShift>();
    public DbSet<AccessRight> AccessRights => Set<AccessRight>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<FingerprintScan> FingerprintScans => Set<FingerprintScan>();
    public DbSet<BiometricTemplate> BiometricTemplates => Set<BiometricTemplate>();
    public DbSet<OneTimeCode> OneTimeCodes => Set<OneTimeCode>();
    public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default) {
        EnforceSnapshotImmutability();
        StampAudit();
        return base.SaveChangesAsync(ct);
    }

    public override int SaveChanges() {
        EnforceSnapshotImmutability();
        StampAudit();
        return base.SaveChanges();
    }

    private void EnforceSnapshotImmutability() {
        foreach (var entry in ChangeTracker.Entries()) {
            if (entry.Entity is IAuditableSnapshot<Guid> &&
                entry.State is EntityState.Modified or EntityState.Deleted) {
                throw new InvalidOperationException(
                    $"'{entry.Entity.GetType().Name}' je append-only (snapshot) i ne smije se mijenjati ni brisati.");
            }
        }
    }
    private void StampAudit() {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries()) {
            if (entry.State == EntityState.Added) {
                if (entry.Entity is IAuditableSnapshot<Guid> snap) snap.CreatedAt = now;
                else if (entry.Entity is IAuditable aud) aud.CreatedAt = now;
            } else if (entry.State == EntityState.Modified && entry.Entity is IAuditable aud2) {
                aud2.ModifiedAt = now;
            }
        }
    }
}