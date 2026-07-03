using Mapster;
using Meridian.Bll.Mapping;
using Meridian.Bll.Services;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Domain.Repositories;
using NSubstitute;

namespace Meridian.UnitTests.Services;

public class HireProgressTests
{
    private readonly IOnboardingTemplateRepository _templates = Substitute.For<IOnboardingTemplateRepository>();
    private readonly IOnboardingBoardRepository _boards = Substitute.For<IOnboardingBoardRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly OnboardingBoardService _sut;

    static HireProgressTests()
    {
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    public HireProgressTests()
    {
        _sut = new OnboardingBoardService(_templates, _boards, _users);
    }

    private static BoardCard Card(CardType type, CardStatus status) => new()
    {
        Title = "Card",
        Description = "Description",
        Type = type,
        Status = status
    };

    [Fact]
    public async Task Counts_tasks_and_reading_separately_and_ignores_contacts()
    {
        _users.GetByRoleAsync(Role.NewHire, Arg.Any<CancellationToken>())
            .Returns(new List<User> { new() { Id = 1, DisplayName = "Nadia", Email = "n@meridian.local", Role = Role.NewHire } });
        _boards.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OnboardingBoard>
            {
                new()
                {
                    Id = 10,
                    HireUserId = 1,
                    Cards = new List<BoardCard>
                    {
                        Card(CardType.Resource, CardStatus.Done),
                        Card(CardType.Resource, CardStatus.InProgress),
                        Card(CardType.Safety, CardStatus.Done),
                        Card(CardType.Safety, CardStatus.ToDo),
                        Card(CardType.Contact, CardStatus.ToDo) // never counted
                    }
                }
            });

        var progress = await _sut.GetHireProgressAsync();

        var row = Assert.Single(progress);
        Assert.True(row.HasBoard);
        Assert.Equal(1, row.TasksDone);
        Assert.Equal(2, row.TasksTotal);
        Assert.Equal(1, row.ReadDone);
        Assert.Equal(2, row.ReadTotal);
    }

    [Fact]
    public async Task Hires_without_a_board_still_appear_with_zero_totals()
    {
        _users.GetByRoleAsync(Role.NewHire, Arg.Any<CancellationToken>())
            .Returns(new List<User>
            {
                new() { Id = 1, DisplayName = "Nadia", Email = "n@meridian.local", Role = Role.NewHire },
                new() { Id = 4, DisplayName = "Ben", Email = "b@meridian.local", Role = Role.NewHire }
            });
        _boards.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OnboardingBoard> { new() { Id = 10, HireUserId = 1 } });

        var progress = await _sut.GetHireProgressAsync();

        Assert.Equal(2, progress.Count);
        var ben = Assert.Single(progress, p => p.HireUserId == 4);
        Assert.False(ben.HasBoard);
        Assert.Equal(0, ben.TasksTotal);
        Assert.Equal(0, ben.ReadTotal);
    }

    [Fact]
    public async Task Only_new_hires_are_listed()
    {
        _users.GetByRoleAsync(Role.NewHire, Arg.Any<CancellationToken>())
            .Returns(new List<User>());
        _boards.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OnboardingBoard> { new() { Id = 10, HireUserId = 2 } });

        var progress = await _sut.GetHireProgressAsync();

        Assert.Empty(progress);
        await _users.Received(1).GetByRoleAsync(Role.NewHire, Arg.Any<CancellationToken>());
    }
}
