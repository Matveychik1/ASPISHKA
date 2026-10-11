using WebGenQRCode.Data.Entities.Identity;

namespace WebGenQRCode.Interfaces;

public interface IJwtTokenService
{
    Task<string> CreateTokenAsync(UserEntity user);
}