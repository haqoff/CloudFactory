using CloudFactory.Brokers;
using CloudFactory.RequestReply;

namespace CloudFactory.TestRequests;

public class EventRequestSender<TRequest> : IRequestSender<TRequest>
{
    private readonly IEventProducer<TRequest> _producer;

    public EventRequestSender(IEventProducer<TRequest> producer)
    {
        _producer = producer;
    }

    public Task SendAsync(TRequest request, CancellationToken cancellationToken)
    {
        return _producer.ProduceAsync(request, cancellationToken);
    }
}