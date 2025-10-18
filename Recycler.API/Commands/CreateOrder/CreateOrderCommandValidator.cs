using FluentValidation;

namespace Recycler.API;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CompanyName.Length).GreaterThan(0)
            .WithMessage("Specify a Company Name.");
        RuleFor(x => x.OrderItems.Count()).GreaterThan(0)
            .WithMessage("Need at least one order item.");
    }
}