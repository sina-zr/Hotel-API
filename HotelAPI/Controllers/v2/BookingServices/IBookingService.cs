using Microsoft.AspNetCore.Mvc;
using SharedModels;

namespace HotelAPI.Controllers.v2.BookingServices
{
    public interface IBookingService
    {
        Task<IActionResult> BookARoom(int guestId, BookingBodyModel bookingBody);
    }
}
