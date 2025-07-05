using MediatR;

namespace Recycler.API.Queries.GetRevenueReport;

public class GetRevenueReportQuery : IRequest<List<RevenueReportDto>> { }
