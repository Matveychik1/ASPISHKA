namespace WebGenQRCode.Models.Seeder;

public class SeederQrCodeModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string User { get; set; } = "";
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string TargetUrl { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int ScanCount { get; set; }
}