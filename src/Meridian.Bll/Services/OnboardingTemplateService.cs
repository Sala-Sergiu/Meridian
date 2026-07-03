using FluentValidation;
using FluentValidation.Results;
using Mapster;
using Meridian.Bll.Dtos;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Domain.Repositories;

namespace Meridian.Bll.Services;

// Template read logic + HR publishing. Depends on the Domain repository
// interfaces only; the cached Dal implementation is composed in behind it at
// the composition root (publishing invalidates that cache).
public class OnboardingTemplateService : IOnboardingTemplateService
{
    private readonly IOnboardingTemplateRepository _templates;
    private readonly IOnboardingBoardRepository _boards;

    public OnboardingTemplateService(
        IOnboardingTemplateRepository templates,
        IOnboardingBoardRepository boards)
    {
        _templates = templates;
        _boards = boards;
    }

    public async Task<OnboardingTemplateDto?> GetTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        var template = await _templates.GetByIdAsync(id, cancellationToken);
        return template?.Adapt<OnboardingTemplateDto>();
    }

    public async Task<IReadOnlyList<OnboardingTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _templates.GetAllAsync(cancellationToken);
        return templates.Adapt<List<OnboardingTemplateDto>>();
    }

    public async Task<PublishCardResultDto?> PublishCardAsync(
        int templateId,
        PublishCardRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Defense in depth: the API validator already rejected bad values, but
        // this use-case must hold on its own for any future caller.
        if (!Enum.TryParse<CardType>(request.Type, ignoreCase: true, out var type)
            || type == CardType.Contact)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    nameof(request.Type),
                    "Type must be Safety or Resource — contacts are not broadcast.")
            });
        }

        var template = await _templates.GetByIdAsync(templateId, cancellationToken);
        if (template is null)
        {
            return null;
        }

        var templateCard = new TemplateCard
        {
            TemplateId = templateId,
            Title = request.Title,
            Description = request.Description,
            Type = type,
            Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url,
            Order = template.Cards.Count == 0 ? 1 : template.Cards.Max(c => c.Order) + 1
        };
        await _templates.AddCardAsync(templateCard, cancellationToken);

        // Broadcast: a ToDo copy onto every existing board, appended after
        // that board's own last card (boards may have diverged from the
        // template, so the order is per board).
        var boards = await _boards.GetAllAsync(cancellationToken);
        var boardCards = boards
            .Select(board => new BoardCard
            {
                BoardId = board.Id,
                Title = templateCard.Title,
                Description = templateCard.Description,
                Type = templateCard.Type,
                Url = templateCard.Url,
                Order = board.Cards.Count == 0 ? 1 : board.Cards.Max(c => c.Order) + 1,
                Status = CardStatus.ToDo
            })
            .ToList();

        if (boardCards.Count > 0)
        {
            await _boards.AddCardsAsync(boardCards, cancellationToken);
        }

        return new PublishCardResultDto
        {
            Card = templateCard.Adapt<TemplateCardDto>(),
            BoardsUpdated = boardCards.Count
        };
    }
}
