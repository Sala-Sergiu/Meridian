using FluentValidation;
using Mapster;
using Meridian.Bll.Dtos;
using Meridian.Bll.Mapping;
using Meridian.Bll.Services;
using Meridian.Domain.Common;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Domain.Repositories;
using NSubstitute;

namespace Meridian.UnitTests.Services;

public class OnboardingBoardServiceTests
{
    private readonly IOnboardingTemplateRepository _templates = Substitute.For<IOnboardingTemplateRepository>();
    private readonly IOnboardingBoardRepository _boards = Substitute.For<IOnboardingBoardRepository>();
    private readonly OnboardingBoardService _sut;

    static OnboardingBoardServiceTests()
    {
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    public OnboardingBoardServiceTests()
    {
        _sut = new OnboardingBoardService(_templates, _boards);
    }

    private static OnboardingTemplate Template() => new()
    {
        Id = 1,
        Name = "Default Onboarding",
        Cards = new List<TemplateCard>
        {
            new()
            {
                Id = 1,
                TemplateId = 1,
                Title = "Workplace safety basics",
                Description = "Required reading before your first day.",
                Type = CardType.Safety,
                Url = "https://intranet.meridian.local/safety/basics",
                Order = 1
            },
            new()
            {
                Id = 2,
                TemplateId = 1,
                Title = "HR contact",
                Description = "Contracts, payroll and anything people-related.",
                Type = CardType.Contact,
                Url = "mailto:hr@meridian.local",
                Order = 2
            }
        }
    };

    [Fact]
    public async Task AssignAsync_ClonesAllTemplateCards_WithToDoStatus_AndCorrectOwner()
    {
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns((OnboardingBoard?)null);
        _templates.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Template());

        var result = await _sut.AssignAsync(1, 42);

        Assert.NotNull(result);
        Assert.False(result!.AlreadyExisted);
        Assert.Equal(42, result.Board.HireUserId);
        Assert.Equal(2, result.Board.Cards.Count);

        var safety = result.Board.Cards[0];
        Assert.Equal("Workplace safety basics", safety.Title);
        Assert.Equal("Safety", safety.Type);
        Assert.Equal("https://intranet.meridian.local/safety/basics", safety.Url);
        Assert.Equal(1, safety.Order);
        Assert.All(result.Board.Cards, c => Assert.Equal("ToDo", c.Status));

        await _boards.Received(1).AddAsync(
            Arg.Is<OnboardingBoard>(b => b.HireUserId == 42 && b.Cards.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignAsync_BoardIsIndependentOfTemplateAfterCloning()
    {
        var template = Template();
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns((OnboardingBoard?)null);
        _templates.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(template);

        OnboardingBoard? persisted = null;
        await _boards.AddAsync(
            Arg.Do<OnboardingBoard>(b => persisted = b),
            Arg.Any<CancellationToken>());

        await _sut.AssignAsync(1, 42);

        Assert.NotNull(persisted);

        // Editing the template after assignment must not reach the cloned board.
        template.Cards.First().Title = "EDITED AFTER ASSIGNMENT";

        Assert.Equal("Workplace safety basics", persisted!.Cards.First().Title);
    }

    [Fact]
    public async Task AssignAsync_WhenHireAlreadyHasBoard_ReturnsExisting_NoDuplicate()
    {
        var existing = new OnboardingBoard
        {
            Id = 7,
            HireUserId = 42,
            Cards = new List<BoardCard>
            {
                new() { Id = 1, BoardId = 7, Title = "Already there", Description = "x", Order = 1, Status = CardStatus.Done }
            }
        };
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _sut.AssignAsync(1, 42);

        Assert.NotNull(result);
        Assert.True(result!.AlreadyExisted);
        Assert.Equal(7, result.Board.Id);

        await _boards.DidNotReceive().AddAsync(Arg.Any<OnboardingBoard>(), Arg.Any<CancellationToken>());
        await _templates.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignAsync_WithUnknownTemplate_ReturnsNull_AndPersistsNothing()
    {
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns((OnboardingBoard?)null);
        _templates.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((OnboardingTemplate?)null);

        var result = await _sut.AssignAsync(99, 42);

        Assert.Null(result);
        await _boards.DidNotReceive().AddAsync(Arg.Any<OnboardingBoard>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyBoardAsync_ReturnsBoardMappedToDtos()
    {
        var board = new OnboardingBoard
        {
            Id = 7,
            HireUserId = 42,
            AssignedAt = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
            Cards = new List<BoardCard>
            {
                new()
                {
                    Id = 1,
                    BoardId = 7,
                    Title = "Workplace safety basics",
                    Description = "Required reading before your first day.",
                    Type = CardType.Safety,
                    Url = "https://intranet.meridian.local/safety/basics",
                    Order = 1,
                    Status = CardStatus.InProgress
                }
            }
        };
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns(board);

        var result = await _sut.GetMyBoardAsync(42);

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        Assert.Equal(42, result.HireUserId);
        Assert.Equal(new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc), result.AssignedAt);

        var card = Assert.Single(result.Cards);
        Assert.Equal("Workplace safety basics", card.Title);
        Assert.Equal("Safety", card.Type);
        Assert.Equal("InProgress", card.Status);
    }

    [Fact]
    public async Task GetMyBoardAsync_WhenNoBoardAssigned_ReturnsNull()
    {
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns((OnboardingBoard?)null);

        var result = await _sut.GetMyBoardAsync(42);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMyBoardCardsAsync_PassesComposedStepsIn_AndMapsPagedResult()
    {
        var cards = new List<BoardCard>
        {
            new() { Id = 1, Title = "Handbook", Description = "d", Type = CardType.Resource, Status = CardStatus.ToDo, Order = 1 }
        };
        _boards.GetBoardCardsAsync(42, Arg.Any<IReadOnlyList<IQueryStep<BoardCard>>>(), Arg.Any<CancellationToken>())
            .Returns(new PagedItems<BoardCard>(cards, TotalCount: 7));

        var result = await _sut.GetMyBoardCardsAsync(42, new BoardCardsQueryDto { Page = 2, PageSize = 5 });

        Assert.NotNull(result);
        Assert.Equal(2, result!.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(7, result.TotalCount);
        Assert.Equal(2, result.TotalPages);

        var card = Assert.Single(result.Items);
        Assert.Equal("Handbook", card.Title);
        Assert.Equal("ToDo", card.Status);

        await _boards.Received(1).GetBoardCardsAsync(
            42,
            Arg.Is<IReadOnlyList<IQueryStep<BoardCard>>>(steps => steps.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyBoardCardsAsync_WhenNoBoardAssigned_ReturnsNull()
    {
        _boards.GetBoardCardsAsync(42, Arg.Any<IReadOnlyList<IQueryStep<BoardCard>>>(), Arg.Any<CancellationToken>())
            .Returns((PagedItems<BoardCard>?)null);

        var result = await _sut.GetMyBoardCardsAsync(42, new BoardCardsQueryDto());

        Assert.Null(result);
    }

    private static OnboardingBoard BoardWithCard(int hireUserId, int cardId, CardStatus status = CardStatus.ToDo) => new()
    {
        Id = 7,
        HireUserId = hireUserId,
        Cards = new List<BoardCard>
        {
            new() { Id = cardId, BoardId = 7, Title = "Handbook", Description = "d", Type = CardType.Resource, Status = status, Order = 1 }
        }
    };

    [Fact]
    public async Task MoveCardAsync_OwnerMovesOwnCard_UpdatesStatus_AndReturnsDto()
    {
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns(BoardWithCard(42, cardId: 5));

        var result = await _sut.MoveCardAsync(42, 5, new MoveCardRequestDto { Status = "InProgress" });

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
        Assert.Equal("InProgress", result.Status);

        await _boards.Received(1).UpdateCardAsync(
            Arg.Is<BoardCard>(c => c.Id == 5 && c.Status == CardStatus.InProgress),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveCardAsync_CardOnAnotherHiresBoard_ReturnsNull_PersistsNothing()
    {
        // Card 99 lives on another hire's board — the caller's own board does
        // not contain it, which must be indistinguishable from "no such card".
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns(BoardWithCard(42, cardId: 5));

        var result = await _sut.MoveCardAsync(42, 99, new MoveCardRequestDto { Status = "Done" });

        Assert.Null(result);
        await _boards.DidNotReceive().UpdateCardAsync(Arg.Any<BoardCard>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveCardAsync_WhenCallerHasNoBoard_ReturnsNull_PersistsNothing()
    {
        _boards.GetByHireIdAsync(2, Arg.Any<CancellationToken>()).Returns((OnboardingBoard?)null);

        var result = await _sut.MoveCardAsync(2, 5, new MoveCardRequestDto { Status = "Done" });

        Assert.Null(result);
        await _boards.DidNotReceive().UpdateCardAsync(Arg.Any<BoardCard>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveCardAsync_InvalidStatus_ThrowsValidationException_PersistsNothing()
    {
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns(BoardWithCard(42, cardId: 5));

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.MoveCardAsync(42, 5, new MoveCardRequestDto { Status = "Shipped" }));

        await _boards.DidNotReceive().UpdateCardAsync(Arg.Any<BoardCard>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveCardAsync_StatusIsCaseInsensitive()
    {
        _boards.GetByHireIdAsync(42, Arg.Any<CancellationToken>()).Returns(BoardWithCard(42, cardId: 5));

        var result = await _sut.MoveCardAsync(42, 5, new MoveCardRequestDto { Status = "done" });

        Assert.Equal("Done", result!.Status);
    }
}
