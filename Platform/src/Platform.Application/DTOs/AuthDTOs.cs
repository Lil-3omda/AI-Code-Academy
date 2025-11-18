using Platform.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class RegisterStudentDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string Address { get; set; }
        public string Gender { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // For Returning Student Info
    public class StudentDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }

        public string ProfilePicture { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }

        public bool IsBlocked { get; set; }
        public bool IsDeleted { get; set; }
    }
    
    public class RegisterInstructorIfAccountExistsDto
    {
        public string LinkedIn { get; set; }
        public string Qualifications { get; set; }
        public string UserId { get; set; } 
    }

    public class InstructorRegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }  
        public string? Address { get; set; }
        public string? Gender { get; set; }

        public string LinkedIn { get; set; }
        public string Qualifications { get; set; }
    }
    public class InstructorReadDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string LinkedIn { get; set; }
        public bool IsVerified { get; set; }
        public string Qualifications { get; set; }
    }
    public class InstructorDto
    {
        public int Id { get; set; }
        public string LinkedIn { get; set; }
        public string Qualifications { get; set; }
        public bool IsVerified { get; set; }

        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public ICollection<CourseDto> Courses { get; set; }
    }
}
