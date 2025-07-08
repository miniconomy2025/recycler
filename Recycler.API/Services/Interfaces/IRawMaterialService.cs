using Recycler.API.Models.ExternalApiResponses;

namespace Recycler.API.Services;

public interface IRawMaterialService
{
    public Task UpdateRawMaterialPrice(IEnumerable<RawMaterial> updateRawMaterials);
} 