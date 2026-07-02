using FluentValidation;
using Meridian.Bll.Dtos;

namespace Meridian.Bll.Validators;

// Validation rules for the board assignment request.
public class AssignBoardRequestValidator : AbstractValidator<AssignBoardRequestDto>
{
    public AssignBoardRequestValidator()
    {
        RuleFor(x => x.TemplateId).GreaterThan(0);

        RuleFor(x => x.HireUserId).GreaterThan(0);
    }
}
