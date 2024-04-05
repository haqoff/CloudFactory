namespace CloudFactory.Brokers;

public interface IEventProducer<in TEvent>
{
    Task ProduceAsync(TEvent e, CancellationToken cancellationToken);
}