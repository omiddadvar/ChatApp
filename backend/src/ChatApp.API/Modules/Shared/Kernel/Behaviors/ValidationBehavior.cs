namespace ChatApp.API.Modules.Shared.Kernel.Behaviors;

using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next().ConfigureAwait(false);

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next().ConfigureAwait(false);

        var errorDictionary = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray());

        if (TryBuildFailureResult<TResponse>(errorDictionary, out var failureResult))
            return failureResult!;


        throw new Exceptions.ValidationException(errorDictionary);
    }

    private static bool TryBuildFailureResult<TResponse>(
        IDictionary<string, string[]> errors,
        out TResponse? result)
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var error = new Error(
                "Validation.Failed",
                string.Join("; ", errors.SelectMany(kv => kv.Value)),
                ErrorType.Validation);

            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) })!
                .MakeGenericMethod(responseType.GetGenericArguments()[0]);

            result = (TResponse?)failureMethod.Invoke(null, new object[] { error });
            return true;
        }

        if (responseType == typeof(Result))
        {
            var error = new Error(
                "Validation.Failed",
                string.Join("; ", errors.SelectMany(kv => kv.Value)),
                ErrorType.Validation);

            result = (TResponse?)(object)Result.Failure(error);
            return true;
        }

        result = default;
        return false;
    }
}