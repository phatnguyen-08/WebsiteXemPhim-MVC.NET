using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsitePhim.Models;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting; // ADD THIS
using Microsoft.AspNetCore.Http; // ADD THIS for IFormFile

// Remove this if you don't need it, or install Microsoft.AspNetCore.WebUtilities for ParseQueryString if truly necessary
// using System.Web; // This is for .NET Framework, not .NET Core

namespace WebsitePhim.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MovieADController : Controller
    {
        private readonly MovieDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // ADD THIS

        public MovieADController(MovieDbContext context, IWebHostEnvironment hostEnvironment) // ADD IWebHostEnvironment
        {
            _context = context;
            _hostEnvironment = hostEnvironment; // Assign it
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.Genre)
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Id)
                .ToListAsync();

            ViewBag.Genres = new SelectList(await _context.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name");
            ViewBag.Years = new SelectList(Enumerable.Range(2000, DateTime.Now.Year - 2000 + 1).Reverse());

            return View(movies);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Genres = new SelectList(await _context.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ReleaseYear,GenreId,VideoUrl")] Movie movie, IFormFile imageFile)
        {
            // You don't need to bind CreatedAt and IsDeleted here, set them manually
            movie.CreatedAt = DateTime.Now;
            movie.IsDeleted = false;

            if (imageFile != null && imageFile.Length > 0)
            {
                // Use _hostEnvironment.WebRootPath for consistent paths
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "movies"); // Correct path

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                movie.ImageUrl = "/images/movies/" + uniqueFileName; // Store relative path
            }
            else
            {
                movie.ImageUrl = "/images/no-image.png";
            }

            if (!string.IsNullOrEmpty(movie.VideoUrl))
            {
                string youtubeId = ExtractYouTubeId(movie.VideoUrl);
                if (!string.IsNullOrEmpty(youtubeId))
                {
                    // Correct YouTube embed URL format
                    movie.VideoUrl = $"https://www.youtube.com/embed/{youtubeId}";
                }
                else
                {
                    movie.VideoUrl = null; // Set to null if invalid
                    ModelState.AddModelError("VideoUrl", "ID Video YouTube không hợp lệ.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm phim mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is not valid, re-populate ViewBag for the view
            ViewBag.Genres = new SelectList(await _context.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name", movie.GenreId);
            return View(movie);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .Include(m => m.Ratings)
                    .ThenInclude(r => r.ApplicationUser)
                .Include(m => m.Comments)
                    .ThenInclude(c => c.ApplicationUser)
                .Include(m => m.Subtitles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                movie.IsDeleted = true;
                _context.Update(movie);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Phim đã được ẩn thành công!";
            }
            return RedirectToAction(nameof(Index)); // Redirect to Index after soft delete
        }

        public async Task<IActionResult> DeletedMovies()
        {
            var movies = await _context.Movies
                .Include(m => m.Genre)
                .Where(m => m.IsDeleted)
                .ToListAsync();
            return View(movies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                movie.IsDeleted = false;
                _context.Update(movie);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Phim đã được khôi phục thành công!";
            }
            return RedirectToAction(nameof(DeletedMovies));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int MovieId, string Content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(Content) || string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được trống hoặc bạn chưa đăng nhập.";
                return RedirectToAction("Details", new { id = MovieId });
            }

            var comment = new Comment
            {
                MovieId = MovieId,
                Content = Content,
                ApplicationUserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Bình luận của bạn đã được thêm!";

            return RedirectToAction("Details", new { id = MovieId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRating(int MovieId, int Value)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Value < 1 || Value > 10 || string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Giá trị đánh giá không hợp lệ (1-10) hoặc bạn chưa đăng nhập.";
                return RedirectToAction("Details", new { id = MovieId });
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.MovieId == MovieId && r.ApplicationUserId == userId);

            if (existingRating != null)
            {
                existingRating.Value = Value;
                existingRating.CreatedAt = DateTime.Now;
                _context.Update(existingRating);
                TempData["SuccessMessage"] = "Đánh giá của bạn đã được cập nhật!";
            }
            else
            {
                var rating = new Rating
                {
                    MovieId = MovieId,
                    Value = Value,
                    ApplicationUserId = userId,
                    CreatedAt = DateTime.Now
                };
                _context.Ratings.Add(rating);
                TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá phim!";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = MovieId });
        }

        public async Task<IActionResult> TopRated()
        {
            var movies = await _context.Movies
                .Where(m => !m.IsDeleted && m.Ratings.Any())
                .Include(m => m.Genre)
                .Select(m => new
                {
                    Movie = m,
                    AverageRating = m.Ratings.Average(r => r.Value)
                })
                .OrderByDescending(x => x.AverageRating)
                .Take(10)
                .Select(x => x.Movie)
                .ToListAsync();

            return View(movies);
        }

        public async Task<IActionResult> HotThisWeek()
        {
            var weekAgo = DateTime.Now.AddDays(-7);

            var movies = await _context.Movies
                .Where(m => !m.IsDeleted && m.Ratings.Any(r => r.CreatedAt >= weekAgo))
                .Include(m => m.Genre)
                .Select(m => new
                {
                    Movie = m,
                    RecentRatingsCount = m.Ratings.Count(r => r.CreatedAt >= weekAgo)
                })
                .OrderByDescending(x => x.RecentRatingsCount)
                .Take(5)
                .Select(x => x.Movie)
                .ToListAsync();

            return View(movies);
        }

        public async Task<IActionResult> ByGenre(int? genreId)
        {
            IQueryable<Movie> movies = _context.Movies
                .Include(m => m.Genre)
                .Where(m => !m.IsDeleted);

            if (genreId.HasValue)
            {
                movies = movies.Where(m => m.GenreId == genreId);
            }

            ViewBag.Genres = new SelectList(await _context.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name", genreId);
            ViewBag.Years = new SelectList(Enumerable.Range(2000, DateTime.Now.Year - 2000 + 1).Reverse());

            return View("Index", await movies.ToListAsync());
        }

        public async Task<IActionResult> ByYear(string year)
        {
            IQueryable<Movie> movies = _context.Movies
                .Include(m => m.Genre)
                .Where(m => !m.IsDeleted);

            if (int.TryParse(year, out int y))
            {
                movies = movies.Where(m => m.ReleaseYear == y);
            }

            ViewBag.Genres = new SelectList(await _context.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name");
            ViewBag.Years = new SelectList(Enumerable.Range(2000, DateTime.Now.Year - 2000 + 1).Reverse(), year);

            return View("Index", await movies.ToListAsync());
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        // Helper method to extract YouTube video ID from various URLs
        private string ExtractYouTubeId(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl)) return null;

            // Handle direct ID
            if (videoUrl.Length == 11 && !videoUrl.Contains("/") && !videoUrl.Contains("=") && !videoUrl.Contains("."))
            {
                return videoUrl;
            }

            Uri uri;
            if (!Uri.TryCreate(videoUrl, UriKind.Absolute, out uri))
            {
                return null;
            }

            string host = uri.Host.ToLower();

            // Handle youtube.com/watch?v=VIDEO_ID
            if (host.Contains("youtube.com") && uri.AbsolutePath.Contains("watch"))
            {
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                if (query.ContainsKey("v"))
                {
                    return query["v"].FirstOrDefault();
                }
            }
            // Handle youtu.be/VIDEO_ID
            else if (host.Contains("youtu.be"))
            {
                string idCandidate = uri.AbsolutePath.Trim('/');
                if (idCandidate.Length == 11)
                {
                    return idCandidate;
                }
            }
            // Handle youtube.com/embed/VIDEO_ID (if already an embed URL)
            else if (host.Contains("youtube.com") && uri.AbsolutePath.Contains("embed"))
            {
                string idCandidate = uri.AbsolutePath.Replace("/embed/", "").Split('?')[0];
                if (idCandidate.Length == 11)
                {
                    return idCandidate;
                }
            }

            return null;
        }
    }
}