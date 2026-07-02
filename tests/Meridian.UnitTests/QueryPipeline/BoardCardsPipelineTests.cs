using Meridian.Bll.Dtos;
using Meridian.Bll.QueryPipeline;
using Meridian.Bll.QueryPipeline.Steps;
using Meridian.Domain.Common;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;

namespace Meridian.UnitTests.QueryPipeline;

// The composer assembles steps from the query-parameters object in a fixed
// order: filters -> sort -> paging.
public class BoardCardsPipelineTests
{
    private static IQueryable<BoardCard> Cards() => new List<BoardCard>
    {
        new() { Id = 1, Type = CardType.Resource, Status = CardStatus.ToDo, Order = 2 },
        new() { Id = 2, Type = CardType.Resource, Status = CardStatus.Done, Order = 1 },
        new() { Id = 3, Type = CardType.Contact, Status = CardStatus.ToDo, Order = 3 },
        new() { Id = 4, Type = CardType.Resource, Status = CardStatus.ToDo, Order = 4 }
    }.AsQueryable();

    private static IQueryable<BoardCard> ApplyAll(IEnumerable<IQueryStep<BoardCard>> steps, IQueryable<BoardCard> source)
        => steps.Aggregate(source, (query, step) => step.Apply(query));

    [Fact]
    public void Compose_WithDefaults_YieldsSortThenPaging()
    {
        var steps = BoardCardsPipeline.Compose(new BoardCardsQueryDto());

        Assert.Collection(steps,
            s => Assert.IsType<OrderSortStep>(s),
            s => Assert.IsType<PagingStep<BoardCard>>(s));

        var result = ApplyAll(steps, Cards()).ToList();
        Assert.Equal(new[] { 1, 2, 3, 4 }, result.Select(c => c.Order));
    }

    [Fact]
    public void Compose_WithAllParams_OrdersStepsFiltersThenSortThenPaging()
    {
        var query = new BoardCardsQueryDto
        {
            Status = "todo",
            Type = "resource",
            Sort = "desc",
            Page = 1,
            PageSize = 20
        };

        var steps = BoardCardsPipeline.Compose(query);

        Assert.Collection(steps,
            s => Assert.IsType<CardStatusFilterStep>(s),
            s => Assert.IsType<CardTypeFilterStep>(s),
            s => Assert.IsType<OrderSortStep>(s),
            s => Assert.IsType<PagingStep<BoardCard>>(s));

        var result = ApplyAll(steps, Cards()).ToList();

        // Resource + ToDo cards only (ids 1 and 4), sorted by Order descending.
        Assert.Equal(new[] { 4, 2 }, result.Select(c => c.Order));
        Assert.Equal(new[] { 4, 1 }, result.Select(c => c.Id));
    }

    [Fact]
    public void Compose_PagingWindowsTheFilteredSortedSet()
    {
        var query = new BoardCardsQueryDto { Status = "ToDo", Page = 2, PageSize = 2 };

        var result = ApplyAll(BoardCardsPipeline.Compose(query), Cards()).ToList();

        // ToDo cards sorted asc are Orders 2, 3, 4 — page 2 of size 2 is just Order 4.
        var only = Assert.Single(result);
        Assert.Equal(4, only.Order);
    }
}
