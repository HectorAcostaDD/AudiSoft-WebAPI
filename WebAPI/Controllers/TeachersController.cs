using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TeachersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeachersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Teachers?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers(int pageNumber = 1, int pageSize = 10)
        {
            var teachers =  await _context.Teachers
                .Include(t => t.Scores)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<Teacher>>.Succeed(teachers));
        }

        // GET: api/Teachers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Teacher>> GetTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Scores)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null) return NotFound(ApiResponse<IEnumerable<Teacher>>.Fail($"Profesor con el ID {id} no fue encontrado."));
            return Ok(ApiResponse<Teacher>.Succeed(teacher));
        }

        // POST: api/Teachers
        [HttpPost]
        public async Task<ActionResult<Teacher>> PostTeacher(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            var response = ApiResponse<Teacher>.Succeed(teacher);
            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, response);
        }

        // PUT: api/Teachers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeacher(int id, Teacher teacher)
        {
            if (id != teacher.Id) 
                return BadRequest(ApiResponse<Teacher>.Fail("El ID proporcionado no coincide con el ID del profesor."));

            _context.Entry(teacher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Teachers.Any(e => e.Id == id))
                    return NotFound(ApiResponse<Teacher>.Fail($"No se encontró ningun Profesor con el ID {id} para actualizar."));
                else
                    throw;
            }

            return Ok(ApiResponse<Teacher>.Succeed(teacher));
        }

        // DELETE: api/Teachers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound(ApiResponse<object>.Fail($"No se encontró ningun Profesor con el ID {id} para eliminar."));

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Succeed(null));
        }
    }
}