using Meridian.Dal.Decorators;
using Meridian.Domain.Entities;
using Meridian.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Meridian.UnitTests.Decorators;

public class CachedOnboardingTemplateRepositoryTests
{
    private readonly IOnboardingTemplateRepository _inner = Substitute.For<IOnboardingTemplateRepository>();
    private readonly CachedOnboardingTemplateRepository _sut;

    public CachedOnboardingTemplateRepositoryTests()
    {
        _sut = new CachedOnboardingTemplateRepository(_inner, new MemoryCache(new MemoryCacheOptions()));
    }

    private static OnboardingTemplate Template() => new() { Id = 1, Name = "Default Onboarding" };

    [Fact]
    public async Task GetByIdAsync_SecondCall_ServedFromCache_InnerCalledOnce()
    {
        _inner.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Template());

        var first = await _sut.GetByIdAsync(1);
        var second = await _sut.GetByIdAsync(1);

        Assert.NotNull(first);
        Assert.Equal(first!.Name, second!.Name);
        await _inner.Received(1).GetByIdAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_SecondCall_ServedFromCache_InnerCalledOnce()
    {
        _inner.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OnboardingTemplate> { Template() });

        var first = await _sut.GetAllAsync();
        var second = await _sut.GetAllAsync();

        Assert.Single(first);
        Assert.Single(second);
        await _inner.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_Miss_IsNotCached_InnerCalledEachTime()
    {
        _inner.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((OnboardingTemplate?)null);

        Assert.Null(await _sut.GetByIdAsync(99));
        Assert.Null(await _sut.GetByIdAsync(99));

        await _inner.Received(2).GetByIdAsync(99, Arg.Any<CancellationToken>());
    }
}
