using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Platform.Application.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string email, string otpCode, string fullName)
        {
            try
            {
                var smtpClient = new SmtpClient(_configuration["Email:SmtpHost"])
                {
                    Port = int.Parse(_configuration["Email:SmtpPort"]),
                    Credentials = new NetworkCredential(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:FromAddress"], "AI Code Academy"),
                    Subject = "Your AI Code Academy Verification Code",
                    Body = CreateOtpEmailBody(otpCode, fullName),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation($"Email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {email}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetOtpAsync(string email, string otpCode, string fullName)
        {
            try
            {
                var smtpClient = new SmtpClient(_configuration["Email:SmtpHost"])
                {
                    Port = int.Parse(_configuration["Email:SmtpPort"]),
                    Credentials = new NetworkCredential(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:FromAddress"], "Netflix Team"),
                    Subject = "Netflix Password Reset Code",
                    Body = CreatePasswordResetEmailBody(otpCode, fullName),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation($"Password reset email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {email}");
                return false;
            }
        }

        private string CreateOtpEmailBody(string otpCode, string fullName)
        {
            return $@"
        <html>
        <body style='font-family: Arial, sans-serif; background-color: #0f0f1a; color: #ffffff;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 30px; background-color: #1a1a2e; border-radius: 12px; box-shadow: 0px 4px 12px rgba(0,0,0,0.6);'>
                <h2 style='color: #00f6ff; text-align:center;'>Welcome to AI Code Academy, {fullName}!</h2>
                <p style='text-align:center; font-size: 16px;'>Please use the following verification code to complete your registration:</p>
                <div style='background: linear-gradient(135deg, #00f6ff, #6a5acd); padding: 20px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 4px; margin: 25px auto; width: fit-content; border-radius: 8px; color: #0f0f1a;'>
                    {otpCode}
                </div>
                <p style='text-align:center;'>⚡ This code will expire in <b>10 minutes</b>.</p>
                <p style='text-align:center; color:#bbbbbb; font-size: 14px;'>If you didn’t request this code, please ignore this email.</p>
                <hr style='margin: 30px 0; border: 0; border-top: 1px solid #333;' />
                <p style='text-align:center; font-size: 14px; color:#888;'>Thanks,<br><b>AI Code Academy Team</b></p>
            </div>
        </body>
        </html>";
        }

        private string CreatePasswordResetEmailBody(string otpCode, string fullName)
        {
            return $@"
        <html>
        <body style='font-family: Arial, sans-serif; background-color: #0f0f1a; color: #ffffff;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 30px; background-color: #1a1a2e; border-radius: 12px; box-shadow: 0px 4px 12px rgba(0,0,0,0.6);'>
                <h2 style='color: #ff4d6d; text-align:center;'>Password Reset Request</h2>
                <p>Hi {fullName},</p>
                <p>We received a request to reset your <b>AI Code Academy account</b> password. Please use the following verification code:</p>
                <div style='background: linear-gradient(135deg, #ff4d6d, #ff914d); padding: 20px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 4px; margin: 25px auto; width: fit-content; border-radius: 8px; color: #0f0f1a;'>
                    {otpCode}
                </div>
                <p style='text-align:center;'>⏳ This code will expire in <b>10 minutes</b>.</p>
                <p style='color:#bbbbbb; font-size: 14px;'>If you didn’t request this reset, please ignore this email and your password will remain unchanged.</p>
                <hr style='margin: 30px 0; border: 0; border-top: 1px solid #333;' />
                <p style='text-align:center; font-size: 14px; color:#888;'>Thanks,<br><b>AI Code Academy Team</b></p>
            </div>
        </body>
        </html>";
        }

    }
}
