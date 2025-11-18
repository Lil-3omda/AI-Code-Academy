using Microsoft.AspNetCore.Mvc;
using Platform.Application.DTOs;
using Platform.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.ServiceInterfaces
{
    public interface IModuleService
    {
        Task<IEnumerable<ModulesDTO>?> GetAllModules();
        Task<ModulesDTO?> GetModulesById(int id);
        Task AddModule(AddModuleDTO addModuleDTO);
        Task UpdateModule(AddModuleDTO addModuleDTO);
        Task DeleteModule(int id);
        Task<IEnumerable<ModulesDTO>> GetModulesByCrsID(int crsId);
        Task<IEnumerable<ModulesDTO>> GetModulesByInsID(int insId);

    }
}
