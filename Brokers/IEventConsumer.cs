namespace CloudFactory.Brokers;

public interface IEventConsumer<out TEvent>
{
    IAsyncEnumerable<TEvent> ConsumeAsync(CancellationToken cancellationToken);
}