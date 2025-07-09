using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Queries.GetRevenueReport;

public class GetStockQuery : IRequest<StockSet> { }
