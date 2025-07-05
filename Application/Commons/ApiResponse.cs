namespace Application.Commons;
public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public ApiResponse(int code, string message, T? data = default)
    {
        Code = code;
        Message = message;
        Data = data;
    }
}

public class ErrorResponse
{
    public int Code { get; set; }
    public string Message { get; set; }

    public ErrorResponse(int code, string message)
    {
        Code = code;
        Message = message;
    }
}
