namespace WebGenQRCode.Models.Users;

public class UserItemModel
{
    public int Id { get; set; }
    public string Email { get; set; } = String.Empty;
    public string FullName { get; set; } = String.Empty;
    public string Image { get; set; } = String.Empty;
}