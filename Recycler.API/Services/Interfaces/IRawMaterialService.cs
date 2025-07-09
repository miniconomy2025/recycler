namespace Recycler.API.Services;

public interface IRawMaterialService : IGenericService<RawMaterial>
{
    public Task UpdateRawMaterialPrice(IEnumerable<RawMaterial> updateRawMaterials);

    public Task<IEnumerable<RawMaterialDto>> GetAvailableRawMaterialsAndQuantity();

} 