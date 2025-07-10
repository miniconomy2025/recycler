using System.Text.Json;

namespace Recycler.API.Services;

public class LogService(IGenericRepository<Log> logRepository) : GenericService<Log>(logRepository), ILogService
{
    public async Task<IEnumerable<Log>> GetLogs(int? maxReceivedLogId)
    {
        if (maxReceivedLogId.HasValue)
        {
            return await logRepository.GetByWhereClauseAsync("id >", maxReceivedLogId.Value);
        }

        return await logRepository.GetAllAsync();
    }
    
    public async Task CreateLog(HttpContext? httpContext, Object request, Object response)
    {
        await logRepository.CreateAsync(new Log()
        {
            RequestSource = httpContext == null
                ? ""
                : $"{httpContext.Request.Headers["Referer"].ToString()}",
            RequestEndpoint = httpContext == null
                ? ""
                : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.Path}{httpContext.Request.QueryString}",
            RequestBody = JsonSerializer.Serialize(request),
            Response = JsonSerializer.Serialize(response),
            Timestamp = DateTime.Now
        });
    }
}