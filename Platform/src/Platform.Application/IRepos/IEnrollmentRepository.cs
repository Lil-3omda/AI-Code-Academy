using Platform.Application.DTOs;
using Platform.Core.Interfaces.IRepos;
using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.IRepos
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        //Task AddAsync(AddEnrollmentDTO entity);
        //Task<IEnumerable<EnrollementDTO>> GetAllEnrollementsAsync();
        Task<IEnumerable<EnrollmentsByStudentDTO>> GetEnrollmentsByStudentIdAsync(int id);
        Task<IEnumerable<EnrollmentsByInstructorDTO>> GetEnrollmentsByInstructorIdAsync(int id);
        Task CancelEnrollement(int id);
        //Task DeleteEnrollement(DeleteEnrollementDTO enrollment);
    }
}
