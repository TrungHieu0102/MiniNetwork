namespace MiniNetwork.Infrastructure.Storage;

public class AwsS3Options
{
    public string BucketName { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string? AccessKey { get; set; } 
    public string? SecretKey { get; set; }  
}
