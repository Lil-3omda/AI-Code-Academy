using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.ServiceInterfaces
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string userId, string email, string fullName);
        Task<bool> ValidateOtpAsync(string userId, string otpCode);
    }
}
