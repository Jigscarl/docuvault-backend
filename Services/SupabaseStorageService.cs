using System.Net.Http.Headers;
using System.Net.Http.Json;
using Docuvault.Api.Services;

namespace Docuvault.Api.Services;

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string BucketName = "docuvault-files";

    public SupabaseStorageService(
        HttpClient httpClient, IConfiguration configuration
    )
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> UploadAsync(
        IFormFile file,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if(file == null || file.Length == 0)
        throw new ArgumentException("Invalid file.");

        var supabaseUrl = GetConfiguration("Supabase:Url");
        var serviceKey = GetConfiguration("Supabase:ServiceRoleKey");
        var extension = Path.GetExtension(file.FileName);
        var storagePath = $"{userId}/{Guid.NewGuid()}{extension}";
        var encodedPath = string.Join("/", storagePath.Split('/').Select(Uri.EscapeDataString));
        var uploadUrl = $"{supabaseUrl.TrimEnd('/')}/storage/v1/object/{BucketName}/{encodedPath}";
        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Add("apikey", serviceKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
        using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType)? "application/octet-stream" : file.ContentType);
        request.Content = content;
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return storagePath;
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var supabaseUrl = GetConfiguration("Supabase:Url");
        var serviceKey = GetConfiguration("Supabase:ServiceRoleKey");
        var deleteUrl = $"{supabaseUrl.TrimEnd('/')}/storage/v1/object/{BucketName}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
        request.Headers.Add("apikey", serviceKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
        request.Content = JsonContent.Create(new{prefixes= new[]{storagePath}});
        using var response = await _httpClient.SendAsync(request, cancellationToken);
    }
    private string GetConfiguration(string key)
    {
        return _configuration[key] ?? throw new InvalidOperationException($"Missing configuration: key");
    }

    public async Task <string> CreateSignedUrlAsync(
        string storagePath,
        int expiresInSeconds = 300,
        CancellationToken cancellationToken = default
    )
    {
        var supabaseUrl = GetConfiguration("Supabase:Url");
        var serviceKey = GetConfiguration("Supabase:ServiceRoleKey");
        var encodedPath = string.Join("/",storagePath.Split('/').Select(Uri.EscapeDataString));
        var signedUrlEndpoint = $"{supabaseUrl.TrimEnd('/')}/storage/v1/object/sign/{BucketName}/{encodedPath}";
        using var request = new HttpRequestMessage(HttpMethod.Post, signedUrlEndpoint);
        request.Headers.Add("apikey", serviceKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
        request.Content = System.Net.Http.Json.JsonContent.Create( new
        {expiresIn = expiresInSeconds}
        );
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SignedUrlResponse>(
            cancellationToken: cancellationToken
        );

        if (string.IsNullOrWhiteSpace(result?.SignedURL))
        throw new InvalidOperationException("Supabase did not return a signed URL.");

        return $"{supabaseUrl.TrimEnd('/')}/storage/v1{result.SignedURL}";
    }

    private sealed class SignedUrlResponse
    {
        public string? SignedURL {get; set;}
    }
}