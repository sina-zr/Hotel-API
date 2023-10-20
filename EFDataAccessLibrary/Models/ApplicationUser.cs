using EFDataAccessLibrary.Models;
using Microsoft.AspNetCore.Identity;

namespace HotelAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int guestId { get; set; }
        public Guest Guest { get; set; } // Navigation property
    }
}