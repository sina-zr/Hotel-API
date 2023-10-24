using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using FluentValidation.Results;
using HotelAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ValidationLibrary;

namespace HotelAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
[AllowAnonymous]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;
    private readonly IHotelContext _db;

    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        IHotelContext db)
    {
        _userManager = userManager;
        _config = config;
        _db = db;
    }

    /// <summary>
    /// Register Guest with this Endpoint
    /// </summary>
    /// <param name="model">Pass username, password, firstname, lastname, email inside Body </param>
    /// <returns>Token with Guest's Id, first and lastname encoded. </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationViewModel model)
    {
        model.FirstName = model.FirstName.ToLower();
        model.LastName = model.LastName.ToLower();

        // Validate model
        RegisterValidation validator = new RegisterValidation();
        ValidationResult validationResult = validator.Validate(model);

        if (validationResult.IsValid)
        {
            try
            {
                // Check if the username is already used.
                var existingUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUser != null)
                {
                    return BadRequest("Username is already in use.");
                }

                // Create a new Guest record with Firstname, LastName, and Email.
                var newGuest = new Guest
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress
                };

                // Add the Guest record to your database context and save changes.
                _db.Guests.Add(newGuest);
                await _db.SaveChangesAsync();

                // Fetch the auto-generated GuestId for the newly created Guest record.
                int guestId = newGuest.Id;

                // Create a new ApplicationUser using the provided model.
                var newUser = new ApplicationUser
                {
                    UserName = model.Username,
                    guestId = guestId
                };

                // Use UserManager to create the new user.
                var result = await _userManager.CreateAsync(newUser, model.PasswordHash);

                if (result.Succeeded)
                {
                    // User registration is successful, generate a JWT token.
                    var userData = new UserDataModel { firstName = model.FirstName, lastName = model.LastName, guestId = guestId };
                    var token = GenerateJwtToken(userData);
                    return Ok(token);
                }
                else
                {
                    // If user creation fails, handle the errors, and potentially delete the Guest record created earlier.
                    _db.Guests.Remove(newGuest);
                    await _db.SaveChangesAsync();

                    // You can check result.Errors for more details on the errors.
                    return BadRequest("User registration failed.");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong!");
            }
        }
        else
        {
            // Validation failed. Build an error response.
            var errorResponse = new
            {
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(error => error.ErrorMessage)
            };

            return BadRequest(errorResponse);
        }

    }

    /// <summary>
    /// Login Guest with this Endpoint
    /// </summary>
    /// <param name="model">Pass username and password inside Body.</param>
    /// <returns>Token with Guest's Id, first and lastname encoded.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        // Find the user by username in the Auth table.
        var user = await _userManager.FindByNameAsync(model.Username);

        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            // User is authenticated. Now, fetch additional user data from the Guests table.
            var guestId = user.guestId; // Assuming you store GuestId in your ApplicationUser class.
            var guest = await _db.Guests.FindAsync(guestId);

            if (guest != null)
            {
                // User data is retrieved. Create a UserDataModel.
                var userData = new UserDataModel
                {
                    firstName = guest.FirstName,
                    lastName = guest.LastName,
                    guestId = guest.Id
                };

                // Generate a JWT token with the user data.
                var token = GenerateJwtToken(userData);
                return Ok(token);
            }
        }

        return Unauthorized("Invalid credentials");
    }

    private string GenerateJwtToken(UserDataModel data)
    {
        // Creating our key
        var secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(
                _config.GetValue<string>("Authentication:SecretKey")));

        var signingCredentials = new SigningCredentials(
            secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new List<Claim>();
        claims.Add(new(JwtRegisteredClaimNames.Sub, data.guestId.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.GivenName, data.firstName.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.FamilyName, data.lastName.ToString()));

        var token = new JwtSecurityToken(
            _config.GetValue<string>("Authentication:Issuer"),
            _config.GetValue<string>("Authentication:Audience"),
            claims,
            DateTime.Now,
            DateTime.Now.AddMinutes(5),
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
