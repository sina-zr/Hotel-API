using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelAPI.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class ValuesController : ControllerBase
{
    // GET: api/v1/<UsersController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "v1 value1", "v1 value2" };
    }
}
