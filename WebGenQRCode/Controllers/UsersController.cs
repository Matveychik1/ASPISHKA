using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetUsers([FromQuery] UserSearchModel model)
    {
        // Захист від некоректних значень
        var page = Math.Max(model.Page, 1);
        var pageSize = Math.Clamp(model.PageSize, 1, 100);

        var query = appQrDbContext.Users.AsNoTracking();

        // Фільтри застосовуються лише якщо поле не пусте
        if (!string.IsNullOrWhiteSpace(model.FirstName))
        {
            var firstName = model.FirstName.Trim().ToLower();
            query = query.Where(x => x.FirstName.ToLower().Contains(firstName));
        }

        if (!string.IsNullOrWhiteSpace(model.LastName))
        {
            var lastName = model.LastName.Trim().ToLower();
            query = query.Where(x => x.LastName.ToLower().Contains(lastName));
        }

        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            var email = model.Email.Trim().ToLower();
            query = query.Where(x => x.Email.ToLower().Contains(email));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(x => x.Id) // обов'язкове сортування для стабільної пагінації
            .Skip((model.Page - 1) * model.PageSize)
            .Take(model.PageSize)
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
