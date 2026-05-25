using FluentValidation.Results;

namespace Acme.SaaS.Application.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures.Select(f => f.ErrorMessage).ToList();
    }
}
