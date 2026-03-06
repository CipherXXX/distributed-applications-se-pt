namespace SkillForge.API.Helpers;

/// <summary>
/// Validation rules for file uploads: allowed extensions and max size (5MB).
/// </summary>
public static class FileUploadValidation
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    private static readonly HashSet<string> AllowedProfileImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    private static readonly HashSet<string> AllowedMaterialExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx"
    };

    public static bool IsValidProfileImage(string? fileName, long length, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            error = "File name is required.";
            return false;
        }
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) || !AllowedProfileImageExtensions.Contains(ext))
        {
            error = "Only image files are allowed (jpg, jpeg, png, gif, webp).";
            return false;
        }
        if (length <= 0 || length > MaxFileSizeBytes)
        {
            error = $"File size must be between 1 byte and {MaxFileSizeBytes / (1024 * 1024)}MB.";
            return false;
        }
        return true;
    }

    public static bool IsValidMaterialFile(string? fileName, long length, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            error = "File name is required.";
            return false;
        }
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) || !AllowedMaterialExtensions.Contains(ext))
        {
            error = "Only PDF and Word documents are allowed (.pdf, .doc, .docx).";
            return false;
        }
        if (length <= 0 || length > MaxFileSizeBytes)
        {
            error = $"File size must be between 1 byte and {MaxFileSizeBytes / (1024 * 1024)}MB.";
            return false;
        }
        return true;
    }
}
