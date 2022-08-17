/*using Data.EF;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Task2ConvertToApi.BackendApi.BackGround
{
    public class Worker : IWorker
    {
        private readonly ILogger<Worker> logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Task2DbContext _context;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, Task2DbContext context)
        {
            _httpClientFactory = httpClientFactory;
            this.logger = logger;
            _context = context;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            await Task.Delay(1000 * 10);
            while (!cancellationToken.IsCancellationRequested)
            {
                var everythingInTheDatabase = _context.UserDailyTimesheetModels.ToList();

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://localhost:7244/");
                foreach (var item in everythingInTheDatabase)
                {
                    if (item.TotalActualWorkingTimeInSeconds != null)
                        continue;
                    else
                    {
                        var response = await client.GetAsync($"/api/Users/CalculateWorkTimePerDay/{item.UserId}/{item.Day}/{item.Month}/{item.Year}");
                        var body = await response.Content.ReadAsStringAsync();
                        logger.LogInformation("successfully");
                    }
                }

                await Task.Delay(1000 * 5);
            }
        }
    }
}*/