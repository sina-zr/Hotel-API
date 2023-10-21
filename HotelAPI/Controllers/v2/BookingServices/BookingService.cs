using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelAPI.Controllers.v2.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly IRoomService _roomService;
        private readonly IGuestService _guestService;
        private readonly IHotelContext _db;

        public BookingService(IRoomService roomService, IGuestService guestService, IHotelContext db)
        {
            _roomService = roomService;
            _guestService = guestService;
            _db = db;
        }

        public async Task<IActionResult> BookARoom(int guestId,string email, DateTime startDate, DateTime endDate, int roomTypeId)
        {
            try
            {
                var availableRooms = await _roomService.GetAvailableRoomsAsync(roomTypeId, startDate, endDate);

                if (availableRooms.Any())
                {
                    var roomId = availableRooms.First().Id;
                    var totalCost = _roomService.CalculateTotalCost(roomTypeId, startDate, endDate);

                    var booking = CreateBooking(roomId, guestId, startDate, endDate, totalCost);

                    await _db.Bookings.AddAsync(booking);
                    await _db.SaveChangesAsync();

                    return new OkResult();
                }
                else
                {
                    return new StatusCodeResult(400);
                }
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }

        private Booking CreateBooking(int roomId, int guestId, DateTime startDate, DateTime endDate, decimal totalCost)
        {
            return new Booking { RoomId = roomId, GuestId = guestId, StartDate = startDate, EndDate = endDate, CheckedIn = false, TotalCost = totalCost };
        }
    }
}
