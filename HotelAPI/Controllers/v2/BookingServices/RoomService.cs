using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers.v2.BookingServices
{
    public class RoomService : IRoomService
    {
        private readonly IHotelContext _db;

        public RoomService(IHotelContext db)
        {
            _db = db;
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            var rooms = await _db.Rooms
                    .Where(room => room.RoomTypeId == roomTypeId)
                    .ToListAsync(); // Assuming ToListAsync is available for async retrieval

            var existingBookings = await _db.Bookings
                .Where(booking => startDate < booking.EndDate && endDate > booking.StartDate)
            .ToListAsync(); // Assuming ToListAsync is available for async retrieval

            return rooms.Where(room => !existingBookings.Any(booking => booking.RoomId == room.Id)).ToList();
        }

        public decimal CalculateTotalCost(int roomTypeId, DateTime startDate, DateTime endDate)
        {
            var roomType = _db.RoomTypes.FirstOrDefault(rt => rt.Id == roomTypeId);
            if (roomType != null)
            {
                return roomType.Price * endDate.Subtract(startDate).Days;
            }
            return 0; // Handle the case when room type is not found.
        }
    }
}
