using System; // Make sure this is present for DateTime

namespace WebsitePhim.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ReleaseYear { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; } 

     
        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; } = false; 


        public int GenreId { get; set; }
        public Genre Genre { get; set; }

     
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Subtitle> Subtitles { get; set; } 
        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();

    }
}