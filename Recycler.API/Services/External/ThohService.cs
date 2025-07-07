using System.Text.Json;
using Recycler.API.Models.ExternalApiResponses;

namespace Recycler.API.Services;

public class ThohService(IHttpClientFactory httpClientFactory, IRawMaterialService rawMaterialService)
{
    private HttpClient httpClient { get; } = httpClientFactory.CreateClient(nameof(ThohService));
        
    public async Task GetAndUpdateRawMaterialPrice()
    {
        var thohRawMaterialList = await GetRawMaterialPriceFromThoh();
        
        if (thohRawMaterialList is not null)
        {
            
            var updateRawMaterials = ConvertToUpdateRawMaterial(thohRawMaterialList.Items, 20);
            
            await rawMaterialService.UpdateRawMaterialPrice(updateRawMaterials);
            
        }
    }

    private async Task<ThohRawMaterialListResponseDto?> GetRawMaterialPriceFromThoh()
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync("simulation/raw-materials");
            response.EnsureSuccessStatusCode();
            
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseBody))
            {
                try
                {
                    var thohRawMaterialList = JsonSerializer.Deserialize<ThohRawMaterialListResponseDto>(
                        responseBody, 
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    
                    return thohRawMaterialList;
                    
                }
                catch (Exception ex)
                {
                    // ToDo: a way to log errors
                }
            }
        }
        catch (HttpRequestException httpRequestException)
        {
            // ToDo: a way to log errors - "THOH Service is not available"
        }
        catch (Exception ex)
        {
            // ToDo: a way to log errors
        }
        
        return null;
    }

    private IEnumerable<RawMaterial> ConvertToUpdateRawMaterial(
        IEnumerable<ThohRawMaterialResponseDto> thohRawMaterials, decimal discountPerc)
    {
        var discountMultiplier = (100 - discountPerc) / 100;

        var updateRawMaterials = new List<RawMaterial>();

        foreach (var thohRawMaterial in thohRawMaterials)
        {
            updateRawMaterials.Add(new RawMaterial()
            {
                Name = thohRawMaterial.RawMaterialName,
                PricePerKg = thohRawMaterial.PricePerKg * discountMultiplier
            });
        }
        
        return updateRawMaterials;
    }

}