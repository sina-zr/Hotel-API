using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using SharedModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ValidationLibrary;

namespace HotelAPI.Controllers.v2.BookingServices;

public class BookingService : IBookingService
{
    private readonly IRoomService _roomService;
    private readonly IHotelContext _db;

    public BookingService(IRoomService roomService, IHotelContext db)
    {
        _roomService = roomService;
        _db = db;
    }

    public async Task<IActionResult> BookARoom(int guestId, BookingBodyModel bookingBody)
    {
        // Validating our parameters
        BookARoomBodyValidator validator = new BookARoomBodyValidator();
        var validationResult = validator.Validate(bookingBody);

        if (validationResult.IsValid)
        {
            try
            {
                var availableRooms = await _roomService.GetAvailableRoomsAsync(bookingBody.RoomTypeId,
                                                                               bookingBody.StartDate,
                                                                               bookingBody.EndDate);

                if (availableRooms.Any())
                {
                    var roomId = availableRooms.First().Id;
                    var totalCost = _roomService.CalculateTotalCost(bookingBody.RoomTypeId,
                                                                    bookingBody.StartDate,
                                                                    bookingBody.EndDate);

                    var booking = CreateBooking(roomId,
                                                guestId,
                                                bookingBody.StartDate,
                                                bookingBody.EndDate,
                                                totalCost);

                    await _db.Bookings.AddAsync(booking);
                    await _db.SaveChangesAsync();

                    return new OkResult();
                }
                else
                {
                    return new ObjectResult("Bad Request or No selected RoomTypes available for the specified dates.")
                    {
                        StatusCode = 400
                    };
                }
            }
            catch (Exception)
            {
                return new ObjectResult("An error occurred while processing your request.")
                {
                    StatusCode = 500
                };
            }
        }
        else
        {
            var errors = validationResult.Errors.Select(error => error.ErrorMessage);
            var response = new
            {
                Message = "Validation failed",
                Errors = errors
            };

            return new ObjectResult(response)
            {
                StatusCode = 400
            };
        }

    }

    private Booking CreateBooking(int roomId, int guestId, DateTime startDate, DateTime endDate, decimal totalCost)
    {
        return new Booking { RoomId = roomId, GuestId = guestId, StartDate = startDate, EndDate = endDate, CheckedIn = false, TotalCost = totalCost };
    }
}
