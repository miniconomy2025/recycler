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
                    Price = updateRawMaterial.Price
                });
            }
            else if (rawMaterial.Price != updateRawMaterial.Price)
            {
                rawMaterial.Price = updateRawMaterial.Price;
                
                await repository.UpdateAsync(rawMaterial, new List<string>() {"Price"});
            }
        }
    }
}