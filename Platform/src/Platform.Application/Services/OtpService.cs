using Microsoft.Extensions.Caching.Memory;
using Platform.Application.ServiceInterfaces;

namespace Platform.Application.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public OtpService(IMemoryCache cache, IEmailService emailService)
        {
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<string> GenerateOtpAsync(string userId, string email, string fullName)
        {
            // Generate random 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Store OTP in cache for 10 minutes
            _cache.Set($"otp_{userId}", otp, TimeSpan.FromMinutes(10));

            // Send OTP via email
            await _emailService.SendOtpEmailAsync(email, otp, fullName);

            return otp; // you might not want to return it in prod for security
        }

        public Task<bool> ValidateOtpAsync(string userId, string otpCode)
        {
            if (_cache.TryGetValue($"otp_{userId}", out string? storedOtp))
            {
                if (storedOtp == otpCode)
                {
                    _cache.Remove($"otp_{userId}");
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}
