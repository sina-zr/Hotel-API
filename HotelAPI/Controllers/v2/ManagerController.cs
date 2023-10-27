using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using HotelAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedModels;
using ValidationLibrary;

namespace HotelAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
[Authorize(Roles = "Manager")]
public class ManagerController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ManagerController> _logger;
    private readonly IHotelContext _db;

    public ManagerController(UserManager<ApplicationUser> userManager,
                             ILogger<ManagerController> logger,
                             IHotelContext db)
    {
        _userManager = userManager;
        _logger = logger;
        _db = db;
    }

    /// <summary>
    /// Manager can Promote a Guest into a Receptionist
    /// </summary>
    /// <param name="username">username of the Guest</param>
    /// <returns></returns>
    [HttpPost("promote")]
    public async Task<IActionResult> PromoteGuestToRecep([FromBody] string username)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                // Check if the user is in the Guest role
                if (await _userManager.IsInRoleAsync(user, "Guest"))
                {
                    // Promote the user to a Receptionist by adding the Receptionist role
                    await _userManager.AddToRoleAsync(user, "Receptionist");
                    _logger.LogInformation("{UserName} is now a Receptionist.", user.UserName);
                    return Ok($"{user.UserName} is now a Receptionist.");
                }
                else
                {
                    _logger.LogWarning("{UserName} is not in the Guest role and cannot be promoted.", user.UserName);
                    return BadRequest($"{user.UserName} is not in the Guest role and cannot be promoted.");
                }
            }
            else
            {
                _logger.LogWarning("User {UserName} not found.", username);
                return BadRequest($"User {username} not found.");
            }

        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Something went wrong: {ErrorMessage}", ex.Message);
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Manager can add a room to Rooms table
    /// knowing the RoomTypeId they wanna add
    /// </summary>
    /// <param name="roomModel">Include the RoomNumber and
    /// RoomTypeId of the room you wanna add inside Body</param>
    /// <returns></returns>
    [HttpPost("addroom")]
    public async Task<IActionResult> AddARoom([FromBody] AddRoomModel roomModel)
    {
        try
        {
            // validate room
            AddRoomValidation validator = new AddRoomValidation(_db);
            var validationResult = validator.Validate(roomModel);

            if (validationResult.IsValid)
            {
                var room = new Room
                {
                    RoomTypeId = roomModel.RoomTypeId,
                    RoomNumber = roomModel.RoomNumber
                };

                await _db.Rooms.AddAsync(room);
                await _db.SaveChangesAsync();

                return Ok($"RoomNumber {room.RoomNumber} with Type {room.RoomTypeId} was successfully added.");
            }
            else
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage);
                var response = new
                {
                    Message = "Validation failed",
                    Errors = errors
                };

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong {ErrorMessage}", ex.Message);

            return StatusCode(500, "Something went wrong.");
        }
    }
}