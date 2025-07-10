namespace Recycler.API.Services;

public interface ILogService : IGenericService<Log>
{
    public Task<IEnumerable<Log>> GetLogs(int? maxReceivedLogId);

    public Task CreateLog(HttpContext httpContext, Object request, Object response);
}