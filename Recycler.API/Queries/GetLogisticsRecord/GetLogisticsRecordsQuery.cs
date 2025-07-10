using MediatR;
using RecyclerApi.Models;
using System.Collections.Generic;

namespace RecyclerApi.Queries
{
    public class GetLogisticsRecordsQuery : IRequest<List<LogisticsRecordDto>>
    {
        
    }
}