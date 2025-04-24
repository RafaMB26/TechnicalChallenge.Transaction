namespace Transaction.Domain.Result;

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
    }

    public bool IsSuccess 
    { 
        get 
        {
            return Error is not null && String.IsNullOrEmpty(Error.Message);
        } 
    }
    public Error Error { get; set; }
    public T Data { get; set; }
}
