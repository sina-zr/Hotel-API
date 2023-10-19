using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.EntityFrameworkCore;
using EFDataAccessLibrary.DataAccess;
using HotelAPI.Controllers.v1;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.v1;

public class SearchRoomsControllerTests
{
    [Fact]
    public async Task GetAvailableRoomTypes_Returns_OkResult()
    {
        // Arrange

        // We set up an in-memory database for testing using Entity Framework Core
        var options = new DbContextOptionsBuilder<HotelContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
            .Options;

        using var context = new HotelContext(options);

        // We create an instance of the controller, passing the in-memory database context.
        var controller  = new SearchRoomsController(context);


        // Act
        var result = await controller.GetAvailableRoomTypes(DateTime.Now,
                                                            DateTime.Now.AddDays(1));

        // Assert

        // We call the action method and assert that it returns an OkObjectResult.
        Assert.IsType<OkObjectResult>(result);
    }
}
