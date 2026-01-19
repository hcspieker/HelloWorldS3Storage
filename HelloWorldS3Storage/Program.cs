using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddUserSecrets<Program>()
    .Build();

var baseUrl = configuration["baseUrl"];
var accessKey = configuration["accessKey"];
var secretKey = configuration["secretKey"];

var s3Config = new AmazonS3Config { ServiceURL = baseUrl };
var s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);

try
{
    var buckets = await s3Client.ListBucketsAsync();

    foreach (var bucket in buckets.Buckets)
    {
        Console.WriteLine("Bucket");
        Console.WriteLine($"Name: {bucket.BucketName}");
        Console.WriteLine($"Region: {bucket.BucketRegion}");

        if (bucket.CreationDate.HasValue)
            Console.WriteLine($"CreationDate: {bucket.CreationDate.Value:dd.MM.yyyy} - {bucket.CreationDate.Value:HH:mm:ss.fff}");

        Console.WriteLine();
        Console.WriteLine("Content (newest 3):");
        Console.WriteLine(new string('-', 15));

        try
        {
            var request = new ListObjectsRequest { BucketName = bucket.BucketName };
            var response = await s3Client.ListObjectsAsync(request);

            if (response.S3Objects == null || response.S3Objects.Count == 0)
            {
                Console.WriteLine("<empty>");
                Console.WriteLine(new string('-', 15));
            }
            else
            {
                foreach (var obj in response.S3Objects.OrderByDescending(x => x.LastModified).Take(3))
                {
                    Console.WriteLine("S3 object");
                    Console.WriteLine($"Key: {obj.Key}");
                    Console.WriteLine($"Size: {obj.Size} bytes");
                    if (obj.LastModified.HasValue)
                        Console.WriteLine($"LastModified: {obj.LastModified.Value:dd.MM.yyyy} - {obj.LastModified.Value:HH:mm:ss.fff}");
                    Console.WriteLine(new string('-', 15));
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"Error while retrieving objects for bucket {bucket.BucketName}");
        }
        Console.WriteLine();
        Console.WriteLine(new string('#', 50));
        Console.WriteLine();
    }
}
catch (Exception)
{
    Console.WriteLine($"Connection to {baseUrl} could not get established.");
}
