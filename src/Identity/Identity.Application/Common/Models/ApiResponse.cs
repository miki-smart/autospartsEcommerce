namespace Identity.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    
    public ApiResponse()
    {
        Success = true;
        Errors = new List<string>();
    }
    
    public ApiResponse(T data) : this()
    {
        Data = data;
    }
    
    public ApiResponse(string message) : this()
    {
        Message = message;
    }
    
    public ApiResponse(List<string> errors)
    {
        Success = false;
        Errors = errors;
    }
    
    public static ApiResponse<T> CreateError(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = new List<string> { error }
        };
    }
}
