namespace CloudFactory.TestRequests;

public class TestResponse
{
    public required string Key { get; init; }
    public required int StatusCode { get; init; }
    public required string ResponseBody { get; init; }
}