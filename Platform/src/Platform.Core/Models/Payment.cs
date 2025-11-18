using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        // العلاقات
        public virtual AppUser User { get; set; }
        public virtual Courses Course { get; set; }
    }
}
