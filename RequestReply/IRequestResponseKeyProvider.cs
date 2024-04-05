namespace CloudFactory.RequestReply;

public interface IRequestResponseKeyProvider<in TRequest, in TResponse, out TKey>
{
    TKey Get(TRequest request);
    TKey Get(TResponse response);
}