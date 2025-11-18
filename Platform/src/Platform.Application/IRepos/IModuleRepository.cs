using Platform.Core.Interfaces.IRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.IRepos
{
    public interface IModuleRepository : IGenericRepository<Platform.Core.Models.Module>
    {
        Task<IEnumerable<Platform.Core.Models.Module>> GetAllModulesByCrsId(int crsId);
        Task<IEnumerable<Platform.Core.Models.Module>> GetAllModulesByInsId(int insId);

    }
}
