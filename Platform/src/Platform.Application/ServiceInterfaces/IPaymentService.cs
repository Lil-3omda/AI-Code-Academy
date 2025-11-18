using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Application.DTOs;

namespace Platform.Application.ServiceInterfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> InitiatePaymentAsync(CreatePaymentDTO request);
        Task<bool> ValidatePaymentCallback(string payload);
    }
}
