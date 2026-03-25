using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsitePhim.Models;

namespace WebsitePhim.ViewComponents
{
    public class TopRatedMoviesViewComponent : ViewComponent
    {
        private readonly MovieDbContext _context;

        public TopRatedMoviesViewComponent(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var movies = await _context.Movies
                .Include(m => m.Ratings)
                .Include(m => m.Genre)
                .Where(m => !m.IsDeleted && m.Ratings.Any())
                .OrderByDescending(m => m.Ratings.Average(r => r.Value))
                .Take(5)
                .ToListAsync();

            return View(movies); // View mặc định là: Views/Shared/Components/TopRatedMovies/Default.cshtml
        }
    }
}
