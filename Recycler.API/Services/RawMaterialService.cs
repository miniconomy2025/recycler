namespace Recycler.API.Services;

public class RawMaterialService(
    IGenericRepository<RawMaterial> rawMaterialRepository,
    IGenericRepository<MaterialInventory> materialInventoryRepository) : GenericService<RawMaterial>(rawMaterialRepository), IRawMaterialService
{
    public async Task UpdateRawMaterialPrice(IEnumerable<RawMaterial> updateRawMaterials)
    {
        
        foreach (var updateRawMaterial in updateRawMaterials)
        {
            var rawMaterial = (await rawMaterialRepository.GetByColumnValueAsync("name", updateRawMaterial.Name))
                .LastOrDefault();

            if (rawMaterial is null)
            {
                await rawMaterialRepository.CreateAsync(new RawMaterial()
                {
                    Name = updateRawMaterial.Name,
                    PricePerKg = updateRawMaterial.PricePerKg
                });
            }
            else if (rawMaterial.PricePerKg != updateRawMaterial.PricePerKg)
            {
                rawMaterial.PricePerKg = updateRawMaterial.PricePerKg;
                
                await rawMaterialRepository.UpdateAsync(rawMaterial, new List<string>() {"PricePerKg"});
            }
        }
    }

    public async Task<IEnumerable<RawMaterialDto>> GetAvailableRawMaterialsAndQuantity()
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