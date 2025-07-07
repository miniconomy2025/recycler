namespace Recycler.API.Services;

public class RawMaterialService(IGenericRepository<RawMaterial> repository) : IRawMaterialService
{
    public async Task UpdateRawMaterialPrice(IEnumerable<RawMaterial> updateRawMaterials)
    {
        
        foreach (var updateRawMaterial in updateRawMaterials)
        {
            var rawMaterial = (await repository.GetByColumnValueAsync("name", updateRawMaterial.Name))
                .LastOrDefault();

            if (rawMaterial is null)
            {
                await repository.CreateAsync(new RawMaterial()
                {
                    Name = updateRawMaterial.Name,
                    PricePerKg = updateRawMaterial.PricePerKg
                });
            }
            else if (rawMaterial.PricePerKg != updateRawMaterial.PricePerKg)
            {
                rawMaterial.PricePerKg = updateRawMaterial.PricePerKg;
                
                await repository.UpdateAsync(rawMaterial, new List<string>() {"PricePerKg"});
            }
        }
    }
}