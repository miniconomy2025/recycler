using MediatR;
using Recycler.API.Models.MaterialInventory;

namespace Recycler.API.Queries.GetMaterialInventory;

public class GetMaterialInventoryQueryHandler(
    IGenericRepository<RawMaterial> rawMaterialRepository,
    IGenericRepository<MaterialInventory> materialInventoryRepository) : IRequestHandler<GetMaterialInventoryQuery, List<MaterialInventoryDto>>
{
    public async Task<List<MaterialInventoryDto>> Handle(GetMaterialInventoryQuery request, CancellationToken cancellationToken)
    {
        var rawMaterials = await rawMaterialRepository.GetAllAsync();

        var materialInventoryDtos = new List<MaterialInventoryDto>();

        foreach (var rawMaterial in rawMaterials)
        {
            var materialInventory = await materialInventoryRepository.GetByIdAsync(rawMaterial.Id);

            materialInventoryDtos.Add(new MaterialInventoryDto()
            {
                MaterialName = rawMaterial.Name,
                AvailableQuantityInKg = materialInventory?.AvailableQuantityInKg ?? 0
            });
        }

        return materialInventoryDtos;
    }
}