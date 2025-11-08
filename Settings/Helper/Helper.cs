using System.Security.Cryptography;
using System.Text;
using System.Drawing;

namespace ChatService.Settings.Helper
{
    public static class Helper
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string hashedEnteredPassword = HashPassword(enteredPassword);
            return hashedEnteredPassword == storedHash;
        }

        public static Image Base64ToImage(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                return null;
            }

            try
            {
                string base64Data = base64String.Contains(",")
                    ? base64String.Substring(base64String.IndexOf(",") + 1)
                    : base64String;

                byte[] imageBytes = Convert.FromBase64String(base64Data);
                using var ms = new MemoryStream(imageBytes);
                return Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to convert base64 to image");
                return null;
            }
        }

        public static string SaveBase64ImageToProjectFolder(string base64String, string folderPath, string prefix)
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                return null;
            }

            try
            {
                var image = Base64ToImage(base64String);
                if (image == null)
                {
                    return null;
                }

                Directory.CreateDirectory(folderPath);

                string fileName = $"{prefix}_{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                string fullPath = Path.Combine(folderPath, fileName);

                using (image) // Ensure image is disposed
                {
                    image.Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);
                }

                return $"/assets/images/{fileName}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to save {prefix} image");
                return null;
            }
        }

        public static string SaveAndCleanupImage(string base64String, string folderPath, string prefix, string existingFilePath)
        {
            try
            {
                // First save the new image
                string newPath = SaveBase64ImageToProjectFolder(base64String, folderPath, prefix);

                if (newPath != null && !string.IsNullOrEmpty(existingFilePath))
                {
                    // Only delete old file if new file was saved successfully
                    DeleteImageFile(existingFilePath);
                }

                return newPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to save and cleanup {prefix} image");
                return null;
            }
        }

        public static bool DeleteImageFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            try
            {
                string fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to delete image at {relativePath}");
                return false;
            }
        }

        public static string ImageFileToBase64(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            try
            {
                string fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                byte[] imageBytes = File.ReadAllBytes(fullPath);
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to convert image to base64: {relativePath}");
                return null;
            }
        }
    }
}
