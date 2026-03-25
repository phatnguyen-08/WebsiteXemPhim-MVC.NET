using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsitePhim.Models;

namespace WebsitePhim.Controllers
{
    public class EpisodeController : Controller
    {
        private readonly MovieDbContext _context;

        public EpisodeController(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Play(int id)
        {
            var episode = await _context.Episodes
                .Include(e => e.Movie)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (episode == null)
                return NotFound();

            return View(episode);
        }
    }
}