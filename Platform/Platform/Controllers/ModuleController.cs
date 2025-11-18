using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platform.Application.DTOs;
using Platform.Application.ServiceInterfaces;
using Platform.Core.Models;
using Platform.Infrastructure.Data.DbContext;
using Platform.Infrastructure.UnitOfWork;
using System.Reflection;
using System.Threading.Tasks;

namespace Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly CourseDbContext courseDbContext;
        private readonly IModuleService moduleService;

        public ModuleController(CourseDbContext courseDbContext , IModuleService moduleService )
        {
            this.courseDbContext = courseDbContext;
            this.moduleService = moduleService;
        }


        //GetAllModuleS

        [HttpGet]
        public async Task<IActionResult> GetAllModules()
        {

            IEnumerable<ModulesDTO> modules =await moduleService.GetAllModules();
            return Ok(modules);

        }




        //GetModuleByID

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllModules(int id)
        {
            ModulesDTO module = await moduleService.GetModulesById(id);

            return Ok(module);

        }


        //Add Module

        [HttpPost]
        public async Task<IActionResult> AddModule(AddModuleDTO module)
        {

            try
            {
                await moduleService.AddModule(module);
                return Ok(new { Message = "Module created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                // send a 400 BadRequest with the error message
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // generic error handler
                return StatusCode(500, new { Message = "An error occurred", Details = ex.Message });
            }

        }



        //Edit Module

        [HttpPut]
        public async Task<IActionResult> EditModule(AddModuleDTO modulesDTO)
        {
            await moduleService.UpdateModule(modulesDTO);
            return Ok(new { message = "Module Updated Successfully" });

        }


        //Delete Module

        [HttpDelete]
        public async Task<IActionResult> DeleteModule(int id)
        {

            await moduleService.DeleteModule(id);
            
            return Ok(new { message = "Module Deleted Successfully" });
        }

        //Get Modules By Course ID
        [HttpGet("GetModulesByCrsID/{crsId}")]
        public async Task<IActionResult> GetModulesByCrsID(int crsId)
        {
            IEnumerable<ModulesDTO> modules = await moduleService.GetModulesByCrsID(crsId);
            return Ok(modules);
        }

        // Get Modules by Instructor ID
        [HttpGet("GetModulesByInstID/{instId}")]
        public async Task<IActionResult> GetModulesByInstID(int instId)
        {
            IEnumerable<ModulesDTO> modules = await moduleService.GetModulesByInsID(instId);
            return Ok(modules);
        }
    }
}
