using Meridian.Bll.QueryPipeline.Steps;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;

namespace Meridian.UnitTests.QueryPipeline;

// Each step in isolation over an in-memory IQueryable<BoardCard> — the steps
// are pure System.Linq, so no EF is involved.
public class QueryStepTests
{
    private static IQueryable<BoardCard> Cards() => new List<BoardCard>
    {
        new() { Id = 1, Title = "Safety", Type = CardType.Safety, Status = CardStatus.Done, Order = 3 },
        new() { Id = 2, Title = "Handbook", Type = CardType.Resource, Status = CardStatus.ToDo, Order = 1 },
        new() { Id = 3, Title = "Dev setup", Type = CardType.Resource, Status = CardStatus.InProgress, Order = 2 },
        new() { Id = 4, Title = "HR contact", Type = CardType.Contact, Status = CardStatus.ToDo, Order = 4 }
    }.AsQueryable();

    [Fact]
    public void CardStatusFilterStep_KeepsOnlyMatchingStatus()
    {
        var result = new CardStatusFilterStep(CardStatus.ToDo).Apply(Cards()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(CardStatus.ToDo, c.Status));
    }

    [Fact]
    public void CardTypeFilterStep_KeepsOnlyMatchingType()
    {
        var result = new CardTypeFilterStep(CardType.Resource).Apply(Cards()).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(CardType.Resource, c.Type));
    }

    [Fact]
    public void OrderSortStep_Ascending_SortsByOrder()
    {
        var result = new OrderSortStep().Apply(Cards()).ToList();

        Assert.Equal(new[] { 1, 2, 3, 4 }, result.Select(c => c.Order));
    }

    [Fact]
    public void OrderSortStep_Descending_SortsByOrderDesc()
    {
        var result = new OrderSortStep(descending: true).Apply(Cards()).ToList();

        Assert.Equal(new[] { 4, 3, 2, 1 }, result.Select(c => c.Order));
    }

    [Fact]
    public void PagingStep_ReturnsTheRequestedWindow()
    {
        var sorted = new OrderSortStep().Apply(Cards());

        var page2 = new PagingStep<BoardCard>(page: 2, pageSize: 2).Apply(sorted).ToList();

        Assert.Equal(new[] { 3, 4 }, page2.Select(c => c.Order));
    }
}
