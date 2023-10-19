using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using HotelAPI.Controllers.v1.BookingServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class BookRoomController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookRoomController(IBookingService bookingService)
    {
        _bookingService = bookingService;
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
        var result = await _bookingService.BookARoom(firstName, lastName, email, startDate, endDate, roomTypeId);

        string message = result switch
        {
            OkResult => "Room booked successfully.",
            StatusCodeResult statusCodeResult when statusCodeResult.StatusCode == 400 => "No available rooms for the specified dates.",
            StatusCodeResult statusCodeResult when statusCodeResult.StatusCode == 500 => "An error occurred while processing your request.",
            _ => "An unknown error occurred."
        };

        return result is ObjectResult objectResult
            ? new ObjectResult(new { Message = message }) { StatusCode = objectResult.StatusCode }
            : new ObjectResult(new { Message = message });
    }
}