namespace Common.Result;

public class Result<T>
{
    public Result(T data)
    {
        Data = data;
        Error = null;
    }
    public Result(Error error)
    {
        Error = error;
        Data = default(T);
    }

    public bool IsSuccess
    {
        get
        {
            return Error is null;
        }
    }
    public Error Error { get; set; }
    public T Data { get; set; }
}