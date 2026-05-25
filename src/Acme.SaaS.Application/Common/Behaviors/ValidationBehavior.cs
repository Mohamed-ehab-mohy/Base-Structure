using FluentValidation;

namespace Acme.SaaS.Application.Common.Behaviors;

public class ValidationBehavior<T>
{
    private readonly IEnumerable<IValidator<T>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<T>> validators)
    {
        _validators = validators;
    }

    public async Task ValidateAsync(T request, CancellationToken ct = default)
    {
        if (!_validators.Any()) return;

        var context = new ValidationContext<T>(request);
        var failures = (await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new Exceptions.ValidationException(failures);
    }
}
