using FingerPrintSystem.Application.Services.DeviceService.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;

namespace FingerPrintSystem.Application.Services.DeviceService;

public class DeviceService(ApplicationDbContext context)
    : FxServiceBaseSnapshot<Device, DeviceGetDto, DeviceCreateDto, ApplicationDbContext>(context)
{
    protected override IQueryable<DeviceGetDto> ProjectToGet(IQueryable<Device> q)
    {
        return q.Select(d => new DeviceGetDto
        {
            Id = d.Id,
            Name = d.Name,
            SerialNumber = d.SerialNumber,
            IpAddress = d.IpAddress,
            RoomId = d.RoomId,
            RoomName = d.Room.Name
        });
    }

    protected override Device CreateEntity(DeviceCreateDto dto)
    {
        return new Device
        {
            Name = dto.Name,
            SerialNumber = dto.SerialNumber,
            IpAddress = dto.IpAddress,
            CertThumbprint = dto.CertThumbprint,
            RoomId = dto.RoomId,
            Room = null!
        };
    }
}