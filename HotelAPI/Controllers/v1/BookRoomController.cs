using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

            var availableRooms = _db.Rooms
                .Where(room => room.RoomTypeId == roomTypeId &&
                    !_db.Bookings.Any(booking =>
                        booking.RoomId == room.Id &&
                        startDate < booking.EndDate &&
                        endDate > booking.StartDate))
                .ToList();

            // Checking if user exists
            var guest = _db.Guests.FirstOrDefault(g => g.FirstName == firstName && g.LastName == lastName);

            if (guest == null)
            {
                guest = new Guest { FirstName = firstName, LastName = lastName, EmailAddress = email };
                await _db.Guests.AddAsync(guest);
                await _db.SaveChangesAsync();

                // Get User Id
                guest = _db.Guests.Where(g => g.FirstName == firstName && g.LastName == lastName).FirstOrDefault();
            }

            var roomId = availableRooms.First().Id;
            var roomType = _db.RoomTypes.Where(roomType => roomType.Id == roomTypeId).First();
            var totalCost = roomType.Price * endDate.Subtract(startDate).Days;

            var booking = new Booking { RoomId = roomId, GuestId = guest!.Id, StartDate = startDate, EndDate = endDate, CheckedIn = false, TotalCost = totalCost };

            await _db.Bookings.AddAsync(booking);
            await _db.SaveChangesAsync();

            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(500, $"An error occurred while processing your request.");
        }
    }
}