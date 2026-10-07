namespace WebGenQRCode.Models.QrCode;

public class CreateQrCodeRequest
{
    //назва qr code
    public string Name { get; set; } = null!;
    //посилання на яке буде здійснено редірект після сканування
    public string TargetUrl { get; set; } = null!;

}
