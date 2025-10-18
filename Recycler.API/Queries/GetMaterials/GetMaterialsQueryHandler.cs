using MediatR;
using Recycler.API.Services;

namespace Recycler.API.Queries.GetMaterials;

public class GetMaterialsQueryHandler(IRawMaterialService rawMaterialService) : IRequestHandler<GetMaterialsQuery, IEnumerable<RawMaterialDto>>
{
    public async Task<IEnumerable<RawMaterialDto>> Handle(GetMaterialsQuery request, CancellationToken cancellationToken)
    {
        return await rawMaterialService.GetAvailableRawMaterialsAndQuantity();
    }
}
