using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Platform.Core.Interfaces.IRepos;
using Platform.Infrastructure.Data.DbContext;

namespace Platform.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T:class
    {
        protected readonly CourseDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(CourseDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        virtual public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);


        public virtual async Task Delete(int id)
        {
            var entity = await _dbSet.FindAsync(id);

            _dbSet.Remove(entity);

        }

        virtual public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.AsNoTracking().ToListAsync();


        virtual public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);


        virtual public void Update(T entity) => _dbSet.Update(entity);
    }
}
