using Platform.Core.Models;
using Platform.Application.DTOs;
using Platform.Application.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Json;
using System.Text.Json;
using Platform.Core.Interfaces.IUnitOfWork;

namespace Platform.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork; // You must have this (repositories)
        private readonly string _apiKey;
        private readonly string _integrationId;
        private readonly string _iframeId;

        public PaymentService(
            HttpClient httpClient,
            IConfiguration config,
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _config = config;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _apiKey = _config["Paymob:ApiKey"];
            _integrationId = _config["Paymob:IntegrationId"];
            _iframeId = _config["Paymob:CardIframeId"];
        }

        public async Task<PaymentResponseDto> InitiatePaymentAsync(CreatePaymentDTO dto)
        {
            try
            {
                // 1️⃣ Get course and student
                var course = await _unitOfWork.courseRepository.GetByIdAsync(dto.CourseId);
                var student = await _unitOfWork.Repository<Student>().GetByIdAsync(dto.StudentId);

                if (course == null)
                    return new PaymentResponseDto { Success = false, Message = "Course not found" };

                if (student == null)
                    return new PaymentResponseDto { Success = false, Message = "Student not found" };

                var user = await _userManager.FindByIdAsync(student.UserId);
                if (user == null)
                    return new PaymentResponseDto { Success = false, Message = "User not found" };

                // 2️⃣ Create enrollment record with Pending status
                var enrollment = new Enrollment
                {
                    StdId = student.Id,
                    CourseId = course.Id,
                    Status = "Pending",
                    ProgressPercentage = 0
                };

                await _unitOfWork.enrollmentRepository.AddAsync(enrollment);
                await _unitOfWork.SaveChangesAsync();

                // 3️⃣ If course is free → no payment needed
                if (course.IsFree)
                {
                    enrollment.Status = "Paid";
                    await _unitOfWork.SaveChangesAsync();
                    return new PaymentResponseDto
                    {
                        Success = true,
                        Message = "Course enrolled successfully (Free course)"
                    };
                }

                // 4️⃣ Start Paymob flow
                var token = await GetAuthTokenAsync();

                var paymobOrderId = await RegisterOrderAsync(token, course, enrollment.Id);

                var paymentKey = await GeneratePaymentKeyAsync(token, paymobOrderId, course, user);

                var iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_iframeId}?payment_token={paymentKey}";

                return new PaymentResponseDto
                {
                    Success = true,
                    PaymentUrl = iframeUrl,
                    TransactionId = paymobOrderId,
                    Message = "Payment initiated successfully"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", new { api_key = _apiKey });
            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return json.RootElement.GetProperty("token").GetString();
        }

        private async Task<string> RegisterOrderAsync(string token, Courses course, int enrollmentId)
        {
            var orderRequest = new
            {
                auth_token = token,
                delivery_needed = "false",
                amount_cents = (int)(course.Price * 100),
                currency = "EGP",
                merchant_order_id = $"ENROLL_{enrollmentId}_{course.Id}",
                items = new[]
                {
                    new { name = course.Title, amount_cents = (int)(course.Price * 100), description = "Course Payment", quantity = 1 }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", orderRequest);
            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return json.RootElement.GetProperty("id").GetInt32().ToString();
        }

        private async Task<string> GeneratePaymentKeyAsync(string token, string paymobOrderId, Courses course, AppUser user)
        {
            var billing = new
            {
                last_name = string.IsNullOrEmpty(user.LastName) ? "has not last name" : user.LastName,
                first_name = string.IsNullOrEmpty(user.FirstName) ? "N/A" : user.FirstName,
                email = user.Email ?? "email@example.com",
                phone_number = user.PhoneNumber ?? "+201000000000",
                street = "N/A",
                building = "N/A",
                floor = "N/A",
                apartment = "N/A",
                city = "Cairo",
                state = "Cairo",
                country = "EGYPT",
                postal_code = "00000"
            };

            var keyRequest = new
            {
                auth_token = token,
                amount_cents = (int)(course.Price * 100),
                expiration = 3600,
                order_id = paymobOrderId,
                billing_data = billing,
                currency = "EGP",
                integration_id = int.Parse(_integrationId)
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", keyRequest);
            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return json.RootElement.GetProperty("token").GetString();
        }

        public async Task<bool> ValidatePaymentCallback(string payload)
        {
            try
            {
                var json = JsonDocument.Parse(payload);
                var root = json.RootElement;

                bool success = root.TryGetProperty("success", out var successElement)
                               && bool.TryParse(successElement.GetString(), out var isSuccess)
                               && isSuccess;

                if (!root.TryGetProperty("merchant_order_id", out var merchantIdElement))
                    return false;

                var merchantIdStr = merchantIdElement.GetString();
                var parts = merchantIdStr?.Split('_');
                if (parts?.Length < 3 || !int.TryParse(parts[1], out int enrollmentId))
                    return false;

                var enrollment = await _unitOfWork.enrollmentRepository.GetByIdAsync(enrollmentId);
                if (enrollment == null)
                    return false;

                if (success)
                {
                    enrollment.Status = "Paid";
                }
                else
                {
                    enrollment.Status = "Canceled";
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
