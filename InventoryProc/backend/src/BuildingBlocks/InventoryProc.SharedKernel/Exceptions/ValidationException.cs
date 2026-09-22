namespace InventoryProc.SharedKernel.Exceptions;

public class ValidationException : Exception
{
    public List<ValidationError> Errors { get; }

    public ValidationException(List<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : base("Validation error occurred.")
    {
        Errors = new List<ValidationError>
        {
            new ValidationError(field, message)
        };
    }
}

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }

    public ValidationError(string field, string message)
    {
        Field = field;
        Message = message;
    }
}
