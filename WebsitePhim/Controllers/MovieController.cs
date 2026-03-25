using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsitePhim.Models;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

public class MovieController : Controller
{
    private readonly MovieDbContext _context;

    public MovieController(MovieDbContext context)
    {
        _context = context;
    }

    // 🔹 Hiển thị danh sách phim (chỉ hiển thị phim chưa bị xóa)
    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies
            .Include(m => m.Genre)
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.Id)
            .Take(12)
            .ToListAsync();

        ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
        ViewBag.Years = new SelectList(Enumerable.Range(2000, 26).Reverse());

        // 🎯 Gợi ý phim theo thể loại gần nhất đã xem
        int? genreId = HttpContext.Session.GetInt32("LastWatchedGenreId");

        ViewBag.SuggestedMovies = genreId != null
            ? _context.Movies
                .Where(m => m.GenreId == genreId && !m.IsDeleted)
                .OrderByDescending(m => m.Ratings.Count())
                .Take(6)
                .ToList()
            : new List<Movie>();

        return View(movies);
    }



    // 🔹 Hiển thị form thêm phim
    public IActionResult Create()
    {
        ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
        return View();
    }

    // 🔹 Xử lý thêm phim vào Database
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Movie movie, IFormFile imageFile)
    {
        if (ModelState.IsValid)
        {
            // Chuyển đổi ID video YouTube thành URL nhúng
            if (!string.IsNullOrEmpty(movie.VideoUrl))
            {
                movie.VideoUrl = $"https://www.youtube.com/embed/{movie.VideoUrl}";
            }

            // Xử lý upload ảnh
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                movie.ImageUrl = "/images/movies/" + fileName;
            }

            _context.Add(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(movie);
    }

    // 🔹 Hiển thị chi tiết phim
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies
            .Include(m => m.Genre)
            .Include(m => m.Ratings)
            .Include(m => m.Comments)
            .Include(m => m.Subtitles)
            .Include(m => m.Episodes)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (movie == null) return NotFound();
        HttpContext.Session.SetInt32("LastWatchedGenreId", movie.GenreId);
        return View(movie);
    }

    // 🔹 Hiển thị form sửa phim
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var movie = await _context.Movies
            .Include(m => m.Genre)            // Load Genre để hiển thị đúng
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null || movie.IsDeleted)
            return NotFound();

        ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
        return View(movie);
    }

    // 🔹 Xử lý cập nhật phim
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Movie movie, IFormFile imageFile)
    {
        if (id != movie.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // Chuyển đổi ID video YouTube thành URL nhúng
            if (!string.IsNullOrEmpty(movie.VideoUrl))
            {
                movie.VideoUrl = $"https://www.youtube.com/embed/{movie.VideoUrl}";
            }

            // Xử lý upload ảnh mới (nếu có)
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/movies", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                movie.ImageUrl = "/images/movies/" + fileName;
            }

            _context.Update(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(movie);
    }

    // 🔹 Hiển thị xác nhận xóa phim
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies.FindAsync(id);
        if (movie == null || movie.IsDeleted) return NotFound(); // Không cho xóa phim đã bị ẩn

        return View(movie);
    }

    // 🔹 Xử lý ẩn phim thay vì xóa hoàn toàn
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            movie.IsDeleted = true; // Đánh dấu là đã xóa
            _context.Update(movie);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // 🔹 Hiển thị danh sách phim đã bị ẩn
    public async Task<IActionResult> DeletedMovies()
    {
        var movies = await _context.Movies.Where(m => m.IsDeleted).ToListAsync();
        return View(movies);
    }

    // 🔹 Khôi phục phim đã bị ẩn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            movie.IsDeleted = false; // Khôi phục phim
            _context.Update(movie);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> AddComment(int MovieId, string Content)
    {
        if (string.IsNullOrWhiteSpace(Content)) return RedirectToAction("Details", new { id = MovieId });

        var comment = new Comment
        {
            MovieId = MovieId,
            Content = Content,
            CreatedAt = DateTime.Now
        };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = MovieId });
    }
    [HttpPost]
    public async Task<IActionResult> AddRating(int MovieId, int Value)
    {
        if (Value < 1 || Value > 10) return RedirectToAction("Details", new { id = MovieId });

        var rating = new Rating
        {
            MovieId = MovieId,
            Value = Value,
            CreatedAt = DateTime.Now
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = MovieId });
    }
    public async Task<IActionResult> TopRated()
    {
        var movies = await _context.Movies
            .Where(m => !m.IsDeleted && m.Ratings.Any())
            .Include(m => m.Genre)
            .Include(m => m.Ratings)
            .OrderByDescending(m => m.Ratings.Average(r => r.Value))
            .Take(10)
            .ToListAsync();

        return View(movies);
    }
    public async Task<IActionResult> HotThisWeek()
    {
        var weekAgo = DateTime.Now.AddDays(-7);

        var movies = await _context.Movies
            .Where(m => !m.IsDeleted && m.Ratings.Any(r => r.CreatedAt >= weekAgo))
            .Include(m => m.Ratings)
            .Include(m => m.Genre)
            .OrderByDescending(m => m.Ratings.Count(r => r.CreatedAt >= weekAgo))
            .Take(5)
            .ToListAsync();

        return View(movies);
    }

    public async Task<IActionResult> ByGenre(int? genreId)
    {
        var movies = _context.Movies
            .Include(m => m.Genre)
            .Where(m => !m.IsDeleted);

        if (genreId.HasValue)
            movies = movies.Where(m => m.GenreId == genreId);

        ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
        ViewBag.Years = new SelectList(Enumerable.Range(2000, 26).Reverse());

        return View("Index", await movies.ToListAsync());
    }

    public async Task<IActionResult> ByYear(string year)
    {
        var movies = _context.Movies
            .Include(m => m.Genre)
            .Where(m => !m.IsDeleted);

        if (int.TryParse(year, out int y))
            movies = movies.Where(m => m.ReleaseYear == y);

        ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
        ViewBag.Years = new SelectList(Enumerable.Range(2000, 26).Reverse());

        return View("Index", await movies.ToListAsync());
    }
    public IActionResult Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return RedirectToAction("Index");

        var movies = _context.Movies
            .Include(m => m.Genre)
            .Where(m => m.Title.Contains(query) && !m.IsDeleted)
            .ToList();
        ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
        ViewBag.Years = new SelectList(Enumerable.Range(2000, 26).Reverse());
        return View("Index", movies);
    }
}
