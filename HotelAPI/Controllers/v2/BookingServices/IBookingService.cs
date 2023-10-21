using Microsoft.AspNetCore.Mvc;

namespace HotelAPI.Controllers.v2.BookingServices
{
    public interface IBookingService
    {
        Task<IActionResult> BookARoom(int guestId, string email, DateTime startDate, DateTime endDate, int roomTypeId);
    }
}
