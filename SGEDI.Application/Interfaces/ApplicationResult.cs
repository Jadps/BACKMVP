namespace SGEDI.Application.Interfaces;

public class ApplicationResult
{
    public bool Succeeded { get; private set; }
    public string[] Errors { get; private set; }

    internal ApplicationResult(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public static ApplicationResult Success()
    {
        return new ApplicationResult(true, Array.Empty<string>());
    }

    public static ApplicationResult Failure(IEnumerable<string> errors)
    {
        return new ApplicationResult(false, errors);
    }
}
