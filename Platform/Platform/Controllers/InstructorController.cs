using Microsoft.AspNetCore.Mvc;
using Platform.Application.DTOs;
using Platform.Core.DTOs;
using Platform.Core.Interfaces;

namespace Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorController : ControllerBase
    {
        private readonly IInstructorService _service;

        public InstructorController(IInstructorService service)
        {
            _service = service;
        }

        [HttpPost("register-new")]
        public async Task<IActionResult> RegisterNew([FromBody] InstructorRegisterDto dto)
        {
            try
            {
                var instructor = await _service.RegisterInstructorAsync(dto);
                return Ok(instructor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register-existing")]
        public async Task<IActionResult> RegisterExisting([FromBody] RegisterInstructorIfAccountExistsDto dto)
        {
            var result = await _service.RegisterInstructor2Async(dto);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok("Instructor registered successfully.");
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var instructor = await _service.GetInstructorByIdAsync(id);
            if (instructor == null) return NotFound();
            return Ok(instructor);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var instructors = await _service.GetAllInstructorsAsync();
            return Ok(instructors);
        }

        [HttpPut("verify/{id:int}")]
        public async Task<IActionResult> Verify(int id)
        {
            var result = await _service.VerifyInstructorAsync(id);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok("Instructor verified successfully.");
        }
    }
}
