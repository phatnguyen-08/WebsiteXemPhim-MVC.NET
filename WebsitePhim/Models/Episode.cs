using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsitePhim.Models
{
    public class Episode
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [ForeignKey("MovieId")]
        public Movie Movie { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string VideoUrl { get; set; } 

        public int EpisodeNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
