using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelAPI.Controllers.v2.BookingServices;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SharedModels;

namespace HotelAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
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
    /// <param name="bookingBody"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> BookARoom([FromBody] BookingBodyModel bookingBody)
    {
        int guestId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        var result = await _bookingService.BookARoom(guestId, bookingBody);

        string message = result switch
        {
            OkResult => "Room booked successfully.",
            StatusCodeResult statusCodeResult when statusCodeResult.StatusCode == 400 => "Bad Request or No available rooms for the specified dates.",
            StatusCodeResult statusCodeResult when statusCodeResult.StatusCode == 500 => "An error occurred while processing your request.",
            _ => "An unknown error occurred."
        };

        return result is ObjectResult objectResult
            ? new ObjectResult(new { Message = message }) { StatusCode = objectResult.StatusCode }
            : new ObjectResult(new { Message = message });
    }
}