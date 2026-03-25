using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsitePhim.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public DateTime CreatedAt { get; set; }

        public int MovieId { get; set; }
        [ForeignKey("MovieId")]
        public Movie Movie { get; set; }
        public string? ApplicationUserId { get; set; } 
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}