using EmailAutomation.API.Models;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EmailAutomation.API.Services;

public interface IEmailQueue
{
    ValueTask EnqueueEmailAsync(EmailRequest request);
    ValueTask<EmailRequest> DequeueEmailAsync(CancellationToken cancellationToken);
}

public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailRequest> _queue;

    public EmailQueue(int capacity)
    {
        // Define bounded channel based on capacity
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<EmailRequest>(options);
    }

    public async ValueTask EnqueueEmailAsync(EmailRequest request)
    {
        if (request == null)
            throw new System.ArgumentNullException(nameof(request));

        await _queue.Writer.WriteAsync(request);
    }

    public async ValueTask<EmailRequest> DequeueEmailAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
