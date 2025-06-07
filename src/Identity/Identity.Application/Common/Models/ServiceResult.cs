namespace Identity.Application.Common.Models;

/// <summary>
/// Represents the result of a service operation
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ServiceResult Success() => new() { IsSuccess = true };
    
    public static ServiceResult Failure(string errorMessage) => new() 
    { 
        IsSuccess = false, 
        ErrorMessage = errorMessage,
        Errors = new List<string> { errorMessage }
    };
    
    public static ServiceResult Failure(List<string> errors) => new() 
    { 
        IsSuccess = false, 
        Errors = errors,
        ErrorMessage = errors.FirstOrDefault()
    };
}

/// <summary>
/// Represents the result of a service operation with data
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data) => new() 
    { 
        IsSuccess = true, 
        Data = data 
    };
    
    public static new ServiceResult<T> Failure(string errorMessage) => new() 
    { 
        IsSuccess = false, 
        ErrorMessage = errorMessage,
        Errors = new List<string> { errorMessage }
    };
    
    public static new ServiceResult<T> Failure(List<string> errors) => new() 
    { 
        IsSuccess = false, 
        Errors = errors,
        ErrorMessage = errors.FirstOrDefault()
    };
}
