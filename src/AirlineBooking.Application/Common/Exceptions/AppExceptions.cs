namespace AirlineBooking.Application.Common.Exceptions;

/// <summary>
/// A handful of app-level exception types that the Api's global exception-handling middleware
/// maps to specific HTTP status codes (see ExceptionHandlingMiddleware). Services throw these
/// instead of returning nullable/Result-wrapper types - keeps the "happy path" return type of
/// every service method clean, and keeps the HTTP-status decision in one place (the
/// middleware) instead of scattered across every controller action.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.")
    {
    }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

public class ValidationAppException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
