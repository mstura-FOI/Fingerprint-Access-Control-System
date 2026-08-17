using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FingerPrintSystem.Infrastructure.EFCore;

public static class ConfigurationSeeder {

    public static readonly Guid SystemActor = new("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(IServiceProvider services) {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var opts = services.GetRequiredService<IOptions<FingerPrintSystemOptions>>().Value;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        // --- Prostorije (prve, jer Device FK ovisi o njima) ---
        foreach (var r in opts.Rooms) {
            if (await db.Rooms.AnyAsync(x => x.Id == r.Id))
                continue; 

            db.Rooms.Add(new Room {
                Id = r.Id,
                Name = r.Name,
                CreatedBy = SystemActor, 
            });
        }

        // --- Uređaji ---
        foreach (var d in opts.Devices) {
            if (await db.Devices.AnyAsync(x => x.Id == d.Id))
                continue;

            if (string.IsNullOrWhiteSpace(d.CertThumbprint)) {
                logger.LogWarning(
                    "Seed: preskačem uređaj {Serial} — nema CertThumbprint. Device je immutable, " +
                    "pa ga se ne može dodati kasnije; postavi thumbprint u appsettings.", d.SerialNumber);
                continue;
            }

            var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == d.RoomId);
            if (room is null) {
                logger.LogWarning("Seed: preskačem uređaj {Serial} — prostorija {RoomId} ne postoji.",
                    d.SerialNumber, d.RoomId);
                continue;
            }

            db.Devices.Add(new Device {
                Id = d.Id,
                Name = d.Name,
                SerialNumber = d.SerialNumber,
                IpAddress = d.IpAddress,
                CertThumbprint = d.CertThumbprint,
                Room = room,                       
                CreatedBy = SystemActor,
            });
        }

        await db.SaveChangesAsync();
    }
}