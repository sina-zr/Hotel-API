using EFDataAccessLibrary.Models;

namespace HotelAPI.Controllers.v1.BookingServices
{
    public interface IRoomService
    {
        Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime startDate, DateTime endDate);
        decimal CalculateTotalCost(int roomTypeId, DateTime startDate, DateTime endDate);
    }
}
