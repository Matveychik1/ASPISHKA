using Microsoft.AspNetCore.Identity;

namespace WebGenQRCode.Data.Entities.Identity;

public class RoleEntity : IdentityRole<int>
{
    public ICollection<UserRoleEntity>? UserRoles { get; set; } //колекція класу
}