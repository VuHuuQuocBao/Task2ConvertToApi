using System.Threading;
using System.Threading.Tasks;

namespace Task2ConvertToApi.BackendApi.BackGroundTask
{
    public interface IWorker
    {
        Task DoWork(CancellationToken cancellationToken);
    }
}