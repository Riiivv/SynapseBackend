namespace Synapse.Application.Common;

public enum ServiceStatus
{
    Success,
    NotFound,
    Forbidden,
    BadRequest,
    Unauthorized
}

public class ServiceResult<T>
{
    public ServiceStatus Status { get; set; }

    public T? Data { get; set; }

    public string? Error { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            Status = ServiceStatus.Success,
            Data = data
        };
    }

    public static ServiceResult<T> NotFound(string error)
    {
        return new ServiceResult<T>
        {
            Status = ServiceStatus.NotFound,
            Error = error
        };
    }

    public static ServiceResult<T> Forbidden()
    {
        return new ServiceResult<T>
        {
            Status = ServiceStatus.Forbidden
        };
    }

    public static ServiceResult<T> BadRequest(string error)
    {
        return new ServiceResult<T>
        {
            Status = ServiceStatus.BadRequest,
            Error = error
        };
    }

    public static ServiceResult<T> Unauthorized(string error)
    {
        return new ServiceResult<T>
        {
            Status = ServiceStatus.Unauthorized,
            Error = error
        };
    }
}