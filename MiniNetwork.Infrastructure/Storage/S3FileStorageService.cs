using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MiniNetwork.Application.Interfaces.Services;

namespace MiniNetwork.Infrastructure.Storage;

public class S3FileStorageService : IFileStorageService
{
    private readonly AwsS3Options _options;
    private readonly IAmazonS3 _s3;

    public S3FileStorageService(IOptions<AwsS3Options> options)
    {
        _options = options.Value;

        var region = RegionEndpoint.GetBySystemName(_options.Region);

        if (!string.IsNullOrWhiteSpace(_options.AccessKey) &&
            !string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            _s3 = new AmazonS3Client(_options.AccessKey, _options.SecretKey, region);
        }
        else
        {
            // Nếu deploy trong môi trường có IAM role (EC2, ECS…), có thể dùng ctor mặc định
            _s3 = new AmazonS3Client(region);
        }
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string key,
        string contentType,
        CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = false,
           // CannedACL = S3CannedACL.PublicRead // cho phép public đọc ảnh avatar
        };

        var response = await _s3.PutObjectAsync(request, ct);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception($"Upload S3 failed, status: {response.HttpStatusCode}");
        }

        // URL public dạng: https://{bucket}.s3.{region}.amazonaws.com/{key}
        var url = $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/{key}";
        return url;
    }
    public async Task DeleteAsync(string urlOrKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(urlOrKey))
            return;

        string key = urlOrKey;

        // Nếu truyền URL đầy đủ thì tách key ra
        if (urlOrKey.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            var prefix = $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/";
            if (!urlOrKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                // Không phải file trong bucket này, bỏ qua
                return;
            }

            key = urlOrKey.Substring(prefix.Length);
        }

        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key
        };

        try
        {
            await _s3.DeleteObjectAsync(request, ct);
        }
        catch (AmazonS3Exception ex)
        {
            // Có thể log nếu cần, nhưng không quăng lỗi lên
            Console.WriteLine($"[S3] Delete failed: {ex.Message}");
        }
    }

}
