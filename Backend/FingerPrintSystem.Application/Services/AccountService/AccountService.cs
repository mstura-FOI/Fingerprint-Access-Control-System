using FingerPrintSystem.Application.Services.AccountService.Dtos;
using FingerPrintSystem.Base.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.Application.Services.AccountService;

public class AccountService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context)
    : FxServiceBaseCrud<ApplicationUser, AccountGetDto, AccountCreateDto,
        AccountUpdateDto, AccountDeleteDto, ApplicationDbContext>(context), IAccountService {
    private static readonly string[] AllowedRoles = [Roles.User];

    protected override IQueryable<AccountGetDto> ProjectToGet(IQueryable<ApplicationUser> query)
        => query.Select(u => new AccountGetDto {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            UserName = u.UserName ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow,
            Roles = new List<string>()
        });

    public override async Task<AccountGetDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) {
        var user = await userManager.FindByIdAsync(id.ToString());
        return user is null ? null : await MapAsync(user);
    }

    public override async Task<PagedList<AccountGetDto>> GetListAsync(
        PageRequest request, CancellationToken cancellationToken = default) {
        var query = userManager.Users.AsNoTracking().OrderBy(u => u.Email);
        var total = await query.CountAsync(cancellationToken);
        var users = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync(cancellationToken);

        var items = new List<AccountGetDto>(users.Count);
        foreach (var u in users) items.Add(await MapAsync(u));

        return new PagedList<AccountGetDto> {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        };
    }

    // ---------------------------------------------------------------
    //  CREATE  (kroz UserManager — hashiranje, normalizacija)
    // ---------------------------------------------------------------
    public override async Task<AccountGetDto> CreateAsync(
        AccountCreateDto dto, CancellationToken cancellationToken = default) {

        var user = new ApplicationUser {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmailConfirmed = true,
            MustChangePassword = true 
        };

        var created = await userManager.CreateAsync(user, dto.Password);
        if (!created.Succeeded) throw IdentityError(created);

        var roleAdded = await userManager.AddToRoleAsync(user, Roles.User);
        if (!roleAdded.Succeeded) throw IdentityError(roleAdded);

        return await MapAsync(user);
    }

    protected override ApplicationUser CreateEntity(AccountCreateDto dto)
    {
        throw new NotImplementedException();
    }

    // ---------------------------------------------------------------
    //  UPDATE  (kroz UserManager; admin meta = zabranjeno)
    // ---------------------------------------------------------------
    public override async Task<bool> UpdateAsync(
        AccountUpdateDto dto, CancellationToken cancellationToken = default) {
        var user = await userManager.FindByIdAsync(dto.Id.ToString());
        if (user is null) return false;

        await GuardNotAdmin(user, "Changing");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase)) {
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    // ---------------------------------------------------------------
    //  DELETE  (kroz UserManager; admin meta = zabranjeno)
    // ---------------------------------------------------------------
    public override async Task<bool> DeleteAsync(
        AccountDeleteDto dto, CancellationToken cancellationToken = default) {
        var user = await userManager.FindByIdAsync(dto.Id.ToString());
        if (user is null) return false;

        await GuardNotAdmin(user, "Delete");

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    protected override void UpdateEntity(ApplicationUser entity, AccountUpdateDto dto)
    {
        throw new NotImplementedException();
    }

    // ---------------------------------------------------------------
    //  Dodatne operacije izvan CRUD-a
    // ---------------------------------------------------------------
    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto) {
        var user = await userManager.FindByIdAsync(dto.Id.ToString());
        if (user is null) return NotFound();

        if (await IsAdmin(user))
            return IdentityResult.Failed(new IdentityError { Description = "Administratorska lozinka se ne može resetirati ovdje." });

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (result.Succeeded) {
            user.MustChangePassword = true;
            await userManager.UpdateAsync(user);
        }
        return result;
    }

    public async Task<IdentityResult> SetActiveAsync(Guid id, bool active) {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        if (await IsAdmin(user))
            return IdentityResult.Failed(new IdentityError { Description = "Administratorski računi se ne mogu (de)aktivirati ovdje." });

        // Deaktivacija = lockout u daleku budućnost (login provjerava IsLockedOutAsync).
        user.LockoutEnd = active ? null : DateTimeOffset.MaxValue;
        return await userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> ChangeOwnPasswordAsync(Guid userId, ChangePasswordDto dto) {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (result.Succeeded) {
            user.MustChangePassword = false;
            await userManager.UpdateAsync(user);
        }
        return result;
    }

    // ---------------------------------------------------------------
    //  Helpers
    // ---------------------------------------------------------------
    private async Task<AccountGetDto> MapAsync(ApplicationUser u) => new() {
        Id = u.Id,
        Email = u.Email ?? string.Empty,
        UserName = u.UserName ?? string.Empty,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Roles = await userManager.GetRolesAsync(u),
        IsActive = u.LockoutEnd is null || u.LockoutEnd <= DateTimeOffset.UtcNow
    };

    private async Task<bool> IsAdmin(ApplicationUser user)
        => await userManager.IsInRoleAsync(user, Roles.Administrator);

    private async Task GuardNotAdmin(ApplicationUser user, string action) {
        if (await IsAdmin(user))
            throw new InvalidOperationException($"Administratorski računi se ne mogu {action}.");
    }

    private static IdentityResult NotFound()
        => IdentityResult.Failed(new IdentityError { Description = "Korisnik nije pronađen." });

    private static InvalidOperationException IdentityError(IdentityResult r)
        => new(string.Join("; ", r.Errors.Select(e => e.Description)));
}