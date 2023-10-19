using Microsoft.AspNetCore.Mvc;

namespace HotelAPI.Controllers.v1.BookingServices
{
    public interface IBookingService
    {
        Task<IActionResult> BookARoom(string firstName, string lastName, string email, DateTime startDate, DateTime endDate, int roomTypeId);
    }
}
