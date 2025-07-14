using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Queries;

public class GetStockQuery : IRequest<StockSet> { }
