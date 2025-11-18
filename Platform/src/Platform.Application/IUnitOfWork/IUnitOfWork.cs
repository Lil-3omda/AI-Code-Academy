using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Application.IRepos;
using Platform.Core.Interfaces.IRepos;

namespace Platform.Core.Interfaces.IUnitOfWork
{
    public interface IUnitOfWork:IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;

        public IVedioRepository vedioRepository { get; }
        public IEnrollmentRepository enrollmentRepository { get; }
        public IModuleRepository moduleRepository { get; }
        public ICourseRepository courseRepository { get; }
        public IInstructorRepository instructorRepository { get; }

        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken token);
    }
}
