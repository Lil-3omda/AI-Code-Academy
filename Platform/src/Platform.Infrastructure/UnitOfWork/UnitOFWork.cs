using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Platform.Application.IRepos;
using Platform.Core.Interfaces.IRepos;
using Platform.Core.Interfaces.IUnitOfWork;
using Platform.Core.Models;
using Platform.Infrastructure.Data.DbContext;
using Platform.Infrastructure.Repositories;

namespace Platform.Infrastructure.UnitOfWork
{
    public class UnitOFWork : IUnitOfWork
    {
        protected readonly CourseDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly Dictionary<Type, object> _repositories = new();


        public UnitOFWork(CourseDbContext context)
        {
            _context = context;
        }


        public IVedioRepository vedioRepository => new VideoRepository(_context);

        public IEnrollmentRepository enrollmentRepository => new EnrollmentRepository(_context);
        public ICategoryRepository categoryRepository => new CategoryRepository(_context);
        public IModuleRepository moduleRepository => new ModuleRepository(_context);

        public ICourseRepository courseRepository => new CourseRepository(_context);

        public IInstructorRepository instructorRepository => new InstructorRepository(_context);

        public void Dispose()
        {
            _context.Dispose();
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repositories.TryGetValue(typeof(T), out var repo))
                return (IGenericRepository<T>)repo;

            var newRepo = new GenericRepository<T>(_context);
            _repositories.Add(typeof(T), newRepo);
            return newRepo;
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken token)
        {
            return _context.SaveChangesAsync(token);
        }
        //
    }
}
