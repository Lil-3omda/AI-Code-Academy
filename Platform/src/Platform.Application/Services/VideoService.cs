using Microsoft.AspNetCore.Http;
using Platform.Application.DTOs;
using Platform.Application.ServiceInterfaces;
using Platform.Core.Interfaces.IUnitOfWork;
using Platform.Core.Models;

namespace Platform.Application.Services
{
    public class VideoService : IVideoService
    {
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _unitOfWork;

        public VideoService(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task AddVideoAsync(IFormFile file, string title, int moduleId,int videoarrang)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty", nameof(file));

            var filePath = await _fileService.UploadVideoAsync(file);
            var duration = await _fileService. GetVideoDurationAsync(filePath);

            var video = new Video
            {
                Title = title,
                FilePath = filePath,
                ModuleId = moduleId,
                Duration = duration, // TODO: calculate real duration if needed
                VideoArrangement = videoarrang
            };

            await _unitOfWork.vedioRepository.AddAsync(video);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteVideoAsync(int id)
        {
            var video = await _unitOfWork.vedioRepository.GetByIdAsync(id);
            if (video == null) throw new KeyNotFoundException("Video not found");

            _fileService.DeleteFile(video.FilePath);

            await _unitOfWork.vedioRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<VideoGetAllDto>> GetAllVideosAsync()
        {
            var videos = await _unitOfWork.vedioRepository.GetVideosWithCourse();

            return videos.Select(v => new VideoGetAllDto
            {
                Id = v.Id,
                Title = v.Title,
                FilePath = v.FilePath,
                Duration = v.Duration,
                VideoArrangement = v.VideoArrangement,
                ModuleId = v.ModuleId,
                CourseId = v.Module?.Course?.Id ?? 0,
                CourseName = v.Module?.Course?.Title ?? ""
            });

        }

        public async Task<IEnumerable<VideoGetAllDto>> GetByInsId(int insId)
        {
            var videos = await _unitOfWork.vedioRepository.GetVideosByInstructorId(insId);
            return videos.Select(v => new VideoGetAllDto
            {
                Id = v.Id,
                Title = v.Title,
                FilePath = v.FilePath,
                Duration = v.Duration,
                VideoArrangement = v.VideoArrangement,
                ModuleId = v.ModuleId,
                CourseId = v.Module?.Course?.Id ?? 0,
                CourseName = v.Module?.Course?.Title ?? ""
            });

        }

        public async Task<Video?> GetVideoByIdAsync(int id)
        {
            return await _unitOfWork.vedioRepository.GetByIdAsync(id);
        }

        public async Task UpdateVideoAsync(VideoUpdateDto dto)
        {
            var video = await _unitOfWork.vedioRepository.GetByIdAsync(dto.Id);
            if (video == null)
                throw new KeyNotFoundException("Video not found");

            // If new file provided → replace old file
            if (dto.File != null && dto.File.Length > 0)
            {
                // delete old file
                _fileService.DeleteFile(video.FilePath);

                // upload new file
                var newFilePath = await _fileService.UploadVideoAsync(dto.File);
                var duration = await _fileService.GetVideoDurationAsync(newFilePath);

                video.FilePath = newFilePath;
                video.Duration = duration;
            }

            // update other fields
            video.Title = dto.Title;
            video.ModuleId = dto.ModuleId;
            video.VideoArrangement = dto.VideoArrangement;

            _unitOfWork.vedioRepository.Update(video);
            await _unitOfWork.SaveChangesAsync();
        }

    }
}
