using CloudFactory.RequestReply;

namespace CloudFactory.TestRequests;

public class TestKeyProvider : IRequestResponseKeyProvider<TestRequestDto, TestResponse, string>
{
    public string Get(TestRequestDto request)
    {
        return request.Key;
    }

    public string Get(TestResponse response)
    {
        return response.Key;
    }
}