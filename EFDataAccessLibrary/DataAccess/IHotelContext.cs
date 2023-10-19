using EFDataAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFDataAccessLibrary.DataAccess;

public interface IHotelContext
{
    DbSet<Booking> Bookings { get; set; }
    DbSet<Guest> Guests { get; set; }
    DbSet<Room> Rooms { get; set; }
    DbSet<RoomType> RoomTypes { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}