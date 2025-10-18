using MediatR;
using Recycler.API.Services;

namespace Recycler.API.Queries.GetLogs;

public class GetLogsQueryHandler(
    ILogService logService) : IRequestHandler<GetLogsQuery, IEnumerable<Log>>
{
    public async Task<IEnumerable<Log>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
    {
        return await logService.GetLogs(request.MaxReceivedLogId);
    }
}