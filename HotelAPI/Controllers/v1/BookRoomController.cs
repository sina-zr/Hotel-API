using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class BookRoomController : ControllerBase
{
    private readonly IHotelContext _db;

    public BookRoomController(IHotelContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Book an Available Room by giving User info
    /// if user doesn't exist in database, Adds it.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="email"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="roomTypeId"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> BookARoom(string firstName, string lastName, string email, DateTime startDate, DateTime endDate, int roomTypeId)
    {
        try
        {
            firstName = firstName.ToLower();
            lastName = lastName.ToLower();

            var availableRooms = await GetAvailableRoomsAsync(roomTypeId, startDate, endDate);

            var guest = await GetOrCreateGuestAsync(firstName, lastName, email);

            if (availableRooms.Any())
            {
                var roomId = availableRooms.First().Id;
                var totalCost = CalculateTotalCost(roomTypeId, startDate, endDate);

                var booking = CreateBooking(roomId, guest.Id, startDate, endDate, totalCost);

                await _db.Bookings.AddAsync(booking);
                await _db.SaveChangesAsync();

                return Ok();
            }
            else
            {
                return StatusCode(400, "No available rooms for the specified dates.");
            }
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    private async Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime startDate, DateTime endDate)
    {
        var rooms = await _db.Rooms
                .Where(room => room.RoomTypeId == roomTypeId)
                .ToListAsync(); // Assuming ToListAsync is available for async retrieval

        var existingBookings = await _db.Bookings
            .Where(booking => startDate < booking.EndDate && endDate > booking.StartDate)
        .ToListAsync(); // Assuming ToListAsync is available for async retrieval

        return rooms.Where(room => !existingBookings.Any(booking => booking.RoomId == room.Id)).ToList();
    }

    private async Task<Guest> GetOrCreateGuestAsync(string firstName, string lastName, string email)
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

    private decimal CalculateTotalCost(int roomTypeId, DateTime startDate, DateTime endDate)
    {
        var roomType = _db.RoomTypes.FirstOrDefault(rt => rt.Id == roomTypeId);
        if (roomType != null)
        {
            return roomType.Price * endDate.Subtract(startDate).Days;
        }
        return 0; // Handle the case when room type is not found.
    }

    private Booking CreateBooking(int roomId, int guestId, DateTime startDate, DateTime endDate, decimal totalCost)
    {
        return new Booking { RoomId = roomId, GuestId = guestId, StartDate = startDate, EndDate = endDate, CheckedIn = false, TotalCost = totalCost };
    }
}