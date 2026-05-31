using FluentValidation;

namespace AssetVest.Application.Commands.AnnualGoals.CreateAnnualGoal;

public class CreateAnnualGoalCommandValidator : AbstractValidator<CreateAnnualGoalCommand>
{
    public CreateAnnualGoalCommandValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2100).WithMessage("Year must be between 2020 and 2100");

        RuleFor(x => x.TargetTotalPortfolioValueEGP)
            .GreaterThan(0).WithMessage("Target portfolio value must be greater than 0");

        RuleFor(x => x.TargetProfitPercent)
            .InclusiveBetween(0, 1000).When(x => x.TargetProfitPercent.HasValue)
            .WithMessage("Target profit percent must be between 0 and 1000");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 1000 characters");

        When(x => x.AllocationGoals != null && x.AllocationGoals.Count > 0, () =>
        {
            RuleForEach(x => x.AllocationGoals).ChildRules(goal =>
            {
                goal.RuleFor(g => g.AssetType)
                    .IsInEnum().WithMessage("Invalid asset type");

                goal.RuleFor(g => g.TargetAllocationPercent)
                    .InclusiveBetween(0, 100).WithMessage("Allocation percent must be between 0 and 100");
            });

            RuleFor(x => x.AllocationGoals)
                .Must(goals => goals!.Sum(g => g.TargetAllocationPercent) <= 100)
                .WithMessage("Total allocation percentages cannot exceed 100%");
        });
    }
}
