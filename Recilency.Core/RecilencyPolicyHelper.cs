using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;


namespace API_Gateway.Recilency
{
  public static class RecilencyPolicyHelper
  {
    public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(int retryCount, TimeSpan sleepDuration)
    {
      return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, retryAttempt =>
            {
              Console.WriteLine("Before Retry");
              return sleepDuration;
            }, onRetry: (outcome, timespan, retryCount, context) =>
            {
              Console.WriteLine("Yeniden Deneniyor...");
            });
    }

    public static IAsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy(TimeSpan timeout)
    {
      return Policy.TimeoutAsync<HttpResponseMessage>(timeout, (context, timespan, task) =>
      {
        Console.WriteLine("Zaman aşımına uğradı");

        return Task.CompletedTask;
      }); // 5 saniye sonra zaman aşımına düş.
    }

    public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBrakerPolicy(int exceptionCount, TimeSpan durationOfBreak)
    {
      return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30), 
         onBreak: (exception, duration) =>
        {
          // Circuit kırıldığında loglama
          Console.WriteLine($"Circuit broken! Next call will fail for {duration.TotalSeconds} seconds due to: {exception.Exception.Message}");
        },
        onReset: () =>
        {
            // Circuit tekrar açıldığında loglama
           Console.WriteLine("Circuit reset!");
        });

    }

  }
}
