using Platform.Application.DTOs;
using Platform.Application.ServiceInterfaces;
using Platform.Core.Interfaces.IUnitOfWork;
using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork unitOfWork;

        public EnrollmentService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }



        //Add Enrollment
        public async Task AddAsync(AddEnrollmentDTO enrollment)
        {
            await unitOfWork.enrollmentRepository.AddAsync(new Enrollment()
            {
                Id = enrollment.Id,
                EnrollmentDate = enrollment.EnrollmentDate,
                CourseId = enrollment.CourseId,
                IsCanceled = false,
                IsDeleted=false,
                ProgressPercentage=enrollment.ProgressPercentage,
                Status=enrollment.Status,
                StdId= enrollment.StdId,              
            });


          await  unitOfWork.SaveChangesAsync();
        }



        //Cancel Enrollment
        public async Task CancelEnrollement(int id)
        {
           await unitOfWork.enrollmentRepository.CancelEnrollement(id);

            await unitOfWork.SaveChangesAsync();
        }



        //Delete Enrollment
        public async Task DeleteEnrollement(int id)
        {
           await unitOfWork.enrollmentRepository.Delete(id);

            await unitOfWork.SaveChangesAsync();
        }



        //Get All Enrollments
        public async Task<IEnumerable<EnrollementDTO>?> GetAllEnrollementsAsync()
        {
            IEnumerable<Enrollment> enrollements = await unitOfWork.enrollmentRepository.GetAllAsync();

            if(enrollements == null)
            {
                return null;
            }
            else
            {
                IEnumerable<EnrollementDTO> enrollementsDto = enrollements.Select(e => new EnrollementDTO()
                {
                    Id = e?.Id ?? 0,
                    EnrollmentDate = e?.EnrollmentDate ?? DateTime.UtcNow,
                    ProgressPercentage = e?.ProgressPercentage ?? 0,
                    StdId = e?.StdId ?? 0,
                    CourseId = e?.CourseId ?? 0,
                    IsDeleted = e?.IsDeleted ?? false,
                    IsCanceled = e?.IsCanceled ?? false,
                    Status = e?.Status ?? "",
                    Student = e?.Student ?? null!,
                    
                    Course  = new EnrolledCoursesDTO()
                    {
                        Id = e?.Course?.Id??0,
                        Title = e?.Course?.Title??"",
                        Description = e?.Course?.Description??"",
                        ThumbnailUrl = e?.Course?.ThumbnailUrl??"",
                        Price = e?.Course?.Price??0,
                        IsFree = e?.Course?.IsFree??false,
                        InstructorId = e?.Course?.InstructorId??0

                    }
                });

                return enrollementsDto;

            }
              
        }


        public async Task<IEnumerable<EnrollmentsByInstructorDTO>> GetEnrollmentsByInstructorIdAsync(int id)
        {
           return await unitOfWork.enrollmentRepository.GetEnrollmentsByInstructorIdAsync(id);
        }


        public async Task<IEnumerable<EnrollmentsByStudentDTO>> GetEnrollmentsByStudentIdAsync(int id)
        {
           return await unitOfWork.enrollmentRepository.GetEnrollmentsByStudentIdAsync(id);
        }
    }
}
