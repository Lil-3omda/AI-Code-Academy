using Microsoft.EntityFrameworkCore;
using Platform.Application.IRepos;
using Platform.Core.Interfaces.IRepos;
using Platform.Infrastructure.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Infrastructure.Repositories
{
    public class ModuleRepository : GenericRepository<Platform.Core.Models.Module>, IModuleRepository
    {
        private readonly CourseDbContext dbContext;

        public ModuleRepository(CourseDbContext dbContext ):base(dbContext)
        {
            this.dbContext = dbContext;
        }



        public new async Task<IEnumerable<Platform.Core.Models.Module>> GetAllAsync()
        {
            return await dbContext.Modules
        .Include(m => m.Videos)
        .Include(m => m.Course)
        .ToListAsync();

        }

        public Task<IEnumerable<Core.Models.Module>> GetAllModulesByCrsId(int crsId)
        {

            var modules = dbContext.Modules
                .Where(m => m.CourseId == crsId)
                .Include(m => m.Videos)
                .Include(m => m.Course)
                .AsNoTracking()
                .ToListAsync();
            return Task.FromResult((IEnumerable<Core.Models.Module>)modules.Result);
        }

        public async Task<IEnumerable<Core.Models.Module>> GetAllModulesByInsId(int insId)
        {
            var modules = await dbContext.Modules
                .Include(m => m.Course)
                .ThenInclude(c => c.Instructor)
                .Include(m => m.Videos)
                .Where(m => m.Course.InstructorId == insId)
                .AsNoTracking()
                .ToListAsync();
            return modules;

        }

        public new async Task<Platform.Core.Models.Module?> GetByIdAsync(int id)
        {
            IEnumerable<Platform.Core.Models.Module> allModules = await GetAllAsync();
            return allModules.SingleOrDefault(x => x.Id == id);
        }


        //public void Update(Module entity)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
