using Microsoft.EntityFrameworkCore;
using Platform.Application.DTOs;
using Platform.Application.IRepos;
using Platform.Core.Models;
using Platform.Infrastructure.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Infrastructure.Repositories
{

    public class EnrollmentRepository : GenericRepository<Enrollment> ,  IEnrollmentRepository
    {
        private readonly CourseDbContext courseDbContext;

        public EnrollmentRepository(CourseDbContext courseDbContext) : base(courseDbContext)
        {
            this.courseDbContext = courseDbContext;
        }

   
        public async Task CancelEnrollement(int id)
        {
            Enrollment enroll = await courseDbContext.Enrollments.SingleOrDefaultAsync(e => e.Id == id);
            enroll.IsCanceled = true;
        }

        public override async Task<IEnumerable<Enrollment>> GetAllAsync()
        {
            IEnumerable<Enrollment> enrollments = await courseDbContext.Enrollments.Include(e=>e.Course).Where(e => e.IsDeleted == false).ToListAsync();
            return enrollments;
        }



        public new async Task Delete(int id)
        {
            Enrollment enroll = await courseDbContext.Enrollments.SingleOrDefaultAsync(e => e.Id == id);
            enroll.IsDeleted = true;

        }

        

        //public Task<IEnumerable<Enrollment>> GetAllAsync()
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<IEnumerable<EnrollementDTO>> GetAllEnrollementsAsync()
        //{
        //    List<EnrollementDTO> enrollmentDTOs = await courseDbContext.Enrollments.Select(e => new EnrollementDTO
        //    {
        //        Id = e.Id,
        //        EnrollmentDate = e.EnrollmentDate,
        //        ProgressPercentage = e.ProgressPercentage,
        //        StdId = e.StdId,
        //        CourseId = e.CourseId,
        //        IsDeleted = e.IsDeleted,
        //        IsCanceled = e.IsCanceled,
        //        Status = e.Status,
        //        Student = e.Student,
        //        Course = new EnrolledCoursesDTO()
        //        {
        //            Id = e.Course.Id,
        //            Title = e.Course.Title,
        //            Description = e.Course.Description,
        //            ThumbnailUrl = e.Course.ThumbnailUrl,
        //            Price = e.Course.Price,
        //            IsFree = e.Course.IsFree,
        //            InstructorId = e.Course.InstructorId

        //        }

        //    }).ToListAsync();



        //    return enrollmentDTOs;
        //}

        ////public Task<Enrollment?> GetByIdAsync(int id)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<IEnumerable<EnrollmentsByInstructorDTO>> GetEnrollmentsByInstructorIdAsync(int id)
        {
            List<EnrollmentsByInstructorDTO> enrollment =await courseDbContext.Enrollments.Include(e=>e.Course).Select(e => new EnrollmentsByInstructorDTO
            {
                Id = e.Id,
                EnrollmentDate = e.EnrollmentDate,
                ProgressPercentage = e.ProgressPercentage,
                StdId = e.StdId,
                CourseId = e.CourseId,
                IsDeleted = e.IsDeleted,
                IsCanceled = e.IsCanceled,
                Status = e.Status,
                Course = new EnrolledCoursesDTO()
                {
                    Id = e.Course.Id,
                    Title = e.Course.Title,
                    Description = e.Course.Description,
                    ThumbnailUrl = e.Course.ThumbnailUrl,
                    Price = e.Course.Price,
                    IsFree = e.Course.IsFree,
                    InstructorId = e.Course.InstructorId


                }

            }).Where(e => e.Course.InstructorId == id).ToListAsync();


            return enrollment;
        }

        public async Task<IEnumerable<EnrollmentsByStudentDTO>> GetEnrollmentsByStudentIdAsync(int id)
        {
            List<EnrollmentsByStudentDTO> enrollment = await courseDbContext.Enrollments.Include(e=>e.Course).Select(S => new EnrollmentsByStudentDTO()
            {
                Id = id,
                EnrollmentDate = S.EnrollmentDate,
                ProgressPercentage = S.ProgressPercentage,
                StdId = S.StdId,
                CourseId = S.CourseId,
                CourseTitle = S.Course.Title,
                CourseDescription = S.Course.Description,
                CourseImageUrl = S.Course.ThumbnailUrl,
                IsDeleted = S.IsDeleted,
                IsCanceled = S.IsCanceled,
                Status = S.Status,
                Student = S.Student
            }).Where(s => s.StdId == id).ToListAsync();

            return enrollment;
        }

        
    }
}
