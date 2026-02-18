using FluentValidation.Results;

public class SyncMessageValidator :
    AbstractValidator<MessageWithValidator>
{
    public SyncMessageValidator() =>
        RuleFor(_ => _.Content)
            .NotEmpty();

    public override ValidationResult Validate(ValidationContext<MessageWithValidator> context)
    {
        ArgumentNullException.ThrowIfNull(context.Headers());
        ArgumentNullException.ThrowIfNull(context.ContextBag());
        return base.Validate(context);
    }
}
