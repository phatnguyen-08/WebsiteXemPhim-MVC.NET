namespace WebsitePhim.Models
{
    public class Subtitle
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public string Language { get; set; }   
        public string FileUrl { get; set; }
    }
}
