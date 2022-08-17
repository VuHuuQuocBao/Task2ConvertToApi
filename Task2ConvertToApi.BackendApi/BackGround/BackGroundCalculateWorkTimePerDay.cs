/*using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Task2ConvertToApi.BackendApi.BackGround
{
    public class BackGroundCalculateWorkTimePerDay : IHostedService
    {
        private readonly ILogger<BackGroundCalculateWorkTimePerDay> logger;
        private readonly IWorker worker;

        public BackGroundCalculateWorkTimePerDay(ILogger<BackGroundCalculateWorkTimePerDay> logger,
            IWorker worker)
        {
            this.logger = logger;
            this.worker = worker;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await worker.DoWork(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}*/