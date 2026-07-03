using FluentValidation;
using Mapster;
using Meridian.Bll.Dtos;
using Meridian.Bll.Mapping;
using Meridian.Bll.Services;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Domain.Repositories;
using NSubstitute;

namespace Meridian.UnitTests.Services;

public class PublishCardTests
{
    private readonly IOnboardingTemplateRepository _templates = Substitute.For<IOnboardingTemplateRepository>();
    private readonly IOnboardingBoardRepository _boards = Substitute.For<IOnboardingBoardRepository>();
    private readonly OnboardingTemplateService _sut;

    static PublishCardTests()
    {
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    public PublishCardTests()
    {
        _sut = new OnboardingTemplateService(_templates, _boards);
        _boards.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<OnboardingBoard>());
    }

    private static PublishCardRequestDto Request() => new()
    {
        Title = "GDPR refresher",
        Description = "Annual privacy training.",
        Type = "Safety",
        Url = "/resources/data-confidentiality"
    };

    private void TemplateIs(OnboardingTemplate? template) =>
        _templates.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(template);

    [Fact]
    public async Task Appends_the_card_to_the_template_with_the_next_order()
    {
        TemplateIs(new OnboardingTemplate
        {
            Id = 1,
            Name = "Default",
            Cards = new List<TemplateCard> { new() { Id = 1, Order = 4, Title = "x", Description = "x" } }
        });
        TemplateCard? added = null;
        await _templates.AddCardAsync(Arg.Do<TemplateCard>(c => added = c), Arg.Any<CancellationToken>());

        var result = await _sut.PublishCardAsync(1, Request());

        Assert.NotNull(result);
        Assert.NotNull(added);
        Assert.Equal(1, added!.TemplateId);
        Assert.Equal(5, added.Order);
        Assert.Equal(CardType.Safety, added.Type);
    }

    [Fact]
    public async Task Broadcasts_a_todo_copy_to_every_existing_board_with_per_board_order()
    {
        TemplateIs(new OnboardingTemplate { Id = 1, Name = "Default" });
        _boards.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<OnboardingBoard>
        {
            new() { Id = 10, HireUserId = 1, Cards = new List<BoardCard> { new() { Order = 8, Status = CardStatus.Done, Title = "x", Description = "x" } } },
            new() { Id = 11, HireUserId = 4 } // empty board
        });
        IReadOnlyList<BoardCard>? broadcast = null;
        await _boards.AddCardsAsync(Arg.Do<IReadOnlyList<BoardCard>>(c => broadcast = c), Arg.Any<CancellationToken>());

        var result = await _sut.PublishCardAsync(1, Request());

        Assert.Equal(2, result!.BoardsUpdated);
        Assert.NotNull(broadcast);
        Assert.All(broadcast!, c => Assert.Equal(CardStatus.ToDo, c.Status));
        Assert.Equal(9, broadcast!.Single(c => c.BoardId == 10).Order);
        Assert.Equal(1, broadcast.Single(c => c.BoardId == 11).Order);
    }

    [Fact]
    public async Task Unknown_template_returns_null_and_nothing_is_written()
    {
        TemplateIs(null);

        var result = await _sut.PublishCardAsync(1, Request());

        Assert.Null(result);
        await _templates.DidNotReceive().AddCardAsync(Arg.Any<TemplateCard>(), Arg.Any<CancellationToken>());
        await _boards.DidNotReceive().AddCardsAsync(Arg.Any<IReadOnlyList<BoardCard>>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Contact")]
    [InlineData("Shipped")]
    public async Task Contact_or_invalid_types_are_rejected(string type)
    {
        TemplateIs(new OnboardingTemplate { Id = 1, Name = "Default" });
        var request = Request();
        request.Type = type;

        await Assert.ThrowsAsync<ValidationException>(() => _sut.PublishCardAsync(1, request));
        await _templates.DidNotReceive().AddCardAsync(Arg.Any<TemplateCard>(), Arg.Any<CancellationToken>());
    }
}
