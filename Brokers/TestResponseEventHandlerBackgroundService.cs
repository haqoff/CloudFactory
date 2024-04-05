using CloudFactory.RequestReply;
using CloudFactory.TestRequests;

namespace CloudFactory.Brokers;

public class TestResponseEventHandlerBackgroundService : BackgroundService
{
    private readonly IEventConsumer<TestResponse> _consumer;
    private readonly IRequestReplyStorage<TestRequestDto, TestResponse, string> _storage;

    public TestResponseEventHandlerBackgroundService(IEventConsumer<TestResponse> consumer, IRequestReplyStorage<TestRequestDto, TestResponse, string> storage)
    {
        _consumer = consumer;
        _storage = storage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var response in _consumer.ConsumeAsync(stoppingToken))
        {
            _storage.HandleResponse(response);
        }
    }
}