using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.ServiceInterfaces
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string otpCode, string fullName);
        Task<bool> SendPasswordResetOtpAsync(string email, string otpCode, string fullName);
    }
}
