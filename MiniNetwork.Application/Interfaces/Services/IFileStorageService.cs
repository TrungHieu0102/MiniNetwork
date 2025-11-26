namespace MiniNetwork.Application.Interfaces.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Upload file lên storage, trả về URL public.
    /// </summary>
    Task<string> UploadAsync(
        Stream stream,
        string key,            // đường dẫn/key trên S3, vd: avatars/{userId}/xxx.png
        string contentType,
        CancellationToken ct = default);
    Task DeleteAsync(
        string urlOrKey,
        CancellationToken ct = default);
}
