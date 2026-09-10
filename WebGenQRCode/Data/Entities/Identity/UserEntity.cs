using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebGenQRCode.Data.Entities.Identity;

public class UserEntity : IdentityUser<int>
{
    [StringLength(100)]
    public string? FirstName { get; set; }
    [StringLength(100)]
    public string? LastName { get; set; }
    [StringLength(100)]
    public string? Image { get; set; }
    public ICollection<UserRoleEntity>? UserRoles { get; set; }
    public ICollection<QrCodeEntity>? QrCodes { get; set; }
}
