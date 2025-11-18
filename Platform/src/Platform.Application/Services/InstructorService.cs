using Microsoft.AspNetCore.Identity;
using Platform.Application.DTOs;
using Platform.Core.DTOs;
using Platform.Core.Interfaces.IUnitOfWork;
using Platform.Core.Models;
using Platform.Application.Interfaces;
using Platform.Core.Interfaces;

namespace Platform.Application.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public InstructorService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // ------------------- Verify Instructor -------------------
        public async Task<(bool Succeeded, string Errors)> VerifyInstructorAsync(int id)
        {
            var instructorRepo = _unitOfWork.Repository<Instructor>();
            var instructor = await instructorRepo.GetByIdAsync(id);
            if (instructor == null) return (false, "Instructor not found");

            instructor.IsVerified = true;
            instructorRepo.Update(instructor);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }

        // ------------------- Get Instructor by Id -------------------
        public async Task<InstructorDto?> GetInstructorByIdAsync(int id)
        {
            var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(id);
            if (instructor == null) return null;

            return new InstructorDto
            {
                Id = instructor.Id,
                LinkedIn = instructor.LinkedIn,
                Qualifications = instructor.Qualifications,
                IsVerified = instructor.IsVerified,
                UserId = instructor.UserId,
                FullName = $"{instructor.User.FirstName} {instructor.User.LastName}",
                Email = instructor.User.Email,
                Courses = instructor.Courses?.Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description
                }).ToList()
            };
        }

        // ------------------- Get All Instructors -------------------
        public async Task<IEnumerable<InstructorDto>> GetAllInstructorsAsync()
        {
            var instructors = await _unitOfWork.instructorRepository.GetAllAsync();
            return instructors.Select(i => new InstructorDto
            {
                Id = i.Id,
                LinkedIn = i.LinkedIn,
                Qualifications = i.Qualifications,
                IsVerified = i.IsVerified,
                UserId = i.UserId,
                FullName = $"{i.User.FirstName} {i.User.LastName}",
                Email = i.User.Email,
                Courses = i.Courses?.Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description
                }).ToList()
            });
        }

        // ------------------- Register Instructor (for existing User) -------------------
        public async Task<(bool Succeeded, string Errors)> RegisterInstructor2Async(RegisterInstructorIfAccountExistsDto dto)
        {
            try
            {
                var instructorRepo = _unitOfWork.Repository<Instructor>();

                var instructors = await instructorRepo.GetAllAsync();
                if (instructors.Any(i => i.UserId == dto.UserId))
                    return (false, "This user is already an instructor.");

                var instructor = new Instructor
                {
                    LinkedIn = dto.LinkedIn,
                    Qualifications = dto.Qualifications,
                    UserId = dto.UserId,
                    IsVerified = false
                };

                await instructorRepo.AddAsync(instructor);
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ------------------- Register Instructor (for new User) -------------------
        public async Task<InstructorReadDto> RegisterInstructorAsync(InstructorRegisterDto dto)
        {
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Address = dto.Address,
                Gender = dto.Gender
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "Instructor");

            var instructor = new Instructor
            {
                UserId = user.Id,
                LinkedIn = dto.LinkedIn,
                Qualifications = dto.Qualifications,
                IsVerified = false
            };

            await _unitOfWork.Repository<Instructor>().AddAsync(instructor);
            await _unitOfWork.SaveChangesAsync();

            return new InstructorReadDto
            {
                Id = instructor.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                LinkedIn = instructor.LinkedIn,
                IsVerified = instructor.IsVerified,
                Qualifications = instructor.Qualifications
            };
        }
    }
}
