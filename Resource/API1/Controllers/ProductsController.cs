using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API1.Controllers
{
  [Route("api1/[controller]")]
  [ApiController]
  public class ProductsController : ControllerBase
  {
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetProducts()
    {

      return Ok("Products");
    }


    // api1/onlyManager
    [HttpGet("onlyManager")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Manager")]
    public IActionResult GetProductOnlyManager()
    {

      return Ok("Only Manager");
    }

  }
}
