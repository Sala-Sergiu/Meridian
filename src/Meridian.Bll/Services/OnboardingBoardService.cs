using FluentValidation;
using FluentValidation.Results;
using Mapster;
using Meridian.Bll.Dtos;
using Meridian.Bll.QueryPipeline;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Domain.Repositories;

namespace Meridian.Bll.Services;

// Template-to-board cloning (Variant A — clone at assignment) and board reads.
// The template is read through IOnboardingTemplateRepository, so assignment
// benefits from the caching decorator composed in at the root. After cloning,
// the board is fully independent of the template.
public class OnboardingBoardService : IOnboardingBoardService
{
    private readonly IOnboardingTemplateRepository _templates;
    private readonly IOnboardingBoardRepository _boards;

    public OnboardingBoardService(
        IOnboardingTemplateRepository templates,
        IOnboardingBoardRepository boards)
    {
        _templates = templates;
        _boards = boards;
    }

    public async Task<AssignBoardResultDto?> AssignAsync(int templateId, int hireUserId, CancellationToken cancellationToken = default)
    {
        // Guard: a hire has at most one board. Assigning again is idempotent —
        // the existing board is returned untouched (flagged AlreadyExisted)
        // rather than treated as an error, so a double-click by HR is harmless.
        var existing = await _boards.GetByHireIdAsync(hireUserId, cancellationToken);
        if (existing is not null)
        {
            return new AssignBoardResultDto
            {
                Board = existing.Adapt<OnboardingBoardDto>(),
                AlreadyExisted = true
            };
        }

        var template = await _templates.GetByIdAsync(templateId, cancellationToken);
        if (template is null)
        {
            return null;
        }

        var board = new OnboardingBoard
        {
            HireUserId = hireUserId,
            AssignedAt = DateTime.UtcNow,
            // Clone every template card into an independent board card: content
            // is copied, progress starts at ToDo. Later template edits will not
            // reach this board.
            Cards = template.Cards
                .OrderBy(c => c.Order)
                .Select(c => new BoardCard
                {
                    Title = c.Title,
                    Description = c.Description,
                    Type = c.Type,
                    Url = c.Url,
                    Order = c.Order,
                    Status = CardStatus.ToDo
                })
                .ToList()
        };

        await _boards.AddAsync(board, cancellationToken);

        return new AssignBoardResultDto
        {
            Board = board.Adapt<OnboardingBoardDto>(),
            AlreadyExisted = false
        };
    }

    public async Task<OnboardingBoardDto?> GetMyBoardAsync(int hireUserId, CancellationToken cancellationToken = default)
    {
        var board = await _boards.GetByHireIdAsync(hireUserId, cancellationToken);
        return board?.Adapt<OnboardingBoardDto>();
    }

    public async Task<BoardCardDto?> MoveCardAsync(
        int hireUserId,
        int cardId,
        MoveCardRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Defense in depth: the API validator already rejected bad values, but
        // this use-case must hold on its own for any future caller.
        if (!Enum.TryParse<CardStatus>(request.Status, ignoreCase: true, out var newStatus))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    nameof(request.Status),
                    $"Status must be one of: {string.Join(", ", Enum.GetNames<CardStatus>())}.")
            });
        }

        // Ownership by construction: only the caller's own board is loaded, so
        // a card on another hire's board is indistinguishable from a card that
        // does not exist — both return null (404), never revealing existence.
        var board = await _boards.GetByHireIdAsync(hireUserId, cancellationToken);
        var card = board?.Cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
        {
            return null;
        }

        card.Status = newStatus;
        await _boards.UpdateCardAsync(card, cancellationToken);

        return card.Adapt<BoardCardDto>();
    }

    public async Task<PagedResult<BoardCardDto>?> GetMyBoardCardsAsync(
        int hireUserId,
        BoardCardsQueryDto query,
        CancellationToken cancellationToken = default)
    {
        // Bll composes the steps and passes them in; Dal applies and
        // materializes — IQueryable never crosses this boundary.
        var steps = BoardCardsPipeline.Compose(query);
        var page = await _boards.GetBoardCardsAsync(hireUserId, steps, cancellationToken);
        if (page is null)
        {
            return null;
        }

        return new PagedResult<BoardCardDto>
        {
            Items = page.Items.Adapt<List<BoardCardDto>>(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = page.TotalCount
        };
    }
}
