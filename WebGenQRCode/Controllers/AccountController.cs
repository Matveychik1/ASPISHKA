using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebGenQRCode.Constans;
using WebGenQRCode.Data.Entities.Identity;
using WebGenQRCode.Interfaces;
using WebGenQRCode.Models.Account;

namespace WebGenQRCode.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AccountController(IImageService imageService,
    UserManager<UserEntity> userManager,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
                throw new Exception("Дана пошта уже зареєстрована");
            user = new UserEntity
            {
                Email = model.Email,
                UserName = model.Email,
                LastName = model.LastName,
                FirstName = model.FirstName,
            };
            if (model.ImageFile != null)
                user.Image = await imageService.SaveOptimizedImageAsync(model.ImageFile);
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }
            await userManager.AddToRoleAsync(user, Roles.User);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
    //метод для авторизування
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email); //асинхронно шукає юзера в базі даних за його email
        if (user != null && await userManager.CheckPasswordAsync(user, model.Password)) //перевірка паролю
        {
            //створення токену і повернення
            var token = await jwtTokenService.CreateTokenAsync(user);
            return Ok(new { Token = token });
        }

        return Unauthorized("Не вірно вказано дані");
    }
}
