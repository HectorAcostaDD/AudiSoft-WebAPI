using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Students?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents(int pageNumber = 1, int pageSize = 10)
        {
            var students = await _context.Students
                .Include(s => s.Scores)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<Student>>.Succeed(students));
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.Scores)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound(ApiResponse<IEnumerable<Student>>.Fail($"Estudiante con el ID {id} no fue encontrado."));
            return Ok(ApiResponse<Student>.Succeed(student));
        }

        // POST: api/Students
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            var response = ApiResponse<Student>.Succeed(student);
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, response);
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.Id)
                return BadRequest(ApiResponse<Score>.Fail("El ID proporcionado no coincide con el ID del Estudiante."));

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Scores.Any(e => e.Id == id))
                    return NotFound(ApiResponse<Student>.Fail($"No se encontró ningun Estudiante con el ID {id} para actualizar."));
                else
                    throw;
            }

            return Ok(ApiResponse<Student>.Succeed(student));
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound(ApiResponse<object>.Fail($"No se encontró ningun Esstudiante con el ID {id} para eliminar."));

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Succeed(null));
        }
    }
}