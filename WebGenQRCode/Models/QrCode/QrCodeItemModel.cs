namespace WebGenQRCode.Models.QrCode;

public class QrCodeItemModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string TargetUrl { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool IsActive { get; set; }
    public string CreateAt { get; set; } = null!;
    public int ScanCount { get; set; }
}