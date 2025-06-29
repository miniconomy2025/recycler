using FluentValidation;

namespace Recycler.API;

public class GetOrderByOrderNumberQueryValidator : AbstractValidator<GetOrderByOrderNumberQuery>
{
    public GetOrderByOrderNumberQueryValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Order Number is required");
    }
}