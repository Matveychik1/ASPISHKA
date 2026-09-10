using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebGenQRCode.Data.Entities.Identity;

namespace WebGenQRCode.Data.Entities;

[Table("tblQrCodes")]
public class QrCodeEntity
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;

    [Required, StringLength(100)]
    public string Name { get; set; } = null!;

    [Required, StringLength(100)]
    public string Code { get; set; } = null!;

    [Required, StringLength(2048)]
    public string TargetUrl { get; set; } = null!;
    //Чи активний даний QR-код
    public bool IsActive { get; set; } = true;

    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    //Скільки разів було здійснено сканування коду
    public int ScanCount { get; set; }
}
