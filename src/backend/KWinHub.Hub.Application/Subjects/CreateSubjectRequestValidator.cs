using FluentValidation;
using KWinHub.Contracts.Subjects;
using KWinHub.Domain.Subjects;

namespace KWinHub.Application.Subjects;

public class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(r => r.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(r => r.SubjectType)
            .NotEmpty()
            .Must(BeValidSubjectType)
            .WithMessage("SubjectType must be one of: Athlete, Team, Individual, Group");

        RuleFor(r => r.ExternalId)
            .MaximumLength(200)
            .When(r => r.ExternalId is not null);

        RuleFor(r => r.DisplayName)
            .MaximumLength(200)
            .When(r => r.DisplayName is not null);

        RuleFor(r => r.Sex)
            .MaximumLength(50)
            .When(r => r.Sex is not null);
    }

    private static bool BeValidSubjectType(string subjectType) =>
        Enum.TryParse<SubjectType>(subjectType, true, out _);
}