using System.Text.Json.Serialization;

namespace CloudFactory.TestRequests;

public class TestRequestDto
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("body")]
    public required string Body { get; init; }
}