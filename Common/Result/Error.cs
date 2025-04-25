namespace Common.Result;

public class Error
{
    public Error(string message, int code)
    {
        Message = message;
        Code = code;
    }
    public string Message { get; }
    public int Code { get; }
}
