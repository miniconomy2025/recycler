using System.Text.Json;
using Recycler.API.Models.ExternalApiResponses;
using Recycler.API.Models.Thoh;

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
    
    public async Task<List<RecycledPhoneModelDto>> GetAvailableRecycledPhonesAsync()
    {
        try
        {
            var response = await httpClient.GetAsync("recycled-phones");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                var phones = JsonSerializer.Deserialize<List<RecycledPhoneModelDto>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return phones ?? new List<RecycledPhoneModelDto>();
            }
        }
        catch (HttpRequestException)
        {
        }
        catch (Exception)
        {
        }

        return new List<RecycledPhoneModelDto>();
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
                catch (Exception)
                {
                    // ToDo: a way to log errors
                }
            }
        }
        catch (HttpRequestException)
        {
            // ToDo: a way to log errors - "THOH Service is not available"
        }
        catch (Exception)
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