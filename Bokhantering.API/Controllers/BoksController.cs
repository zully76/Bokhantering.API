using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bokhantering.API.Data;
using Bokhantering.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bokhantering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BoksController : ControllerBase
    {
        private readonly BokhanteringDbContext _context;

        public BoksController(BokhanteringDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Bok>>> GetBoks()
        {
            return await _context.Boks.ToListAsync();
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Bok>> GetBok(int id)
        {
            var bok = await _context.Boks.FindAsync(id);

            if (bok == null)
            {
                return NotFound();
            }

            return bok;

        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Bok>> PostBok(Bok bok)
        {
            _context.Boks.Add(bok);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBok), new { Id = bok.Id}, bok);

        }
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutBok(int id, Bok bok)
        {
            if (id != bok.Id)
            {
                return BadRequest();
            }

            _context.Entry(bok).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();

        }
        [HttpDelete("{id}")]
        [Authorize]

        public async Task<IActionResult> DeleteBok(int id)
        {
            var bok = await _context.Boks.FindAsync(id);
            if (bok == null)
            {

                return NotFound();

            }

            _context.Boks.Remove(bok);
            await _context.SaveChangesAsync();

            return NoContent();

        }


    }
}
