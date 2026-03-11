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
    public bool IsSuccess { get; protected set; }
    public string[] Errors { get; protected set; }
    public ErrorType ErrorType { get; protected set; }

    public string? ErrorMessage => Errors?.FirstOrDefault();

    internal ApplicationResult(bool succeeded, IEnumerable<string> errors, ErrorType errorType)
    {
        IsSuccess = succeeded;
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

    public static ApplicationResult Failure(string error, ErrorType errorType = ErrorType.Validation)
    {
        return new ApplicationResult(false, new[] { error }, errorType);
    }
}

public class ApplicationResult<T> : ApplicationResult
{
    public T? Data { get; private set; }

    private ApplicationResult(bool succeeded, T? value, IEnumerable<string> errors, ErrorType errorType) 
        : base(succeeded, errors, errorType)
    {
        Data = value;
    }

    public static ApplicationResult<T> Success(T value)
    {
        return new ApplicationResult<T>(true, value, Array.Empty<string>(), ErrorType.None);
    }

    public static new ApplicationResult<T> Failure(IEnumerable<string> errors, ErrorType errorType = ErrorType.Validation)
    {
        return new ApplicationResult<T>(false, default, errors, errorType);
    }

    public static new ApplicationResult<T> Failure(string error, ErrorType errorType = ErrorType.Validation)
    {
        return new ApplicationResult<T>(false, default, new[] { error }, errorType);
    }
}
