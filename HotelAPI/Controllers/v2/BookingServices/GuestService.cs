using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers.v2.BookingServices
{
    public class GuestService : IGuestService
    {
        private readonly IHotelContext _db;

        public GuestService(IHotelContext db)
        {
            _db = db;
        }

        public async Task<Guest> GetOrCreateGuestAsync(string firstName, string lastName, string email)
        {
            var guest = await _db.Guests.FirstOrDefaultAsync(g => g.FirstName == firstName && g.LastName == lastName);

            if (guest == null)
            {
                guest = new Guest { FirstName = firstName, LastName = lastName, EmailAddress = email };
                _db.Guests.Add(guest);
                await _db.SaveChangesAsync();
            }

            return guest;
        }
    }
}
