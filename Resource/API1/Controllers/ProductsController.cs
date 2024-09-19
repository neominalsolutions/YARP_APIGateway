using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers
{
  [Route("api1/[controller]")]
  [ApiController]
  public class ProductsController : ControllerBase
  {
    [HttpGet]
    public IActionResult GetProducts()
    {
      // Thread.Sleep(6000);

      return Ok("Products");
    }

  }
}
