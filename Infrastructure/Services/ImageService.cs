using Application.Interfaces;

namespace Infrastructure.Services;

using System.Text.RegularExpressions;
using Application.Interfaces;
using CsvHelper.TypeConversion;

public class ImageService : IImageService
{
    public async Task<string> SaveImageToServer(
        string base64String,
        string extension,
        params string[] folders
    )
    {
        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64String.Split(',').Last());

            string fileName = $"{Guid.NewGuid()}.{extension.Split('.').Last().TrimStart('.')}";
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            foreach (var folder in folders)
            {
                directoryPath = Path.Combine(directoryPath, folder);
            }
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filePath = Path.Combine(directoryPath, fileName);
            await File.WriteAllBytesAsync(filePath, imageBytes);
            if (folders.Length > 0)
            {
                var finalPath = Path.Combine("uploads", "");
                foreach (var folder in folders)
                {
                    finalPath = Path.Combine(finalPath, folder);
                }
                finalPath = Path.Combine(finalPath, fileName);
                return finalPath;
            }
            return $"/uploads/{fileName}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving image: {ex.Message}");
            return null;
        }
    }

    public async Task<string> OverWriteImageToServer(
        string base64String,
        string fileName,
        params string[] folders
    )
    {
        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64String.Split(',').Last());
            string directoryPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot\\uploads"
            );
            foreach (var folder in folders)
            {
                directoryPath = Path.Join(directoryPath, folder);
            }
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            var f = fileName.Split('\\').Last();
            string filePath = Path.Join(directoryPath, f);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            await File.WriteAllBytesAsync(filePath, imageBytes);
            if (folders.Length > 0)
            {
                var finalPath = Path.Combine("uploads", "");
                foreach (var folder in folders)
                {
                    finalPath = Path.Combine(finalPath, folder);
                }
                finalPath = Path.Combine(finalPath, f);
                return finalPath;
            }
            return $"/uploads/{fileName}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving image: {ex.Message}");
            return null;
        }
    }
}
