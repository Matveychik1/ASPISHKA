using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Security.Claims;
using WebGenQRCode.Data;
using WebGenQRCode.Data.Entities;
using WebGenQRCode.Data.Entities.Identity;
using WebGenQRCode.Models.QrCode;

namespace WebGenQRCode.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QrCodesController(AppQrDbContext appQrDbContext,
    UserManager<UserEntity> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetQrCodes()
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(email))
            return Unauthorized();
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound();
        var qrCodes = appQrDbContext.QrCodes
            .Where(q => q.UserId == user.Id)
            .Select(q => new QrCodeItemModel
            {
                Id = q.Id,
                Name = q.Name,
                TargetUrl = q.TargetUrl,
                Code = q.Code,
                IsActive = q.IsActive,
                CreateAt = q.CreateAt.ToString("dd.MM.yyyy HH:mm:ss"),
                ScanCount = q.ScanCount
            })
            .ToList();
        // Implementation for getting QR codes
        return Ok(qrCodes);
    }

    [HttpPost]
    public async Task<IActionResult> CreateQrCode([FromBody] CreateQrCodeRequest request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(); //Шукаємо email користувача в claims, якщо його немає, повертаємо 401 Unauthorized
        }
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }
        var entity = new QrCodeEntity
        {
            Name = request.Name,
            TargetUrl = request.TargetUrl,
            Code = Guid.NewGuid().ToString("N"), // Генеруємо унікальний код для QR-коду
            UserId = user.Id
        };
        appQrDbContext.QrCodes.Add(entity);
        await appQrDbContext.SaveChangesAsync();
        // Implementation for creating QR code
        return Ok();
    }

    [HttpGet("scan/{code}")]
    public async Task<IActionResult> ScanQrCode(string code)
    {
        var qrCode = await appQrDbContext.QrCodes.FirstOrDefaultAsync(q => q.Code == code);
        if (qrCode == null)
            return NotFound("QR-код не знайдено");
        if (!qrCode.IsActive)
            return BadRequest("QR-код не активний");

        qrCode.ScanCount++; //збільшуємо лічильник сканувань
        await appQrDbContext.SaveChangesAsync();

        return Redirect(qrCode.TargetUrl); // Перенаправляємо користувача на цільовий URL
    }

    [HttpPatch("change-state")]
    public async Task<IActionResult> ChangeState([FromBody] QrCodeStateModel state)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(email))
            return Unauthorized();
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound();
        var qrCode = await appQrDbContext.QrCodes
            .FirstOrDefaultAsync(q => q.Code == state.Code && q.UserId == user.Id);
        if (qrCode == null)
            return NotFound();
        qrCode.IsActive = state.IsActivate;
        appQrDbContext.SaveChanges();
        return Ok();
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQrCode(int id, [FromBody] UpdateQrCodeRequest request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(email))
            return Unauthorized();
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound();
        var qrCode = await appQrDbContext.QrCodes
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == user.Id);
        if (qrCode == null)
            return NotFound();
        qrCode.Name = request.Name;
        qrCode.TargetUrl = request.TargetUrl;
        qrCode.IsActive = request.IsActive;
        appQrDbContext.SaveChanges();
        return Ok();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteQrCode(int id)
    {
        //пошук QR-коду в базі даних (ігноруючи вже видалені, якщо активовано Global Query Filter)
        var qrCode = await appQrDbContext.QrCodes.FirstOrDefaultAsync(q => q.Id == id);

        if (qrCode == null)
        {
            return NotFound(new { message = $"QR-код з ID {id} не знайдено." });
        }

        //для м'якого видалення
        qrCode.IsDeleted = true;
        qrCode.DeletedAt = DateTime.UtcNow; //фіксує час видалення

        await qrCode.SaveChangesAsync();

        return NoContent();
    }
}
