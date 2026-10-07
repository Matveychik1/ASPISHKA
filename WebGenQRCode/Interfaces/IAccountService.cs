namespace WebGenQRCode.Interfaces;

public interface IAccountService
{
    /// <summary>
    /// Авторизація користувача через гугл
    /// </summary>
    /// <param name="token">Токен, отриманий від Google</param>
    /// <returns>Токен для доступу до системи</returns>
    public Task<string> LoginByGoogle(string token);
}