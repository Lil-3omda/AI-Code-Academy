using Microsoft.AspNetCore.Identity;
using Platform.Application.DTOs;
using Platform.Core.DTOs;
using Platform.Core.Interfaces;
using Platform.Core.Models;
using Platform.Application.Interfaces;
using System.Linq;
using System.Threading.Tasks;


namespace Platform.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtService _jwtService;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }
        public async Task<(bool Succeeded, string Errors)> RegisterAsync(RegisterStudentDto dto)
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
                return (false, errors);
            }

            return (true, string.Empty);
        }

        public async Task<(bool Succeeded, string Token, string Errors)> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return (false, string.Empty, "Invalid email or password.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
                return (false, string.Empty, "Invalid email or password.");

            var token = _jwtService.GenerateJwtToken(user);
            return (true, token, string.Empty);
        }
    }
}
