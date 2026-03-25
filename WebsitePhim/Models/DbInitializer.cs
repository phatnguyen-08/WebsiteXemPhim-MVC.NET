using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using WebsitePhim.Models;

namespace WebsitePhim.Data
{
    public static class DbInitializer
    {
        public static async Task Seed(IApplicationBuilder applicationBuilder)
        {
            using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();

            var context = serviceScope.ServiceProvider.GetService<MovieDbContext>();
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.EnsureCreated();

            // ─── 1. ROLES ───────────────────────────────────────────────
            foreach (var roleName in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // ─── 2. USERS ───────────────────────────────────────────────
            ApplicationUser adminUser = null;
            ApplicationUser normalUser = null;

            if (await userManager.FindByEmailAsync("admin@websitephim.com") == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@websitephim.com",
                    Email = "admin@websitephim.com",
                    FullName = "Quản trị viên",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                adminUser = await userManager.FindByEmailAsync("admin@websitephim.com");
            }

            if (await userManager.FindByEmailAsync("user@websitephim.com") == null)
            {
                normalUser = new ApplicationUser
                {
                    UserName = "user@websitephim.com",
                    Email = "user@websitephim.com",
                    FullName = "Người dùng mẫu",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(normalUser, "User@123");
                await userManager.AddToRoleAsync(normalUser, "User");
            }
            else
            {
                normalUser = await userManager.FindByEmailAsync("user@websitephim.com");
            }

            // ─── 3. GENRES ──────────────────────────────────────────────
            if (!context.Genres.Any())
            {
                context.Genres.AddRange(
                    new Genre { Name = "Hành động" },
                    new Genre { Name = "Kinh dị" },
                    new Genre { Name = "Tình cảm" },
                    new Genre { Name = "Viễn tưởng" }
                );
                context.SaveChanges();
            }

            // ─── 4. MOVIES ──────────────────────────────────────────────
            if (!context.Movies.Any())
            {
                var action = context.Genres.First(g => g.Name == "Hành động");
                var horror = context.Genres.First(g => g.Name == "Kinh dị");
                var romance = context.Genres.First(g => g.Name == "Tình cảm");
                var scifi = context.Genres.First(g => g.Name == "Viễn tưởng");

                // ★ picsum.photos/seed/{tên cố định}/w/h
                //   → cùng tên seed = cùng ảnh mãi mãi, mọi máy đều giống nhau
                context.Movies.AddRange(
                    new Movie
                    {
                        Title = "Inception",
                        Description = "Cobb là tên trộm chuyên đánh cắp bí mật từ trong tiềm thức của nạn nhân.",
                        ReleaseYear = 2010,
                        GenreId = action.Id,
                        ImageUrl = "https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg",
                        VideoUrl = "https://www.youtube.com/embed/YoHD9XEInc0",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    new Movie
                    {
                        Title = "Avatar 2",
                        Description = "Cuộc chiến tiếp theo tại hành tinh Pandora dưới đại dương.",
                        ReleaseYear = 2022,
                        GenreId = action.Id,
                        ImageUrl = "https://image.tmdb.org/t/p/w500/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg",
                        VideoUrl = "https://www.youtube.com/embed/d9MyW72ELq0",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    new Movie
                    {
                        Title = "The Conjuring",
                        Description = "Câu chuyện kinh dị có thật về cặp đôi điều tra ngoại cảm.",
                        ReleaseYear = 2013,
                        GenreId = horror.Id,
                        ImageUrl = "https://image.tmdb.org/t/p/w500/wVYREutTvI2tmxr6ujrHT704wGF.jpg",
                        VideoUrl = "https://www.youtube.com/embed/k10ETZ41q5I",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    new Movie
                    {
                        Title = "Titanic",
                        Description = "Chuyện tình bi thương trên con tàu huyền thoại.",
                        ReleaseYear = 1997,
                        GenreId = romance.Id,
                        ImageUrl = "https://image.tmdb.org/t/p/w500/9xjZS2rlVxm8SFx8kPC3aIGCOYQ.jpg",
                        VideoUrl = "https://www.youtube.com/embed/2e-eXJ6HgkQ",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    },
                    new Movie
                    {
                        Title = "Interstellar",
                        Description = "Hành trình xuyên không gian để cứu nhân loại.",
                        ReleaseYear = 2014,
                        GenreId = scifi.Id,
                        ImageUrl = "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
                        VideoUrl = "https://www.youtube.com/embed/zSWdZVtXT7E",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    }
                );
                context.SaveChanges();
            }

            // ─── 5. EPISODES ────────────────────────────────────────────
            if (!context.Episodes.Any())
            {
                var inception = context.Movies.First(m => m.Title == "Inception");
                var interstellar = context.Movies.First(m => m.Title == "Interstellar");

                context.Episodes.AddRange(
                    new Episode
                    {
                        MovieId = inception.Id,
                        Title = "Tập 1 - Khởi đầu",
                        EpisodeNumber = 1,
                        VideoUrl = "https://www.youtube.com/embed/YoHD9XEInc0",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Episode
                    {
                        MovieId = inception.Id,
                        Title = "Tập 2 - Giấc mơ sâu hơn",
                        EpisodeNumber = 2,
                        VideoUrl = "https://www.youtube.com/embed/YoHD9XEInc0",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Episode
                    {
                        MovieId = interstellar.Id,
                        Title = "Tập 1 - Rời Trái Đất",
                        EpisodeNumber = 1,
                        VideoUrl = "https://www.youtube.com/embed/zSWdZVtXT7E",
                        CreatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }

            // ─── 6. RATINGS ─────────────────────────────────────────────
            if (!context.Ratings.Any() && normalUser != null)
            {
                var movies = context.Movies.ToList();
                var ratings = new[] { 5, 4, 5, 3, 5 };

                for (int i = 0; i < movies.Count; i++)
                {
                    context.Ratings.Add(new Rating
                    {
                        MovieId = movies[i].Id,
                        Value = ratings[i],
                        ApplicationUserId = normalUser.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                context.SaveChanges();
            }

            // ─── 7. COMMENTS ────────────────────────────────────────────
            if (!context.Comments.Any() && normalUser != null)
            {
                var inception = context.Movies.First(m => m.Title == "Inception");
                var titanic = context.Movies.First(m => m.Title == "Titanic");

                context.Comments.AddRange(
                    new Comment
                    {
                        MovieId = inception.Id,
                        Content = "Bộ phim xuất sắc, cốt truyện rất sâu sắc!",
                        ApplicationUserId = normalUser.Id,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Comment
                    {
                        MovieId = titanic.Id,
                        Content = "Cảnh cuối phim khiến mình khóc mãi.",
                        ApplicationUserId = normalUser.Id,
                        CreatedAt = DateTime.UtcNow
                    }
                );
                context.SaveChanges();
            }

            // ─── 8. SUBTITLES ───────────────────────────────────────────
            if (!context.Subtitles.Any())
            {
                var inception = context.Movies.First(m => m.Title == "Inception");
                var interstellar = context.Movies.First(m => m.Title == "Interstellar");

                context.Subtitles.AddRange(
                    new Subtitle
                    {
                        MovieId = inception.Id,
                        Language = "Tiếng Việt",
                        FileUrl = "/subtitles/inception_vi.vtt"
                    },
                    new Subtitle
                    {
                        MovieId = inception.Id,
                        Language = "English",
                        FileUrl = "/subtitles/inception_en.vtt"
                    },
                    new Subtitle
                    {
                        MovieId = interstellar.Id,
                        Language = "Tiếng Việt",
                        FileUrl = "/subtitles/interstellar_vi.vtt"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
