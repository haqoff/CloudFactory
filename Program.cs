using CloudFactory.Brokers;
using CloudFactory.RequestReply;
using CloudFactory.TestRequests;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(typeof(IRequestReplyStorage<,,>), typeof(RequestReplyStorage<,,>));
builder.Services.AddSingleton<FileBroker>();
builder.Services.AddSingleton<IEventConsumer<TestResponse>>(p => p.GetRequiredService<FileBroker>());
builder.Services.AddSingleton<IEventProducer<TestRequestDto>>(p => p.GetRequiredService<FileBroker>());
builder.Services.AddSingleton<IRequestResponseKeyProvider<TestRequestDto, TestResponse, string>, TestKeyProvider>();
builder.Services.AddSingleton<IRequestSender<TestRequestDto>, EventRequestSender<TestRequestDto>>();
builder.Services.AddHostedService<TestResponseEventHandlerBackgroundService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("requests", async ([FromBody] TestRequestDto requestDto, [FromServices] IRequestReplyStorage<TestRequestDto, TestResponse, string> requestReplyStorage, CancellationToken cancellationToken) =>
    {
        var response = await requestReplyStorage.GetResponseAsync(requestDto, cancellationToken);
        return TypedResults.Text(response.ResponseBody, statusCode: response.StatusCode);
    })
    .WithName("GetResponse")
    .WithOpenApi();


app.Run();