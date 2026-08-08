namespace ArchUnitNet.Common.Error;

/// <summary>
/// Thrown when the user has misused the API.
/// This is a programming error in the caller's code — not a library bug.
/// </summary>
public class UserError : Exception
{
    public UserError(string message) : base(message)
    {
    }

    public UserError(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
