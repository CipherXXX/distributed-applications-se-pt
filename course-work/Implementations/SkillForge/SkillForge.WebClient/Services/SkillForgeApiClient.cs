using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SkillForge.WebClient.Models.Api;

namespace SkillForge.WebClient.Services;

public class SkillForgeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string TokenKey = "SkillForgeToken";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SkillForgeApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private string? GetToken()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString(TokenKey);
    }

    private void SetToken(string? token)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
        {
            if (token == null)
                ctx.Session.Remove(TokenKey);
            else
                ctx.Session.SetString(TokenKey, token);
        }
    }

    private void ApplyToken()
    {
        var token = GetToken();
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public void SetAuthToken(string token)
    {
        SetToken(token);
    }

    public void ClearAuthToken()
    {
        SetToken(null);
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(GetToken());

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        ApplyToken();
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    public async Task<AuthResponse?> LoginAsync(object request, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/v1/auth/login", content, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var auth = JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);
        if (auth != null) SetToken(auth.Token);
        return auth;
    }

    public async Task<AuthResponse?> RegisterAsync(object request, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/v1/auth/register", content, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var auth = JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);
        if (auth != null) SetToken(auth.Token);
        return auth;
    }

    public async Task<AuthResponse?> RegisterAdminAsync(object request, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        ApplyToken();
        var response = await _httpClient.PostAsync("api/v1/auth/register-admin", content, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);
    }

    public async Task<PagedResult<StudentDto>?> GetStudentsAsync(int page = 1, int pageSize = 10, string? firstName = null, string? lastName = null, string? email = null, CancellationToken cancellationToken = default)
    {
        var q = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(firstName)) q.Add($"firstName={Uri.EscapeDataString(firstName)}");
        if (!string.IsNullOrWhiteSpace(lastName)) q.Add($"lastName={Uri.EscapeDataString(lastName)}");
        if (!string.IsNullOrWhiteSpace(email)) q.Add($"email={Uri.EscapeDataString(email)}");
        var url = "api/v1/students?" + string.Join("&", q);
        return await GetAsync<PagedResult<StudentDto>>(url, cancellationToken);
    }

    public async Task<StudentDto?> GetStudentAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetAsync<StudentDto>($"api/v1/students/{id}", cancellationToken);
    }

    public async Task<StudentDto?> CreateStudentAsync(object dto, CancellationToken cancellationToken = default)
    {
        return await PostAsync<StudentDto>("api/v1/students", dto, cancellationToken);
    }

    public async Task<StudentDto?> UpdateStudentAsync(int id, object dto, CancellationToken cancellationToken = default)
    {
        return await PutAsync<StudentDto>($"api/v1/students/{id}", dto, cancellationToken);
    }

    public async Task<bool> DeleteStudentAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await DeleteAsync($"api/v1/students/{id}", cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    /// <summary>Upload profile image for a student. Returns the new profile image URL on success, null on failure.</summary>
    public async Task<string?> UploadProfileImageAsync(int studentId, IFormFile file, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream();
        var response = await UploadMultipartAsync($"api/v1/students/{studentId}/profile-image", "file", stream, file.FileName, cancellationToken);
        if (response?.IsSuccessStatusCode != true) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("profileImageUrl", out var urlProp))
            return urlProp.GetString();
        return null;
    }

    /// <summary>Upload material file for a course. Returns the new material file URL on success, null on failure.</summary>
    public async Task<string?> UploadCourseMaterialAsync(int courseId, IFormFile file, CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream();
        var response = await UploadMultipartAsync($"api/v1/courses/{courseId}/material", "file", stream, file.FileName, cancellationToken);
        if (response?.IsSuccessStatusCode != true) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("materialFileUrl", out var urlProp))
            return urlProp.GetString();
        return null;
    }

    private async Task<HttpResponseMessage?> UploadMultipartAsync(string url, string formFieldName, Stream fileStream, string fileName, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(streamContent, formFieldName, fileName);
        ApplyToken();
        return await _httpClient.PostAsync(url, content, cancellationToken);
    }

    public async Task<PagedResult<CourseDto>?> GetCoursesAsync(int page = 1, int pageSize = 10, string? title = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var q = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(title)) q.Add($"title={Uri.EscapeDataString(title)}");
        if (isActive.HasValue) q.Add($"isActive={isActive.Value}");
        var url = "api/v1/courses?" + string.Join("&", q);
        return await GetAsync<PagedResult<CourseDto>>(url, cancellationToken);
    }

    public async Task<CourseDto?> GetCourseAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetAsync<CourseDto>($"api/v1/courses/{id}", cancellationToken);
    }

    public async Task<CourseDto?> CreateCourseAsync(object dto, CancellationToken cancellationToken = default)
    {
        return await PostAsync<CourseDto>("api/v1/courses", dto, cancellationToken);
    }

    public async Task<CourseDto?> UpdateCourseAsync(int id, object dto, CancellationToken cancellationToken = default)
    {
        return await PutAsync<CourseDto>($"api/v1/courses/{id}", dto, cancellationToken);
    }

    public async Task<bool> DeleteCourseAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await DeleteAsync($"api/v1/courses/{id}", cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<PagedResult<EnrollmentDto>?> GetEnrollmentsAsync(int page = 1, int pageSize = 10, int? studentId = null, int? courseId = null, bool? completed = null, CancellationToken cancellationToken = default)
    {
        var q = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (studentId.HasValue) q.Add($"studentId={studentId}");
        if (courseId.HasValue) q.Add($"courseId={courseId}");
        if (completed.HasValue) q.Add($"completed={completed.Value}");
        var url = "api/v1/enrollments?" + string.Join("&", q);
        return await GetAsync<PagedResult<EnrollmentDto>>(url, cancellationToken);
    }

    public async Task<PagedResult<EnrollmentDto>?> GetMyEnrollmentsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var url = $"api/v1/enrollments/mine?page={page}&pageSize={pageSize}";
        return await GetAsync<PagedResult<EnrollmentDto>>(url, cancellationToken);
    }

    public async Task<EnrollmentDto?> EnrollMeInCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await PostAsync<EnrollmentDto>("api/v1/enrollments/me", new { courseId }, cancellationToken);
    }

    public async Task<EnrollmentDto?> GetEnrollmentAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetAsync<EnrollmentDto>($"api/v1/enrollments/{id}", cancellationToken);
    }

    public async Task<EnrollmentDto?> CreateEnrollmentAsync(object dto, CancellationToken cancellationToken = default)
    {
        return await PostAsync<EnrollmentDto>("api/v1/enrollments", dto, cancellationToken);
    }

    public async Task<EnrollmentDto?> UpdateEnrollmentAsync(int id, object dto, CancellationToken cancellationToken = default)
    {
        return await PutAsync<EnrollmentDto>($"api/v1/enrollments/{id}", dto, cancellationToken);
    }

    public async Task<bool> DeleteEnrollmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await DeleteAsync($"api/v1/enrollments/{id}", cancellationToken);
        return response?.IsSuccessStatusCode == true;
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken) where T : class
    {
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }

    private async Task<T?> PostAsync<T>(string url, object payload, CancellationToken cancellationToken) where T : class
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        ApplyToken();
        var response = await _httpClient.PostAsync(url, content, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }

    private async Task<T?> PutAsync<T>(string url, object payload, CancellationToken cancellationToken) where T : class
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        ApplyToken();
        var response = await _httpClient.PutAsync(url, content, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }

    private async Task<HttpResponseMessage?> DeleteAsync(string url, CancellationToken cancellationToken)
    {
        ApplyToken();
        return await _httpClient.DeleteAsync(url, cancellationToken);
    }

    /// <summary>Download a file from uploads (path e.g. profiles/xxx.jpg). Returns the response so caller can stream it; null if not authenticated or request failed.</summary>
    public async Task<HttpResponseMessage?> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var url = "api/v1/files/download?path=" + Uri.EscapeDataString(path);
        return await SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);
    }
}
