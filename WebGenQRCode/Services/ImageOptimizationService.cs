using SkiaSharp;
using WebGenQRCode.Interfaces;

namespace WebGenQRCode.Services;

/// <summary>
/// Стискає фото на сервері перед збереженням, щоб великі фото
/// (наприклад 2-5 Мб з телефону) не "як є" вантажили сторінку,
/// а перетворювались у легкий, але якісний JPEG.
/// </summary>
public class ImageOptimizationService(IConfiguration configuration,
    ILogger<ImageOptimizationService> logger)
    : IImageService
{
    private const int quality = 80; //якість фото

    public async Task<string> SaveOptimizedImageAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Файл зображення порожній", nameof(file));

        byte[] imageBytes;

        await using (var stream = file.OpenReadStream())
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            imageBytes = ms.ToArray();
        }

        var originalSizeKb = file.Length / 1024;

        return await SaveOptimizedImageAsync(imageBytes, originalSizeKb); //
    }

    public async Task<string> SaveOptimizedImageAsync(string base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
            throw new ArgumentException("Base64-рядок зображення порожній", nameof(base64Image));

        var base64Data = base64Image.Split(',')[1];

        byte[] imageBytes;

        try
        {
            imageBytes = Convert.FromBase64String(base64Data);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Некоректний base64-рядок зображення", nameof(base64Image), ex);
        }

        if (imageBytes.Length == 0)
            throw new ArgumentException("Base64-рядок не містить даних зображення", nameof(base64Image));

        var originalSizeKb = imageBytes.Length / 1024;

        return await SaveOptimizedImageAsync(imageBytes, originalSizeKb);
    }

    /// <summary>
    /// Спільна внутрішня логіка збереження: приймає вже готовий байт-масив
    /// зображення (незалежно від того, звідки він прийшов — IFormFile чи base64).
    /// </summary>
    private async Task<string> SaveOptimizedImageAsync(byte[] imageBytes, long originalSizeKb) //приймає набір байтів
    {
        var folder = configuration.GetRequiredSection("ImagesDir").Get<string>() ?? "myimages"; //вказує на папку де зберігати фото
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), folder);
        Directory.CreateDirectory(folderPath);

        var sizes = configuration
            .GetRequiredSection("ImageSizes")
            .Get<List<int>>() ?? throw new InvalidOperationException("ImageSizes not found");

        var imageName = Guid.NewGuid().ToString();
        var extension = ".webp";

        var tasks = sizes.Select(size => SaveSizeAsync(
            imageBytes,
            size,
            Path.Combine(folderPath, $"{imageName}_{size}{extension}")));

        await Task.WhenAll(tasks);

        logger.LogInformation(
            "Фото ({Original} КБ) успішно збережено у {Count} розмірах",
            originalSizeKb,
            sizes.Count);

        return imageName;
    }


    private async Task SaveSizeAsync(byte[] imageBytes, int maxWidth, string path)
    {
        using var original = SKBitmap.Decode(imageBytes);

        if (original is null)
            throw new InvalidOperationException("Не вдалося розпізнати зображення");

        using var resized = ResizeIfNeeded(original, maxWidth, maxWidth);

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Webp, quality);

        await using var stream = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true);

        await data.AsStream().CopyToAsync(stream);
    }

    private static SKBitmap ResizeIfNeeded(SKBitmap source, int maxWidth, int maxHeight)
    {
        var width = source.Width;
        var height = source.Height;

        var ratio = Math.Min((double)maxWidth / width, (double)maxHeight / height);

        if (ratio >= 1.0)
            return source.Copy();

        var newWidth = (int)Math.Round(width * ratio);
        var newHeight = (int)Math.Round(height * ratio);

        var resized = source.Resize(
            new SKImageInfo(newWidth, newHeight),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));

        return resized ?? throw new InvalidOperationException("Не вдалося змінити розмір зображення");
    }

    public Task RemoveImageAsync(string imageName)
    {
        var folder = configuration.GetRequiredSection("ImagesDir").Get<string>() ?? "myimages";
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), folder);
        if (string.IsNullOrWhiteSpace(imageName))
            throw new ArgumentException("Ім'я зображення не може бути порожнім", nameof(imageName));

        var sizes = configuration
            .GetRequiredSection("ImageSizes")
            .Get<List<int>>() ?? throw new InvalidOperationException("ImageSizes not found");

        const string extension = ".webp";
        var deletedCount = 0;

        foreach (var size in sizes)
        {
            var filePath = Path.Combine(folderPath, $"{imageName}_{size}{extension}");

            if (!File.Exists(filePath))
            {
                logger.LogWarning("Файл {FilePath} не знайдено", filePath);
                continue;
            }

            try
            {
                File.Delete(filePath);
                deletedCount++;
            }
            catch (IOException ex)
            {
                logger.LogError(ex, "Не вдалося видалити файл {FilePath}", filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError(ex, "Немає доступу для видалення файлу {FilePath}", filePath);
            }
        }

        logger.LogInformation(
            "Видалено {DeletedCount} з {TotalCount} файлів для зображення {ImageName}",
            deletedCount,
            sizes.Count,
            imageName);

        return Task.CompletedTask;
    }
}
