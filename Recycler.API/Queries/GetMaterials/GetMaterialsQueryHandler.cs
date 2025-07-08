using MediatR;

namespace Recycler.API.Queries.GetMaterials;

public class GetMaterialsQueryHandler(
    IGenericRepository<RawMaterial> rawMaterialRepository,
    IGenericRepository<MaterialInventory> materialInventoryRepository) : IRequestHandler<GetMaterialsQuery, List<RawMaterialDto>>
{
    public async Task<List<RawMaterialDto>> Handle(GetMaterialsQuery request, CancellationToken cancellationToken)
    {
        var rawMaterials = await rawMaterialRepository.GetAllAsync();
        
        var rawMaterialDtos = new List<RawMaterialDto>();

        foreach (var rawMaterial in rawMaterials)
        {
            var materialInventory = await materialInventoryRepository.GetByIdAsync(rawMaterial.Id);
            
            rawMaterialDtos.Add(new RawMaterialDto()
            {
                Name = rawMaterial.Name,
                AvailableQuantityInKg = materialInventory?.AvailableQuantityInKg ?? 0,
                PricePerKg = rawMaterial.PricePerKg
            });
        }

        return rawMaterialDtos;
    }
}
