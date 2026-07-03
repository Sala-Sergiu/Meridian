using FluentValidation;
using Meridian.Bll.Dtos;

namespace Meridian.Bll.Validators;

public class CalendarQueryValidator : AbstractValidator<CalendarQueryDto>
{
    public CalendarQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).When(x => x.Year is not null);
        RuleFor(x => x.Month).InclusiveBetween(1, 12).When(x => x.Month is not null);
    }
}
