using Meridian.Bll.Dtos;
using Meridian.Bll.QueryPipeline;
using Meridian.Dal.Persistence;
using Meridian.Dal.Repositories;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meridian.UnitTests.Repositories;

// Exercises the real Dal query execution (apply steps -> count -> page ->
// materialize) against the EF in-memory provider. Test-only Dal usage, like
// the caching decorator tests.
public class OnboardingBoardRepositoryQueryTests
{
    private static MeridianDbContext NewContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MeridianDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new MeridianDbContext(options);
    }

    private static async Task SeedBoardAsync(MeridianDbContext context)
    {
        context.Add(new OnboardingBoard
        {
            HireUserId = 42,
            AssignedAt = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
            Cards = new List<BoardCard>
            {
                new() { Title = "A", Description = "d", Type = CardType.Resource, Status = CardStatus.ToDo, Order = 1 },
                new() { Title = "B", Description = "d", Type = CardType.Resource, Status = CardStatus.ToDo, Order = 2 },
                new() { Title = "C", Description = "d", Type = CardType.Contact, Status = CardStatus.ToDo, Order = 3 },
                new() { Title = "D", Description = "d", Type = CardType.Resource, Status = CardStatus.Done, Order = 4 },
                new() { Title = "E", Description = "d", Type = CardType.Resource, Status = CardStatus.ToDo, Order = 5 }
            }
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetBoardCardsAsync_TotalCountReflectsFilteredSet_NotThePagedWindow()
    {
        await using var context = NewContext(nameof(GetBoardCardsAsync_TotalCountReflectsFilteredSet_NotThePagedWindow));
        await SeedBoardAsync(context);
        var sut = new OnboardingBoardRepository(context);

        // ToDo + Resource matches A, B, E (3 cards); page 1 of size 2 windows to 2.
        var steps = BoardCardsPipeline.Compose(new BoardCardsQueryDto
        {
            Status = "ToDo",
            Type = "Resource",
            Page = 1,
            PageSize = 2
        });

        var result = await sut.GetBoardCardsAsync(42, steps);

        Assert.NotNull(result);
        Assert.Equal(3, result!.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(new[] { "A", "B" }, result.Items.Select(c => c.Title));
    }

    [Fact]
    public async Task GetBoardCardsAsync_AppliesSortDescending()
    {
        await using var context = NewContext(nameof(GetBoardCardsAsync_AppliesSortDescending));
        await SeedBoardAsync(context);
        var sut = new OnboardingBoardRepository(context);

        var steps = BoardCardsPipeline.Compose(new BoardCardsQueryDto { Sort = "desc", Page = 1, PageSize = 3 });

        var result = await sut.GetBoardCardsAsync(42, steps);

        Assert.NotNull(result);
        Assert.Equal(5, result!.TotalCount);
        Assert.Equal(new[] { "E", "D", "C" }, result.Items.Select(c => c.Title));
    }

    [Fact]
    public async Task GetBoardCardsAsync_WhenHireHasNoBoard_ReturnsNull()
    {
        await using var context = NewContext(nameof(GetBoardCardsAsync_WhenHireHasNoBoard_ReturnsNull));
        await SeedBoardAsync(context);
        var sut = new OnboardingBoardRepository(context);

        var result = await sut.GetBoardCardsAsync(999, BoardCardsPipeline.Compose(new BoardCardsQueryDto()));

        Assert.Null(result);
    }
}
