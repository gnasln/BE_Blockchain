namespace NetHelper.Common.Models;


public enum StatusCode
{
    FORBIDDEN = 403,
    CONFLICT = 409,
    BADREQUEST = 400,
    NOTFOUND = 404,
    UNAUTHORIZED = 401,
    INTERNALSERVERERROR = 500,
    CREATED = 201,
    OK = 200,
}

public class ResultCustom<T>
{
    public StatusCode Status { set; get; }
    public string[]? Message { set; get; }
    public T? Data { set; get; }
}
