
using MediatR;
using Recycler.API.Models.MaterialInventory;

namespace Recycler.API.Queries. GetLogs;

public class GetLogsQuery : IRequest<IEnumerable<Log>>
{
    public int? MaxReceivedLogId { get; set; }
}