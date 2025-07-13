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
        var httpRequest = httpContext?.Request;
        
        await logRepository.CreateAsync(new Log()
        {
            RequestSource = httpContext?.GetRouteData()?.Values["controller"]?.ToString() ?? "",
            RequestEndpoint = httpRequest == null
                ? ""
                : $"{httpRequest.Scheme}://{httpRequest.Host}/{httpRequest.Path}{httpRequest.QueryString}",
            RequestBody = JsonSerializer.Serialize(request),
            Response = JsonSerializer.Serialize(response),
            Timestamp = DateTime.Now
        });
    }
}