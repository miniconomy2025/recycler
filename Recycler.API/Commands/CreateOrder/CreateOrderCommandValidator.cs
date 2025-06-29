using FluentValidation;

namespace Recycler.API;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0)
            .WithMessage("Specify a Supplier ID.");
        RuleFor(x => x.OrderItems.Length).GreaterThan(0)
            .WithMessage("Need at least one order item.");
    }
}