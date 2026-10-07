namespace WebGenQRCode.Constans;

public static class Roles
{
    public const string Admin = "admin";
    public const string User = "user";

    public static List<string> ListRoles() => [Admin, User];
}
