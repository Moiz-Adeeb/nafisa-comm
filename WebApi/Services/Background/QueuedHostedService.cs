using Application.Interfaces;
using MediatR;
using Newtonsoft.Json;

namespace WebApi.Services.Background
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public QueuedHostedService(
            IBackgroundTaskQueueService taskQueue,
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            TaskQueue = taskQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = loggerFactory.CreateLogger<QueuedHostedService>();
        }

        public IBackgroundTaskQueueService TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            while (false == stoppingToken.IsCancellationRequested)
            {
                object workItemDto = null;
                try
                {
                    var workItem = await TaskQueue.DequeueAsync(stoppingToken);
                    workItemDto = workItem;
                    await mediator.Send(workItem, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Ignored
                }
                catch (Exception ex)
                {
                    var json = "";
                    if (workItemDto != null)
                    {
                        json = JsonConvert.SerializeObject(workItemDto);
                    }
                    _logger.LogError(
                        ex,
                        $"Error occurred executing Work item. -- {workItemDto?.GetType().Name}"
                            + json
                    );
                }
            }
        }
    }
}
