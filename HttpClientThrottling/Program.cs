using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpClientThrottling
{
    class Program
    {
        private static HttpClient httpClient = new HttpClient();
        
        static async Task Main(string[] args)
        {
            var tasks = new List<Task>();
            
            // here we set the max allowed concurrent request
            var throttler = new SemaphoreSlim(3);

            for (var i = 0; i < 12; i++)
            {
                // let's wait here until we can pass from the Semaphore
                await throttler.WaitAsync();

                // add our task logig here
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await ExecuteRequest();
                        
                        // let's wait here for 1 second to honor the API's rate limit
                        await Task.Delay(1000);
                    }
                    finally
                    {
                        // here we release the throttler immediately
                        throttler.Release();
                    }
                }));
            }

            // await for all the tasks to complete
            await Task.WhenAll(tasks.ToArray());
        }

        private static async Task<HttpResponseMessage> ExecuteRequest()
        {
            var result = await httpClient.GetAsync("https://www.bing.com");
            return result;
        }
    }
}
