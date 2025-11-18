using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Platform.Application.ServiceInterfaces;
using Xabe.FFmpeg;

namespace Platform.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        // Allowed video types
        private readonly string[] _allowedVideoTypes = { ".mp4", ".mov", ".avi", ".mkv", ".webm" };

        public FileService(IWebHostEnvironment env)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));

            // Ensure WebRootPath exists
            if (string.IsNullOrEmpty(_env.WebRootPath))
            {
                _env.WebRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
            }

            if (!Directory.Exists(_env.WebRootPath))
                Directory.CreateDirectory(_env.WebRootPath);
        }

        // ✅ Uploads a video file safely to: wwwroot/uploads/{folderName}
        public async Task<string> UploadVideoAsync(IFormFile file, string folderName = "videos")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedVideoTypes.Contains(extension))
                throw new ArgumentException("Invalid video format. Allowed: mp4, mov, avi, mkv, webm");

            // ✅ Limit file size (100MB max)
            if (file.Length > 100 * 1024 * 1024)
                throw new ArgumentException("Video file is too large (max 100 MB).");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // ✅ Asynchronous copy for better performance
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // ✅ Return relative URL (used by frontend)
            return $"/uploads/{folderName}/{fileName}";
        }

        // ✅ Deletes any uploaded file by URL
        public void DeleteFile(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return;

            var filePath = Path.Combine(_env.WebRootPath, fileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error deleting file {filePath}: {ex.Message}");
                }
            }
        }

        // ✅ Returns video duration (in seconds)
        public async Task<int> GetVideoDurationAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("File URL cannot be empty.");

            var absolutePath = Path.Combine(_env.WebRootPath, fileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(absolutePath))
                throw new FileNotFoundException($"Video file not found at path: {absolutePath}");

            var mediaInfo = await FFmpeg.GetMediaInfo(absolutePath);
            var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

            return videoStream == null ? 0 : (int)videoStream.Duration.TotalSeconds;
        }
    }
}
