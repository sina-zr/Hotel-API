﻿using EFDataAccessLibrary.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers.v2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    [AllowAnonymous]
    public class SearchRoomsController : ControllerBase
    {
        private readonly IHotelContext _db;

        public SearchRoomsController(IHotelContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get Available Room Types (not Rooms) and their info
        /// like Title, Description and Price
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns> List of availableRoomTypes wrraped in IACtionResult </returns>
        [HttpGet]
        public async Task<IActionResult> GetAvailableRoomTypes(DateTime startDate, DateTime endDate)
        {
            try
            {
                var availableRoomTypes = await _db.RoomTypes
                    .Where(room =>
                        !_db.Bookings.Any(booking =>
                            booking.RoomId == room.Id &&
                            startDate < booking.EndDate &&
                            endDate > booking.StartDate))
                    .ToListAsync();

                return Ok(availableRoomTypes);
            }
            catch (Exception)
            {
                // Handle any exceptions
                return StatusCode(500, $"An error occurred while processing your request.");
            }
        }
    }
}
