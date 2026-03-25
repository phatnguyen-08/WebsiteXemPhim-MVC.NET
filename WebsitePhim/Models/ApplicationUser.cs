using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System; 

namespace WebsitePhim.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}