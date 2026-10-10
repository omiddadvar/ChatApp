namespace ChatApp.API.Modules.Shared.Kernel.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    public ValidationException() : base("Validation failures occurred.")
        => Errors = new Dictionary<string, string[]>();
    public ValidationException(IDictionary<string, string[]> errors) : this() => Errors = errors;
}
