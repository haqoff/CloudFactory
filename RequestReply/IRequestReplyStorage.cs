namespace CloudFactory.RequestReply;

public interface IRequestReplyStorage<in TRequest, TResponse, TKey> where TKey : notnull
{
    Task<TResponse> GetResponseAsync(TRequest request, CancellationToken cancellationToken);
    void HandleResponse(TResponse response);
}