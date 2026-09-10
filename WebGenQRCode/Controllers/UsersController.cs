using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGenQRCode.Data;
using WebGenQRCode.Models.Users;

namespace WebGenQRCode.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(AppQrDbContext appQrDbContext) : ControllerBase
{
    [HttpGet]
    [Authorize]/**/
    public async Task<IActionResult> GetUsers()
    {
        var users = await appQrDbContext.Users
            .Select(x=> new UserItemModel
            {
                Id = x.Id,
                FullName = $"{x.LastName} {x.FirstName}",
                Email = x.Email,
                Image = x.Image
            }).ToListAsync();

        return Ok(users);
    }
}
