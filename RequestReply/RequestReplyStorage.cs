using System.Collections.Concurrent;

namespace CloudFactory.RequestReply;

public class RequestReplyStorage<TRequest, TResponse, TKey> : IRequestReplyStorage<TRequest, TResponse, TKey> where TKey : notnull
{
    private readonly IRequestSender<TRequest> _requestSender;
    private readonly IRequestResponseKeyProvider<TRequest, TResponse, TKey> _keyProvider;
    private readonly ILogger<RequestReplyStorage<TRequest, TResponse, TKey>> _logger;
    private readonly ConcurrentDictionary<TKey, ResponseRecord> _map = new();

    public RequestReplyStorage(IRequestSender<TRequest> requestSender, IRequestResponseKeyProvider<TRequest, TResponse, TKey> keyProvider, ILogger<RequestReplyStorage<TRequest, TResponse, TKey>> logger)
    {
        _requestSender = requestSender;
        _keyProvider = keyProvider;
        _logger = logger;
    }

    public async Task<TResponse> GetResponseAsync(TRequest request, CancellationToken cancellationToken)
    {
        var key = _keyProvider.Get(request);
        var responseRecord = _map.GetOrAdd(key, _ => CreateRecord(request));
        Interlocked.Increment(ref responseRecord.AttachedCount);
        try
        {
            await responseRecord.RequestSendingLazyTask.Value.WaitAsync(cancellationToken);
            var response = await responseRecord.ResponseCompletionSource.Task.WaitAsync(cancellationToken);
            return response;
        }
        finally
        {
            var attachedCount = Interlocked.Decrement(ref responseRecord.AttachedCount);
            if (attachedCount == 0 && _map.TryRemove(new KeyValuePair<TKey, ResponseRecord>(key, responseRecord)))
            {
                responseRecord.CancellationTokenSource.Cancel();
            }
        }
    }

    public void HandleResponse(TResponse response)
    {
        var key = _keyProvider.Get(response);
        if (!_map.TryGetValue(key, out var responseRecord))
        {
            _logger.LogWarning("Response for key {key} was ignored.", key);
            return;
        }

        if (!responseRecord.ResponseCompletionSource.TrySetResult(response))
        {
            _logger.LogWarning("Response for key {key} has already been set.", key);
        }
    }

    private ResponseRecord CreateRecord(TRequest request)
    {
        var cts = new CancellationTokenSource();
        return new ResponseRecord(cts, new Lazy<Task>(() => _requestSender.SendAsync(request, cts.Token)));
    }

    private class ResponseRecord(CancellationTokenSource cancellationTokenSource, Lazy<Task> requestSendingLazyTask)
    {
        public readonly CancellationTokenSource CancellationTokenSource = cancellationTokenSource;
        public readonly TaskCompletionSource<TResponse> ResponseCompletionSource = new();
        public readonly Lazy<Task> RequestSendingLazyTask = requestSendingLazyTask;
        public int AttachedCount;
    }
}