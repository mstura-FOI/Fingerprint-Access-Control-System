using FingerPrintSystem.Application.Services.RoomService;
using FingerPrintSystem.Application.Services.RoomService.Dtos;
using FingerPrintSystem.Base.Controllers;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;


namespace FingerPrintSystem.WebApi.Controllers;

public class RoomController(RoomService service)
    : FxControllerBaseSnapshot<Room, RoomGetDto, RoomService, ApplicationDbContext>(service);