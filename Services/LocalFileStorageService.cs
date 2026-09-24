using Maiven_Portal_Managment.Exceptions;

namespace Maiven_Portal_Managment.Services;

public sealed class LocalFileStorageService(IWebHostEnvironment environment)
{
    private static readonly Dictionary<string, string> AllowedExtensionsByContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public const long MaxFileBytes = 5 * 1024 * 1024;

    public string AvatarUploadsRelativeDir => Path.Combine("uploads", "avatars");

    private string WebRoot()
    {
        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        return webRoot;
    }

    public string GetAvatarUploadsRoot()
    {
        var root = Path.Combine(WebRoot(), "uploads", "avatars");
        Directory.CreateDirectory(root);
        return root;
    }

    public async Task<string> SaveAvatarAsync(long userId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("An image file is required.");
        }

        if (file.Length > MaxFileBytes)
        {
            throw new BadRequestException("Image must not exceed 5MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException("Only .jpg, .jpeg, .png and .webp images are allowed.");
        }

        if (!string.IsNullOrWhiteSpace(file.ContentType) &&
            AllowedExtensionsByContentType.TryGetValue(file.ContentType, out var expectedExtension))
        {
            var normalized = extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg" : extension.ToLowerInvariant();
            var expected = expectedExtension.ToLowerInvariant();
            if (!normalized.Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("File extension does not match its content type.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(file.ContentType) &&
            !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Only image files are allowed.");
        }

        var root = GetAvatarUploadsRoot();
        var safeExtension = extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg" : extension.ToLowerInvariant();
        var fileName = $"{userId}_{Guid.NewGuid():N}{safeExtension}";
        var physicalPath = Path.Combine(root, fileName);

        await using var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/avatars/{fileName}".Replace('\\', '/');
    }

    public void DeleteByRelativeUrl(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl) || !relativeUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var trimmed = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRoot = WebRoot();
        var physicalPath = Path.Combine(webRoot, trimmed);
        var root = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
        var target = Path.GetFullPath(physicalPath);

        if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(target))
        {
            return;
        }

        File.Delete(target);
    }
}
