using Microsoft.AspNetCore.Mvc;
using Platform.Application.DTOs;
using Platform.Application.DTOs;
using Platform.Application.ServiceInterfaces;
using Platform.Application.ServiceInterfaces;
using System.Threading.Tasks;

namespace Platform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service)
        {
            _service = service;
        }

        // GET: api/course
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _service.GetAllAsync();
            return Ok(courses);
        }

        // GET: api/course/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _service.GetByIdAsync(id);
            if (course == null) return NotFound();
            return Ok(course);
        }



        [HttpGet("GetByCategoryName/{name}")]
        public async Task<IActionResult> GetByCategoryName(string name)
        {
            var Course= await _service.GetByCategoryNameAsync(name);
            return Ok(Course);
        }

        // POST: api/course
        [Consumes("multipart/form-data")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }


        // PUT: api/course/{id}

        [Consumes("multipart/form-data")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] CourseUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound();

            return Ok(updated);
        }

        // DELETE: api/course/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        // GET: api/course/instructor/{instructorId}
        [HttpGet("GetCourseByInsId/{instructorId}")]
        public async Task<IActionResult> GetCoursesByInstructorId(int instructorId)
        {
            var courses = await _service.GetCoursesByInstructorId(instructorId);
            return Ok(courses);
        }


    }
}
