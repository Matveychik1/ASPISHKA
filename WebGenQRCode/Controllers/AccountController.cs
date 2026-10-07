using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebGenQRCode.Constans;
//using WebGenQRCode.Constants;
using WebGenQRCode.Data.Entities.Identity;
using WebGenQRCode.Interfaces;
using WebGenQRCode.Models.Account;
using WebGenQRCode.Models.Users;

namespace WebGenQRCode.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AccountController(IImageService imageService,
    UserManager<UserEntity> userManager,
    IAccountService accountService,
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
                FirstName = model.FirstName
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

            var token = await jwtTokenService.CreateTokenAsync(user);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }

    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
        {
            var token = await jwtTokenService.CreateTokenAsync(user);
            return Ok(new { Token = token });
        }

        return Unauthorized("Не вірно вказано дані");
    }

    //private readonly UserManager userManager;
    //public class UsersController(UserManager userManager)
    //{
    //    userManager = userManager;
    //}

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserItemModel>>> GetUsers()
    {
        // 1. Отримуємо всіх користувачів із бази даних
        var users = await userManager.Users.ToListAsync();
        var userDtos = new List<UserItemModel>();

        foreach (var user in users)
        {
            // 2. Отримуємо список зовнішніх логінів для кожного користувача
            var logins = await userManager.GetLoginsAsync(user);

            // 3. Визначаємо спосіб реєстрації
            string method = "Password"; // Значення за замовчуванням (звичайна форма)

            // Перевіряємо, чи є серед логінів Google
            if (logins.Any(l => l.LoginProvider.Equals("Google", StringComparison.OrdinalIgnoreCase)))
            {
                method = "Google";
            }
            // Якщо використовуєте інші соцмережі, можна додати умови:
            // else if (logins.Any(l => l.LoginProvider.Equals("Facebook", ...))) { method = "Facebook"; }

            userDtos.Add(new UserItemModel
            {
                Email = user.Email,
                FullName = user.FirstName + " " + user.LastName,
                RegistrationMethod = method
            });
        }

        return Ok(userDtos);
    }
        //[HttpPost]
        //public async Task<IActionResult> LoginByGoogle([FromBody] GoogleLoginRequestModel model)
        //{
        //    try
        //    {
        //        var token = await accountService.LoginByGoogle(model.Token);
        //        return Ok(new { Token = token });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { Error = ex.Message });
        //    }
        //}

        //[Authorize]
        //[HttpGet]
        //public async Task<IActionResult> Profile()
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email)
        //        ?? User.FindFirstValue("email");
        //    if (string.IsNullOrEmpty(email))
        //        return Unauthorized("Email not found in token");

        //    var user = await userManager.FindByEmailAsync(email);
        //    if (user == null)
        //        return NotFound("User not found");
        //    var roles = await userManager.GetRolesAsync(user);
        //    var model = new ProfileModel
        //    {
        //        Id = user.Id,
        //        Email = user.Email ?? string.Empty,
        //        FirstName = user.FirstName ?? string.Empty,
        //        LastName = user.LastName ?? string.Empty,
        //        Image = user.Image ?? string.Empty,
        //        Roles = roles
        //    };
        //    return Ok(model);
        //}

    }