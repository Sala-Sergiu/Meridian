using Mapster;
using Meridian.Bll.Mapping;
using Meridian.Bll.Services;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Domain.Repositories;
using NSubstitute;

namespace Meridian.UnitTests.Services;

public class OnboardingTemplateServiceTests
{
    private readonly IOnboardingTemplateRepository _repository = Substitute.For<IOnboardingTemplateRepository>();
    private readonly IOnboardingBoardRepository _boards = Substitute.For<IOnboardingBoardRepository>();
    private readonly OnboardingTemplateService _sut;

    static OnboardingTemplateServiceTests()
    {
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    public OnboardingTemplateServiceTests()
    {
        _sut = new OnboardingTemplateService(_repository, _boards);
    }

    private static OnboardingTemplate SeededTemplate() => new()
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
                Id = 6,
                TemplateId = 1,
                Title = "HR contact",
                Description = "Contracts, payroll and anything people-related.",
                Type = CardType.Contact,
                Url = "mailto:hr@meridian.local",
                Order = 6
            }
        }
    };

    [Fact]
    public async Task GetTemplateAsync_ReturnsTemplateWithCardsMappedToDtos()
    {
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(SeededTemplate());

        var result = await _sut.GetTemplateAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Default Onboarding", result.Name);
        Assert.Equal(2, result.Cards.Count);

        var safety = result.Cards[0];
        Assert.Equal("Workplace safety basics", safety.Title);
        Assert.Equal("Safety", safety.Type);
        Assert.Equal("https://intranet.meridian.local/safety/basics", safety.Url);
        Assert.Equal(1, safety.Order);

        var contact = result.Cards[1];
        Assert.Equal("Contact", contact.Type);
        Assert.Equal("mailto:hr@meridian.local", contact.Url);
    }

    [Fact]
    public async Task GetTemplateAsync_WithUnknownId_ReturnsNull()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((OnboardingTemplate?)null);

        var result = await _sut.GetTemplateAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTemplatesAsync_ReturnsAllTemplatesAsDtos()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OnboardingTemplate> { SeededTemplate() });

        var result = await _sut.GetTemplatesAsync();

        var template = Assert.Single(result);
        Assert.Equal("Default Onboarding", template.Name);
        Assert.Equal(2, template.Cards.Count);
    }
}
