using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Recycler.API.Dto;
using Recycler.API.Models;
using Recycler.API.Services;

namespace Recycler.Tests.Infrastructure;

public class TestRecyclingService : IRecyclingService
{
    private readonly string _connectionString;
    private const int MACHINE_PRODUCTION_RATE = 20; 

    public TestRecyclingService(string connectionString)
    {
        _connectionString = connectionString;
    }

    private NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public async Task<PhoneRecyclingEstimate> EstimateRecyclingYieldAsync(int phoneId, int quantity = 1)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();

         var phoneDetailsSql = @"
            SELECT 
                p.id as PhoneId,
                p.model as Model,
                pb.brand_name as BrandName,
                COALESCE(pi.quantity, 0) as AvailableQuantity
            FROM phone p
            JOIN phonebrand pb ON p.phone_brand_id = pb.id
            LEFT JOIN phoneinventory pi ON p.id = pi.phone_id
            WHERE p.id = @PhoneId";

        var phoneDetails = await connection.QuerySingleOrDefaultAsync<PhoneInventoryDto>(phoneDetailsSql, new { PhoneId = phoneId });
        
        if (phoneDetails == null)
            throw new ArgumentException($"Phone with ID {phoneId} not found");


        var ratiosSql = @"
                SELECT 
                    pp.name as PartName,
                    pptr.phone_part_quantity_per_phone as PartsPerPhone,
                    rm.name as MaterialName,
                    ptrmr.raw_material_quantity_per_phone_part as MaterialPerPart
                FROM phonetophonepartratio pptr
                JOIN phoneparts pp ON pptr.phone_part_id = pp.id
                JOIN phoneparttorawmaterialratio ptrmr ON pp.id = ptrmr.phone_part_id
                JOIN rawmaterial rm ON ptrmr.raw_material_id = rm.id
                WHERE pptr.phone_id = @PhoneId";

        var ratios = await connection.QueryAsync<PhonePartRatioDto>(ratiosSql, new { PhoneId = phoneId });

        var estimate = new PhoneRecyclingEstimate
        {
            PhoneId = phoneDetails.PhoneId,
            PhoneModel = phoneDetails.Model ?? string.Empty,
            BrandName = phoneDetails.BrandName ?? string.Empty,
            EstimatedMaterials = ratios.ToDictionary(r => r.MaterialName, r => (double)(r.PartsPerPhone * r.MaterialPerPart * quantity)),
            TotalEstimatedQuantity = ratios.Sum(r => r.PartsPerPhone * r.MaterialPerPart * quantity)
        };

        return estimate;
    }

    public async Task<RecyclingResult> StartRecyclingAsync()
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();

        var result = new RecyclingResult
        {
            Success = true,
            RecycledMaterials = new List<RecycledMaterialResult>(),
            Message = "Recycling process completed successfully",
            ProcessingDate = DateTime.UtcNow,
            PhonesProcessed = 0
        };

        var availablePhonesSql = @"
            SELECT 
                p.id as PhoneId,
                p.model as Model,
                pb.brand_name as BrandName,
                pi.quantity as AvailableQuantity
            FROM phone p
            JOIN phonebrand pb ON p.phone_brand_id = pb.id
            JOIN phoneinventory pi ON p.id = pi.phone_id
            WHERE pi.quantity > 0
            ORDER BY pi.quantity DESC
            LIMIT @Limit";

        var availablePhones = await connection.QueryAsync<PhoneInventoryDto>(availablePhonesSql, new { Limit = MACHINE_PRODUCTION_RATE });

        if (!availablePhones.Any())
        {
            result.Success = false;
            result.Message = "No phones available for recycling";
            return result;
        }

        var operationalMachinesSql = "SELECT COUNT(*) FROM machines WHERE is_operational = true";
        var operationalMachineCount = await connection.QuerySingleAsync<int>(operationalMachinesSql);

        if (operationalMachineCount == 0)
        {
            result.Success = false;
            result.Message = "No operational machines available";
            return result;
        }

        var totalProcessingCapacity = operationalMachineCount * MACHINE_PRODUCTION_RATE;
        var phonesToProcess = availablePhones.Take(totalProcessingCapacity).ToList();

        foreach (var phone in phonesToProcess)
        {
            var estimate = await EstimateRecyclingYieldAsync(phone.PhoneId, phone.AvailableQuantity);
            
            var updatePhoneInventorySql = @"
                UPDATE phoneinventory 
                SET quantity = quantity - @Quantity 
                WHERE phone_id = @PhoneId";
            
            await connection.ExecuteAsync(updatePhoneInventorySql, new { 
                PhoneId = phone.PhoneId, 
                Quantity = phone.AvailableQuantity 
            });

            foreach (var material in estimate.EstimatedMaterials)
            {
                var updateMaterialInventorySql = @"
                    INSERT INTO materialinventory (material_id, available_quantity_in_kg)
                    VALUES ((SELECT id FROM rawmaterial WHERE name = @MaterialName), @Quantity)
                    ON CONFLICT (material_id) 
                    DO UPDATE SET available_quantity_in_kg = materialinventory.available_quantity_in_kg + @Quantity";

                await connection.ExecuteAsync(updateMaterialInventorySql, new { 
                    MaterialName = material.Key, 
                    Quantity = material.Value 
                });

                result.RecycledMaterials.Add(new RecycledMaterialResult
                {
                    MaterialName = material.Key,
                    QuantityInKg = material.Value,
                    RecycledDate = DateTime.UtcNow,
                    SourcePhoneModels = phone.Model ?? string.Empty
                });
            }
            
            result.PhonesProcessed += phone.AvailableQuantity;
        }

        result.Message = $"Successfully processed {phonesToProcess.Count} phones";
        return result;
    }
}
