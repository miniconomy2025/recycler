using MediatR;

namespace Recycler.API.Queries.GetMaterials;

public class GetMaterialsQuery : IRequest<List<RawMaterialDto>> {}
