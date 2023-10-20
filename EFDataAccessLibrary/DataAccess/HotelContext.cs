using EFDataAccessLibrary.Models;
using HotelAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFDataAccessLibrary.DataAccess;

public class HotelContext : IdentityDbContext<ApplicationUser>, IHotelContext
{
    public HotelContext(DbContextOptions<HotelContext> options) : base(options) { }

    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Guest) // ApplicationUser has one Guest
            .WithOne(g => g.User) // Guest has one ApplicationUser
            .HasForeignKey<ApplicationUser>(u => u.guestId); // Define the foreign key

    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}