using Microsoft.EntityFrameworkCore;
using Platform.Application.DTOs;
using Platform.Application.IRepos;
using Platform.Application.ServiceInterfaces;
using Platform.Core.DTOs;
using Platform.Core.Interfaces.IUnitOfWork;
using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repo;

        private readonly IUnitOfWork _unitOfWork;


        public CourseService(ICourseRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;

        }
        private CourseDetailsDto MapToDto(Courses c)
        {
            return new CourseDetailsDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                ThumbnailUrl = c.ThumbnailUrl,
                Price = c.Price,
                IsFree = c.IsFree,
                CreatedAt = c.CreatedAt,
                CategoryId = c.CategoryId,
                InstructorId = c.InstructorId,
                CategoryName = c.Category?.Name,
                InstructorName = c.Instructor?.User?.FirstName+" "+ c.Instructor?.User?.LastName  // adjust if Instructor.User has Name field
            };
        }
        private CourseDetailsMoreDto MapMoreToDto(Courses c)
        {
            return new CourseDetailsMoreDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                ThumbnailUrl = c.ThumbnailUrl,
                Price = c.Price,
                IsFree = c.IsFree,
                CreatedAt = c.CreatedAt,
                CategoryId = c.CategoryId,
                InstructorId = c.InstructorId,
                CategoryName = c.Category?.Name,
                InstructorName = c.Instructor?.User?.FirstName + " " + c.Instructor?.User?.LastName,
                NumofModulues = c.Modules?.Count() ?? 0,
                NumofVideos = c.Modules?.Sum(m => m.Videos?.Count() ?? 0) ?? 0
                // adjust if Instructor.User has Name field
            };
        }

        public async Task<IEnumerable<CourseDetailsDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto);
        }

        public async Task<CourseDetailsDto?> GetByIdAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            return c == null ? null : MapToDto(c);
        }





        public async Task<IEnumerable<CourseDetailsDto>?> GetByCategoryNameAsync(string name)
        {
          IEnumerable<Courses>?  courses = await  _repo.GetByCategoryNameAsync(name);

            if (courses == null || !courses.Any())
                return null;

            return  courses.Select(c => new CourseDetailsDto
            {
                Id = c?.Id??0,
                Title = c?.Title??"",
                Description = c?.Description ?? "",
                ThumbnailUrl = c?.ThumbnailUrl ?? "",
                Price = c?.Price ?? 0,
                IsFree = c?.IsFree ?? false,
                CreatedAt = c?.CreatedAt ?? DateTime.Now,
                CategoryId = c?.CategoryId ?? 0,
                InstructorId = c?.InstructorId ?? 0,
                CategoryName = c?.Category?.Name ?? "",
                InstructorName = c?.Instructor?.User?.FirstName ?? "" // adjust if Instructor.User has Name field

            });

        }






        public async Task<CourseDetailsDto> CreateAsync(CourseCreateDto dto)
        {
            var FolderPath = Path.Combine(AppContext.BaseDirectory, "wwwroot","uploads","Images");
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            var FileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ThumbnailUrl.FileName)}";
            var FilePath = Path.Combine(FolderPath, FileName);
            using var fileStream = new FileStream(FilePath, FileMode.Create);
            dto.ThumbnailUrl.CopyTo(fileStream);


            var entity = new Courses
            {
                Title = dto.Title,
                Description = dto.Description,
                ThumbnailUrl = FileName,
                Price = dto.Price,
                IsFree = dto.IsFree,
                CategoryId = dto.CategoryId,
                InstructorId = dto.InstructorId
            };

            var created = await _repo.AddAsync(entity);
            return MapToDto(created);
        }

        public async Task<bool> UpdateAsync(int id, CourseUpdateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;



            var folderPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads", "Images");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }


            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ThumbnailUrl.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);

            dto.ThumbnailUrl.CopyTo(fileStream);





            var oldImage = Path.Combine(folderPath, existing.ThumbnailUrl);
            if (oldImage != null)
            {
                File.Delete(oldImage);
            }


            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.ThumbnailUrl = fileName;
            existing.Price = dto.Price;
            existing.IsFree = dto.IsFree;
            existing.CategoryId = dto.CategoryId;
            existing.InstructorId = dto.InstructorId;

            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            await _repo.DeleteAsync(existing);
            return true;
        }

        public async Task<IEnumerable<CourseDetailsMoreDto>> GetCoursesByInstructorId(int instructorId)
        {
            var list = await _repo.GetCoursesByInstructorId(instructorId);
            return list.Select(MapMoreToDto);

        }

    }
}
