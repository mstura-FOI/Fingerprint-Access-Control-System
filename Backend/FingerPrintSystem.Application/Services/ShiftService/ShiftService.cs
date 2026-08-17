using System;
using System.Collections.Generic;
using System.Text;
using FingerPrintSystem.Application.Services.ShiftService.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;

namespace FingerPrintSystem.Application.Services.ShiftService {
    public class ShiftService(ApplicationDbContext context)
        : FxServiceBaseCrud<Shift, ShiftGetDto, ShiftCreateDto, ShiftUpdateDto, ShiftDeleteDto, ApplicationDbContext>(context), IShiftService {
        protected override IQueryable<ShiftGetDto> ProjectToGet(IQueryable<Shift> q)
            => q.Select(s => new ShiftGetDto {
                Id = s.Id,
                Name = s.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            });

        protected override Shift CreateEntity(ShiftCreateDto dto)
            => new() { Name = dto.Name, StartTime = dto.StartTime, EndTime = dto.EndTime };

        protected override void UpdateEntity(Shift entity, ShiftUpdateDto dto) {
            entity.Name = dto.Name;
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
        }
    }
}
