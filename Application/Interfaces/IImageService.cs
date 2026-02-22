namespace Application.Interfaces;

public interface IImageService
{
    Task<string> SaveImageToServer(string token, string extension, params string[] folders);
    Task<string> OverWriteImageToServer(string token, string fileName, params string[] folders);
}
