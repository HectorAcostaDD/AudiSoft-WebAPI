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
    public class ScoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Scores?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Score>>> GetScores(int pageNumber = 1, int pageSize = 10)
        {
            var scores = await _context.Scores
                .Include(s => s.Teacher)
                .Include(s => s.Student)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<Score>>.Succeed(scores));
        }

        // GET: api/Scores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Score>> GetScore(int id)
        {
            var score = await _context.Scores
                .Include(s => s.Teacher)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (score == null) return NotFound(ApiResponse<IEnumerable<Score>>.Fail($"Nota con el ID {id} no fue encontrado."));
            return Ok(ApiResponse<Score>.Succeed(score));
        }

        // POST: api/Scores
        [HttpPost]
        public async Task<ActionResult<Score>> PostScore(Score score)
        {
            _context.Scores.Add(score);
            await _context.SaveChangesAsync();
            var response = ApiResponse<Score>.Succeed(score);
            return CreatedAtAction(nameof(GetScore), new { id = score.Id }, response);
        }

        // PUT: api/Scores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScore(int id, Score score)
        {
            if (id != score.Id)
                return BadRequest(ApiResponse<Score>.Fail("El ID proporcionado no coincide con el ID de la Nota."));

            _context.Entry(score).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Scores.Any(e => e.Id == id))
                    return NotFound(ApiResponse<Score>.Fail($"No se encontró ninguna Nota con el ID {id} para actualizar."));
                else
                    throw;
            }

            return Ok(ApiResponse<Score>.Succeed(score));
        }

        // DELETE: api/Scores/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteScore(int id)
        {
            var score = await _context.Scores.FindAsync(id);
            if (score == null)
                return NotFound(ApiResponse<object>.Fail($"No se encontró ninguna Nota con el ID {id} para eliminar."));

            _context.Scores.Remove(score);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Succeed(null));
        }
    }
}