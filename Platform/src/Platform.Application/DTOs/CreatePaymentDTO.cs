using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.DTOs
{
    public class CreatePaymentDTO
    {
        public int CourseId { get; set; }         // Which course student wants to buy
        public int StudentId { get; set; }        // Which student is paying
        public string PaymentMethod { get; set; }
    }
}
