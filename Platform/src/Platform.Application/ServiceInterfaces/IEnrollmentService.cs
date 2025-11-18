using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Application.DTOs;


namespace Platform.Application.ServiceInterfaces
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<EnrollementDTO>?> GetAllEnrollementsAsync();
        Task<IEnumerable<EnrollmentsByStudentDTO>> GetEnrollmentsByStudentIdAsync(int id);
        Task<IEnumerable<EnrollmentsByInstructorDTO>> GetEnrollmentsByInstructorIdAsync(int id);
        Task CancelEnrollement(int id);
        Task DeleteEnrollement(int id);
        Task AddAsync(AddEnrollmentDTO enrollment);
    }
}
