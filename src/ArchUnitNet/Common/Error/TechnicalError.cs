namespace ArchUnitNet.Common.Error;

/// <summary>
/// Thrown when the library or runtime environment encounters an error.
/// This is a library bug, configuration problem, or environmental issue — not a user error.
/// </summary>
public class TechnicalError : Exception
{
    public TechnicalError(string message) : base(message)
    {
    }

    public TechnicalError(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
