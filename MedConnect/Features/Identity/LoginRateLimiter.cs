using System.Collections.Concurrent;

namespace MedConnect.Features.Identity;

/// In-memory sliding-window limiter (per caller key, normally the client IP).
/// Guards the public register/login endpoints against credential stuffing on
/// top of the per-account Identity lockout.
public class LoginRateLimiter
{
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _attempts = new();
    private const int MaxAttempts = 10;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    public (bool Allowed, TimeSpan RetryAfter) TryAcquire(string key)
    {
        var now = DateTime.UtcNow;
        var queue = _attempts.GetOrAdd(key, _ => new Queue<DateTime>());

        lock (queue)
        {
            while (queue.Count > 0 && now - queue.Peek() > Window)
            {
                queue.Dequeue();
            }

            if (queue.Count >= MaxAttempts)
            {
                var oldest = queue.Peek();
                return (false, oldest + Window - now);
            }

            queue.Enqueue(now);
            return (true, TimeSpan.Zero);
        }
    }
}