
namespace NetHelper.Common.Models;

public class ResultCustomPaginate<T>
{
    public StatusCode Status { set; get; }
    public string[]? Message { set; get; }
    public T? Data { set; get; }
    public int? TotalItems { get; set; }
    public int? TotalPages { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}