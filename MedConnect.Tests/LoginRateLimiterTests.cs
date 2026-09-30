using MedConnect.Features.Identity;
using Xunit;

namespace MedConnect.Tests;

public class LoginRateLimiterTests
{
    [Fact]
    public void AllowsUpToMaxAttemptsThenBlocks()
    {
        var limiter = new LoginRateLimiter();
        for (var i = 0; i < 10; i++)
        {
            var (allowed, _) = limiter.TryAcquire("192.168.1.1");
            Assert.True(allowed);
        }

        var (blocked, retryAfter) = limiter.TryAcquire("192.168.1.1");
        Assert.False(blocked);
        Assert.True(retryAfter > TimeSpan.Zero);
    }

    [Fact]
    public void DifferentKeysAreIndependent()
    {
        var limiter = new LoginRateLimiter();
        for (var i = 0; i < 10; i++)
        {
            limiter.TryAcquire("192.168.1.2");
        }

        Assert.False(limiter.TryAcquire("192.168.1.2").Allowed);
        Assert.True(limiter.TryAcquire("192.168.1.3").Allowed);
    }
}