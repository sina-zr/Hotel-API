using EFDataAccessLibrary.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HotelAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
[AllowAnonymous]
public class SearchRoomsController : ControllerBase
{
    private readonly IHotelContext _db;
    private readonly ILogger<SearchRoomsController> _logger;

    public SearchRoomsController(IHotelContext db, ILogger<SearchRoomsController> logger)
    {
        _db = db;
        _logger = logger;
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
            var parameters = new[]
            {
                new SqlParameter("@startDate", SqlDbType.Date) { Value = startDate },
                new SqlParameter("@endDate", SqlDbType.Date) { Value = endDate }
            };

            //var availableRoomTypes = _db.RoomTypes
            //    .FromSqlRaw("dbo.spRoomTypes_GetAvailableRoomTypes @startDate, @endDate",
            //                                                  parameters).ToList();


            var bookedRoomIds = _db.Bookings
                .Where(b =>
                    (startDate < b.StartDate && endDate >= b.EndDate) ||
                    (b.StartDate < endDate && endDate < b.EndDate) ||
                    (b.StartDate <= startDate && startDate < b.EndDate))
                .Select(b => b.RoomId)
                .Distinct();

            var availableRooms = _db.Rooms
                .Where(r => !bookedRoomIds.Contains(r.Id))
                .Select(r => r.RoomTypeId)
                .ToList();

            var availableRoomTypes = _db.RoomTypes
                .Where(t => availableRooms.Contains(t.Id))
                .ToList();

            return Ok(availableRoomTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occuerred while processing a request: {ErrorMessage}", ex.Message);

            // Handle any exceptions
            return StatusCode(500, $"An error occurred while processing your request.");
        }
    }
}
