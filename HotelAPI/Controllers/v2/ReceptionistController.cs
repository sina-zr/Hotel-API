using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HotelAPI.Controllers.v2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    [Authorize]
    public class ReceptionistController : ControllerBase
    {
        private readonly IHotelContext _db;

        public ReceptionistController(IHotelContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Receptionist can search bookings a user has
        /// by asking Guest's firstName and LastName.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns>Returns Bookings of user where its CheckIn date has arrived
        /// And CheckOut date hasn't arrived yet.</returns>
        [HttpGet]
        public async Task<IActionResult> SearchBookings(string firstName, string lastName)
        {
            try
            {
                var key = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var guest = await _db.Guests.Where(g => g.FirstName == firstName && g.LastName == lastName).FirstOrDefaultAsync();
                if (guest == null)
                {
                    return NotFound("Could not find user");
                }

                var bookings = await _db.Bookings
                    .Where(b => b.GuestId == guest.Id &&
                        b.StartDate < DateTime.Now &&
                        b.EndDate > DateTime.Now).ToListAsync();

                return Ok(bookings);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        /// <summary>
        /// Sets CheckedIn for A Booking as true.
        /// </summary>
        /// <param name="reservationId"></param>
        /// <returns> IActionResult </returns>
        [HttpPut]
        public async Task<IActionResult> CheckInAReservation(int reservationId)
        {
            try
            {
                var reservation = await _db.Bookings.Where(b => b.Id == reservationId).FirstOrDefaultAsync();

                if (reservation == null)
                {
                    return NotFound("Reservation was Not found!");
                }

                reservation.CheckedIn = true;
                await _db.SaveChangesAsync();

                return Ok("Successfully checkedIn Reservation");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
