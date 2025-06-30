using MediatR;

namespace Recycler.API.Queries.GetMaterials;

public class GetMaterialsQueryHandler : IRequestHandler<GetMaterialsQuery, List<RawMaterialDto>>
{
    public async Task<List<RawMaterialDto>> Handle(GetMaterialsQuery request, CancellationToken cancellationToken)
    {
        return new List<RawMaterialDto>
        {
            new() { Id = 1, Name = "Aluminum", AvailableQuantityInKg = 120.5f, Price = 25.5m },
            new() { Id = 2, Name = "Copper", AvailableQuantityInKg = 50.0f, Price = 40.0m }
        };
    }
}
