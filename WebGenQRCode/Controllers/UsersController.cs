using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGenQRCode.Data;
using WebGenQRCode.Models;
using WebGenQRCode.Models.Users;

namespace WebGenQRCode.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(AppQrDbContext appQrDbContext) : ControllerBase
{
    [HttpGet]
    [Authorize] // Доступ до даних можна лише авторизованому користувачу
    public async Task<IActionResult> GetUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        // Захист від некоректних значень
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = appQrDbContext.Users.AsNoTracking();

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(x => x.Id) // обов'язкове сортування для стабільної пагінації
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserItemModel
            {
                Id = x.Id,
                FullName = $"{x.LastName} {x.FirstName}",
                Email = x.Email,
                Image = x.Image
            })
            .ToListAsync();

        var result = new PagedResult<UserItemModel>
        {
            Items = users,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(result);
    }
}
