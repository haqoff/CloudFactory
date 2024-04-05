namespace CloudFactory.RequestReply;

public interface IRequestSender<in TRequest>
{
    Task SendAsync(TRequest request, CancellationToken cancellationToken);
}