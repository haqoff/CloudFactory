using System.Runtime.CompilerServices;
using System.Threading.Channels;
using CloudFactory.TestRequests;

namespace CloudFactory.Brokers;

public class FileBroker : IEventConsumer<TestResponse>, IEventProducer<TestRequestDto>, IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly string _dirPath;

    public FileBroker()
    {
        _dirPath = Path.Combine(AppContext.BaseDirectory, "BrokerTest");
        Directory.CreateDirectory(_dirPath);
        _watcher = new FileSystemWatcher(_dirPath);
        _watcher.EnableRaisingEvents = true;
    }

    public async IAsyncEnumerable<TestResponse> ConsumeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<FileSystemEventArgs>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false
        });

        void HandleEvent(object sender, FileSystemEventArgs e)
        {
            channel.Writer.TryWrite(e);
        }

        _watcher.Created += HandleEvent;
        await foreach (var change in channel.Reader.ReadAllAsync(cancellationToken))
        {
            var response = await CreateResponseAsync(change);
            if (response is not null)
            {
                yield return response;
            }
        }

        channel.Writer.TryComplete();
        _watcher.Created -= HandleEvent;
    }

    public Task ProduceAsync(TestRequestDto e, CancellationToken cancellationToken)
    {
        var name = Path.Combine(_dirPath, e.Key + ".req");
        return File.WriteAllTextAsync(name, e.Body, cancellationToken);
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }

    private async Task<TestResponse?> CreateResponseAsync(FileSystemEventArgs args)
    {
        var ext = Path.GetExtension(args.FullPath);
        if (ext != ".resp")
        {
            return null;
        }

        var key = Path.GetFileNameWithoutExtension(args.FullPath);
        var content = await File.ReadAllTextAsync(args.FullPath);
        var newLineIndex = content.IndexOf('\n');
        var statusCode = int.Parse(content.AsSpan(0, newLineIndex));
        var body = new string(content.AsSpan(newLineIndex + 1));
        return new TestResponse()
        {
            Key = key,
            StatusCode = statusCode,
            ResponseBody = body
        };
    }
}