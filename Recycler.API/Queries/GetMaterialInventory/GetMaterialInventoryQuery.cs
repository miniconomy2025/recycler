
using MediatR;
using Recycler.API.Models.MaterialInventory;

namespace Recycler.API.Queries.GetMaterialInventory;
public class GetMaterialInventoryQuery : IRequest<List<MaterialInventoryDto>> { }