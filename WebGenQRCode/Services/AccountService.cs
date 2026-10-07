using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using System.Text.Json;
using WebGenQRCode.Constans;
//using WebGenQRCode.Constants;
using WebGenQRCode.Data.Entities.Identity;
using WebGenQRCode.Interfaces;
using WebGenQRCode.Models.Account;

namespace WebGenQRCode.Services;

public class AccountService(UserManager<UserEntity> userManager,
    IImageService imageService,
    IJwtTokenService jwtTokenService) : IAccountService
{
    public async Task<string> LoginByGoogle(string token)
    {
        using var httpClient = new HttpClient();
        //Встановлюємо токен в заголовок авторизації
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        //Налаштовуємо запит до Google API для отримання даних користувача
        var response = await httpClient
            .GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        if (!response.IsSuccessStatusCode)
            throw new Exception("Помилка при отриманні даних користувача з Google");
        throw new Exception("Помилка при створенні нового користувача");
        //var json = await response.Content.ReadAsStringAsync();
        //GoogleAccountModel googleAccount = JsonSerializer
        //    .Deserialize<GoogleAccountModel>(json)!;
        ////Перевіряємо чи існує користувач з таким email в базі даних
        //var existingUser = await userManager.FindByEmailAsync(googleAccount.Email);
        //if (existingUser != null)
        //{
        //    //перевіряємо чи користувач має запис про вхід з google
        //    var logins = await userManager.FindByLoginAsync("Google", googleAccount.GoogleId);
        //    if (logins == null)
        //    {
        //        //якщо користувач існує, але не має запису про вхід з google
        //        //додаємо запис про вхід з google
        //        var gResult = await userManager.AddLoginAsync(existingUser,
        //            new UserLoginInfo("Google", googleAccount.GoogleId, "Google"));
        //        if (!gResult.Succeeded)
        //            throw new Exception("Помилка при додаванні запису про вхід з Google");
        //    }
        //    //Повертаємо токен для доступу до системи
        //    var tokenServer = await jwtTokenService.CreateTokenAsync(existingUser);
        //    return tokenServer;
        //}
        ////Якщо користувач не існує, створюємо нового користувача
        //var newUser = new UserEntity
        //{
        //    FirstName = googleAccount.FirstName,
        //    LastName = googleAccount.LastName,
        //    Email = googleAccount.Email,
        //    UserName = googleAccount.Email,
        //};
        //if (!string.IsNullOrEmpty(googleAccount.Picture))
        //{
        //    newUser.Image =
        //        await imageService.SaveImageFromUrlAsync(googleAccount.Picture);
        //}
        ////Додаємо запис про вхід з google - без пароля,
        ////бо користувач авторизується через google
        //var result = await userManager.CreateAsync(newUser);
        //if (result.Succeeded)
        //{
        //    await userManager.AddLoginAsync(newUser,
        //        new UserLoginInfo("Google", googleAccount.GoogleId, "Google"));
        //    await userManager.AddToRoleAsync(newUser, Roles.User);
        //    var jwtToken = await jwtTokenService.CreateTokenAsync(newUser);
        //    return jwtToken;
        //}
        //else
        //{
        //    throw new Exception("Помилка при створенні нового користувача");
        //}
    }
}