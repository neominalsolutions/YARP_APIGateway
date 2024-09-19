using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_Gateway.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AggregatesController : ControllerBase
  {
    private readonly HttpClient api1;

    public AggregatesController(IHttpClientFactory clientFactory)
    {
      this.api1 = clientFactory.CreateClient("api1");
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
      // endpointi değiştirelim // servisi down edip deniyelim.
      var data =  await  this.api1.GetStringAsync("/api1/products");


      return Ok(data);
    }

  }
}
