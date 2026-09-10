namespace WebGenQRCode.Interfaces;

public interface IImageService
{
        /// <summary>
        /// Стискає та зберігає зображення на диск.
        /// Незалежно від початкового розміру/формату, файл буде перекодований
        /// у JPEG зі зменшеною шириною та заданою якістю стиснення.
        /// </summary>
        /// <param name="file">Файл, переданий користувачем (IFormFile)</param>
        /// <param name="folderPath">Абсолютний шлях до папки, куди зберігати (напр. wwwroot/images)</param>
        /// <returns>Ім'я збереженого файлу (без шляху)</returns>
        Task<string> SaveOptimizedImageAsync(IFormFile file);
        /// <summary>
        /// Те саме, що SaveOptimizedImageAsync(IFormFile, string), але приймає
        /// зображення у вигляді base64-рядка (як звичайного, так і у форматі
        /// data URI на кшталт "data:image/png;base64,....").
        /// </summary>
        /// <param name="base64Image">Base64-рядок із вмістом зображення</param>
        /// <param name="folderPath">Абсолютний шлях до папки, куди зберігати</param>
        /// <returns>Ім'я збереженого файлу (без шляху)</returns>
        Task<string> SaveOptimizedImageAsync(string base64Image);
        Task RemoveImageAsync(string imageName);
}
