using System;
using System.Collections.Generic;
using System.Linq;

namespace MVP.Application.Interfaces;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Forbidden = 4,
    Conflict = 5,
    Unexpected = 99
}

public class ApplicationResult
{
    public bool Succeeded { get; protected set; }
    public string[] Errors { get; protected set; }
    public ErrorType ErrorType { get; protected set; }

    internal ApplicationResult(bool succeeded, IEnumerable<string> errors, ErrorType errorType)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        ErrorType = errorType;
    }

    public static ApplicationResult Success()
    {
        return new ApplicationResult(true, Array.Empty<string>(), ErrorType.None);
    }

    public static ApplicationResult Failure(IEnumerable<string> errors, ErrorType errorType = ErrorType.Validation)
    {
        return new ApplicationResult(false, errors, errorType);
    }
}

public class ApplicationResult<T> : ApplicationResult
{
    public T? Value { get; private set; }

    private ApplicationResult(bool succeeded, T? value, IEnumerable<string> errors, ErrorType errorType) 
        : base(succeeded, errors, errorType)
    {
        Value = value;
    }

    public static ApplicationResult<T> Success(T value)
    {
        return new ApplicationResult<T>(true, value, Array.Empty<string>(), ErrorType.None);
    }

    public static new ApplicationResult<T> Failure(IEnumerable<string> errors, ErrorType errorType = ErrorType.Validation)
    {
        return new ApplicationResult<T>(false, default, errors, errorType);
    }
}
