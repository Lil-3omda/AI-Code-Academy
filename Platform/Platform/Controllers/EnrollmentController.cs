using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platform.Application.DTOs;
using Platform.Application.ServiceInterfaces;
using Platform.Core.Models;
using Platform.Infrastructure.Data.DbContext;
using System.Threading.Tasks;

namespace Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly CourseDbContext courseDbContext;
        private readonly IEnrollmentService enrollmentService;

        public EnrollmentController(CourseDbContext courseDbContext  , IEnrollmentService enrollmentService)
        {
            this.courseDbContext = courseDbContext;
            this.enrollmentService = enrollmentService;
        }
        





        [HttpGet]
        public async Task<IActionResult> GetAllEnrollments()
        {

            IEnumerable<EnrollementDTO> enrollmentDTOs=await enrollmentService.GetAllEnrollementsAsync();   
            return Ok(enrollmentDTOs); 
        }



        //GET Enrollments Of Specific Student By UserId

        [HttpGet("GetEnrollmentsByStudentId/{id}")]
        public async Task<IActionResult> GetEnrollmentsByStudentId(int id)
        {
           IEnumerable<EnrollmentsByStudentDTO> EnrollmentsByStudentDTO= await enrollmentService.GetEnrollmentsByStudentIdAsync(id);

            return Ok(EnrollmentsByStudentDTO);
        }




        //GET Enrollments Of Specific instructor By UserId

        [HttpGet("GetEnrollmentsByInstructorId/{id}")]
        public async Task<IActionResult> GetEnrollmentsByInstructorId(int id)
        {

          IEnumerable<EnrollmentsByInstructorDTO> enrollments = await  enrollmentService.GetEnrollmentsByInstructorIdAsync(id);
            //List<Enrollment> enrollment = courseDbContext.Enrollments.Include(e => e.Course).Where(s => s.Course.InstructorId == id).ToList();
            //List<EnrollmentsByInstructorDTO> enrollment= courseDbContext.Enrollments.Select(e=>new EnrollmentsByInstructorDTO {
            //    Id=e.Id,
            //    EnrollmentDate=e.EnrollmentDate,
            //    ProgressPercentage=e.ProgressPercentage,
            //    StdId=e.StdId,
            //    CourseId=e.CourseId,
            //    IsDeleted=e.IsDeleted,
            //    IsCanceled=e.IsCanceled,
            //    Status=e.Status,
            //    Course=new EnrolledCoursesDTO()
            //    {
            //        Id=e.Course.Id, 
            //        Title=e.Course.Title,
            //        Description=e.Course.Description,
            //        ThumbnailUrl=e.Course.ThumbnailUrl,
            //        Price=e.Course.Price,
            //        IsFree=e.Course.IsFree,
            //        InstructorId=e.Course.InstructorId


            //    }

            //}).Where(e=>e.Course.InstructorId==id).ToList();    


            return Ok(enrollments);
        }



        [HttpPost("AddEnrollment")]
        public  async Task<IActionResult> AddEnrollment(AddEnrollmentDTO enrollment)
        {
            await enrollmentService.AddAsync(enrollment);
            return Ok("Enrollment Added Successfully");
        }



        [HttpPut("DeleteEnrollment/{id}")]
        public async Task<IActionResult> DeleteEnrollement(int id)
        {
            await enrollmentService.DeleteEnrollement(id);

            return Ok("Enrollment Deleted Successfully");

        }





        [HttpPut("CancelEnrollment/{id}")]
        public async Task<IActionResult> CancelEnrollement(int id)
        {
            await enrollmentService.CancelEnrollement(id);
            return Ok("Enrollment Canceled Successfully");

        }



    }
}
