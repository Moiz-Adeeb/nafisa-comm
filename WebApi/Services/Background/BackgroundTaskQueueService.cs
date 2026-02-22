using System.Collections.Concurrent;
using Application.Interfaces;

namespace WebApi.Services.Background
{
    public class BackgroundTaskQueueService : IBackgroundTaskQueueService
    {
        private readonly ConcurrentQueue<(string, object, CancellationToken)> _workItems = new();

        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(object workItem)
        {
            _workItems.Enqueue((Guid.NewGuid().ToString(), workItem, CancellationToken.None));
            _signal.Release();
        }

        public void QueueBackgroundWorkItem(string key, object workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }
            if (_workItems.Any(p => p.Item1 == key))
            {
                return;
            }
            _workItems.Enqueue((key, workItem, CancellationToken.None));
            _signal.Release();
        }

        public async Task<object> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem.Item2;
        }
    }
}
