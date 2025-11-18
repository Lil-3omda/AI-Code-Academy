using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Core.Interfaces.IRepos
{
    public interface IInstructorRepository
    {
        Task AddAsync(Instructor instructor);
        Task<Instructor?> GetByIdAsync(int id);
        Task<IEnumerable<Instructor>> GetAllAsync();
        Task UpdateAsync(Instructor instructor);
        Task SaveChangesAsync();
    }
}
