using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Core.DTOs;
using Platform.Application.DTOs;


namespace Platform.Application.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, string Errors)> RegisterAsync(RegisterStudentDto dto);
        Task<(bool Succeeded, string Token, string Errors)> LoginAsync(LoginDto dto);
        Task<bool> EmailExistsAsync(string email);


    }
}
