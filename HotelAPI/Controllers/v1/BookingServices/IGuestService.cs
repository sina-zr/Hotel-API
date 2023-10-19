using EFDataAccessLibrary.Models;

namespace HotelAPI.Controllers.v1.BookingServices
{
    public interface IGuestService
    {
        Task<Guest> GetOrCreateGuestAsync(string firstName, string lastName, string email);
    }
}
