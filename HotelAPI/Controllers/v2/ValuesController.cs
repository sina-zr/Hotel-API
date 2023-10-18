using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
public class ValuesController : ControllerBase
{
    // GET: api/v2/<UsersController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "v2 value1", "v2 value2" };
    }
}
