using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.Tasks;

public class ServerTaskWorker
{
    private readonly ILogger<ServerTaskWorker> _logger;
    private readonly List<IServerTask> _tasks;

    public ServerTaskWorker(ILogger<ServerTaskWorker> logger, List<IServerTask> tasks)
    {
        _logger = logger;
        _tasks = tasks;
    }

    public void Start()
    {
        Task.Run(WorkAsync);
    }

    private bool _cancelled;
    
    public void Stop()
    {
        _cancelled = true;
    }

    private async Task WorkAsync()
    {
        while (!_cancelled)
        {
            foreach (var task in _tasks.Where(task => task.WaitingToExecute()))
            {
                task.LastExecuted = DateTime.Now;
                ProcessTaskAsync(task);
            }

            Thread.Sleep(200);
        }
    }

    private async Task ProcessTaskAsync(IServerTask task)
    {
        var stopwatch = Stopwatch.StartNew();
        await task.ExecuteAsync();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds >= task.PeriodicInterval.TotalMilliseconds / 2)
        {
            _logger.LogWarning($"Task '{task.Name}' took {stopwatch.ElapsedMilliseconds}ms to run.");
        }
    }
}