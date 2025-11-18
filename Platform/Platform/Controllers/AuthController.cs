using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.DTOs;
using Platform.Core.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Platform.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Platform.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;


namespace Platform.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly CourseDbContext _context;
        private readonly IOtpService _otpService;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration config,
            CourseDbContext context,
            IOtpService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _context = context;
            _otpService = otpService;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterStudentDto dto)
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
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Student");

            // Create Student entry
            var student = new Student
            {
                UserId = user.Id,
                isBlocked = false,
                isDeleted = false
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Student registered successfully" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid credentials");

            // ❌ Don't return token here yet
            // Instead send OTP
            var otp = await _otpService.GenerateOtpAsync(user.Id, user.Email, $"{user.FirstName} {user.LastName}");

            return Ok(new { message = "OTP sent to email, please verify" });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // AppUser.Id
            var entityId = User.FindFirstValue("entityId"); // Student/Instructor.Id
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id.ToString() == entityId);
                if (student == null) return NotFound("Student not found");

                return Ok(new StudentDto
                {
                    Id = student.Id,
                    UserId = student.UserId,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture,
                    Address = user.Address,
                    Gender = user.Gender,
                    IsBlocked = student.isBlocked,
                    IsDeleted = student.isDeleted
                });
            }
            else if (role == "Instructor")
            {
                var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.Id.ToString() == entityId);
                if (instructor == null) return NotFound("Instructor not found");

                return Ok(new InstructorDto
                {
                    Id = instructor.Id,
                    UserId = instructor.UserId,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                });
            }

            return BadRequest("Unknown role");
        }




        private async Task<string> GenerateJwtToken(AppUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var mainRole = userRoles.FirstOrDefault() ?? "User";

            string entityId = user.Id; // default fallback

            if (mainRole == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (student != null)
                    entityId = student.Id.ToString();
            }
            else if (mainRole == "Instructor")
            {
                var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == user.Id);
                if (instructor != null)
                    entityId = instructor.Id.ToString();
            }

            var claims = new List<Claim>
            {
                // ✅ Include both AppUser.Id and EntityId
                new Claim(ClaimTypes.NameIdentifier, user.Id),     // AppUser.Id for Identity
                new Claim("entityId", entityId),                   // Student/Instructor Id for your tables
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", mainRole)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);
            var exists = user != null;

            return Ok(new { email, exists });
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            var otp = await _otpService.GenerateOtpAsync(user.Id, user.Email, $"{user.FirstName} {user.LastName}");
            return Ok(new { message = "OTP sent to email" });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromQuery] string email, [FromQuery] string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            var isValid = await _otpService.ValidateOtpAsync(user.Id, code);
            if (!isValid) return BadRequest("Invalid or expired OTP");

            // ✅ Generate JWT token after successful OTP verification
            var token = await GenerateJwtToken(user); 

            return Ok(new
            {
                message = "OTP verified successfully",
                token
            });
        }

    }
}
