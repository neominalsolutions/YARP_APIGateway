using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly;

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


    [HttpGet]
    public async Task<IActionResult> GetAggregateResponse()
    {
      List<string> responses = new();

      // Aggregates içerisinde birden fazla micro hizmetten veri toplayuacağımız için hatalı bir request durumu oluşabilir. Uygulamayı dayanıklı hale getirmek için Polly kütüphanesinden yaralanabiliriz.
      await Polly.Policy.Handle<Exception>().RetryAsync(3, (exception, retryCount) =>
      {
        Console.Out.WriteLineAsync("Hata =>" + exception.Message);
        Console.Out.WriteLineAsync("ReTryCount =>" + retryCount);

      }).ExecuteAsync(async ()=>
      {
        // endpointi değiştirelim // servisi down edip deniyelim.
        var data = await this.api1.GetStringAsync("https://localhost:7144/api3");
        var data2 = await this.api1.GetStringAsync("https://localhost:7211/api2");

       
        responses.Add(data);
        responses.Add(data2);

      });


      return Ok(responses);

    }

  }
}
