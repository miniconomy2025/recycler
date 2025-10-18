using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Recycler.API.Dto;
using Recycler.API.Models;

namespace Recycler.API.Services
{
   public interface IRecyclingService
    {
        Task<RecyclingResult> StartRecyclingAsync();
        Task<PhoneRecyclingEstimate> EstimateRecyclingYieldAsync(int phoneId, int quantity = 1);
    }

    public class RecyclingService : IRecyclingService
    {
        private readonly IConfiguration _configuration;
        private const int MACHINE_PRODUCTION_RATE = 20; 

        public RecyclingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
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
                    ppr.phone_part_quantity_per_phone as PartsPerPhone,
                    pmr.raw_material_quantity_per_phone_part as MaterialPerPart,
                    rm.name as MaterialName
                FROM phonetophonepartratio ppr
                JOIN phoneparttorawmaterialratio pmr ON ppr.phone_part_id = pmr.phone_part_id
                JOIN rawmaterial rm ON pmr.raw_material_id = rm.id
                WHERE ppr.phone_id = @PhoneId";

            var ratios = await connection.QueryAsync<PhonePartRatioDto>(ratiosSql, new { PhoneId = phoneId });

            var materialTotals = new Dictionary<string, double>();

            foreach (var ratio in ratios)
            {
                var materialName = ratio.MaterialName;
                var partsPerPhone = ratio.PartsPerPhone;
                var materialPerPart = ratio.MaterialPerPart;

                var totalMaterialPerPhone = partsPerPhone * materialPerPart;
                var totalForQuantity = totalMaterialPerPhone * quantity;

                if (materialTotals.ContainsKey(materialName))
                    materialTotals[materialName] += totalForQuantity;
                else
                    materialTotals[materialName] = totalForQuantity;
            }

            return new PhoneRecyclingEstimate
            {
                PhoneId = phoneId,
                PhoneModel = phoneDetails.Model,
                BrandName = phoneDetails.BrandName,
                EstimatedMaterials = materialTotals,
                TotalEstimatedQuantity = materialTotals.Values.Sum()
            };
        }

        public async Task<RecyclingResult> StartRecyclingAsync()
        {
            await using var connection = GetConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var totalMachinesSql = @"
                    SELECT COUNT(*) 
                    FROM Machines";

                var totalMachinesCount = await connection.QuerySingleAsync<int>(totalMachinesSql, transaction: transaction);

                if (totalMachinesCount == 0)
                {
                    return new RecyclingResult
                    {
                        Success = false,
                        Message = "No recycling machines available. Please acquire recycling machines to process phones.",
                        PhonesProcessed = 0,
                        TotalMaterialsRecycled = 0
                    };
                }

                var operationalMachinesSql = @"
                    SELECT COUNT(*) 
                    FROM Machines 
                    WHERE is_operational = true";

                var operationalMachinesCount = await connection.QuerySingleAsync<int>(operationalMachinesSql, transaction: transaction);

                if (operationalMachinesCount == 0)
                {
                    return new RecyclingResult
                    {
                        Success = false,
                        Message = $"You have {totalMachinesCount} recycling machine(s), but none are operational. Please repair your machines before recycling.",
                        PhonesProcessed = 0,
                        TotalMaterialsRecycled = 0
                    };
                }

                var maxProcessingCapacity = operationalMachinesCount * MACHINE_PRODUCTION_RATE;

                var phoneInventoriesSql = @"
                    SELECT 
                        pi.phone_id as PhoneId,
                        p.model as Model,
                        pb.brand_name as BrandName,
                        pi.quantity as AvailableQuantity
                    FROM phoneinventory pi
                    JOIN phone p ON pi.phone_id = p.id
                    JOIN phonebrand pb ON p.phone_brand_id = pb.id
                    WHERE pi.quantity > 0
                    ORDER BY pi.quantity DESC";

                var phoneInventories = await connection.QueryAsync<PhoneInventoryDto>(phoneInventoriesSql, transaction: transaction);

                if (!phoneInventories.Any())
                {
                    return new RecyclingResult
                    {
                        Success = false,
                        Message = $"No phones available for recycling. You have {operationalMachinesCount} operational machine(s) with capacity to process {maxProcessingCapacity} phones.",
                        PhonesProcessed = 0,
                        TotalMaterialsRecycled = 0
                    };
                }

                var totalAvailablePhones = phoneInventories.Sum(pi => pi.AvailableQuantity);
                var phonesToProcess = Math.Min(totalAvailablePhones, maxProcessingCapacity);
                var actualMachinesUsed = (int)Math.Ceiling((double)phonesToProcess / MACHINE_PRODUCTION_RATE);

                var allRecycledMaterials = new List<RecycledMaterialResult>();
                var processedPhoneModels = new List<string>();
                var totalProcessedCount = 0;

                foreach (var phoneInventory in phoneInventories)
                {
                    if (totalProcessedCount >= phonesToProcess)
                        break;

                    var phoneId = phoneInventory.PhoneId;
                    var availableQuantity = phoneInventory.AvailableQuantity;
                    var model = phoneInventory.Model;
                    var brandName = phoneInventory.BrandName;

                    var remainingCapacity = phonesToProcess - totalProcessedCount;
                    var quantityToProcess = Math.Min(availableQuantity, remainingCapacity);
                    var quantityRemaining = availableQuantity - quantityToProcess;

                    if (quantityToProcess > 0)
                    {
                        var estimate = await EstimateRecyclingYieldAsync(phoneId, quantityToProcess);

                        var updatePhoneInventorySql = @"
                            UPDATE phoneinventory 
                            SET quantity = @QuantityRemaining 
                            WHERE phone_id = @PhoneId";

                        await connection.ExecuteAsync(updatePhoneInventorySql,
                            new { PhoneId = phoneId, QuantityRemaining = quantityRemaining },
                            transaction);

                        foreach (var (materialName, estimatedQuantity) in estimate.EstimatedMaterials)
                        {
                            var quantityInKg = (int)Math.Floor(estimatedQuantity);

                            if (quantityInKg > 0)
                            {
                                var materialIdSql = "SELECT id FROM rawmaterial WHERE name = @MaterialName";
                                var materialId = await connection.QuerySingleOrDefaultAsync<int?>(materialIdSql, new { MaterialName = materialName }, transaction);

                                if (materialId.HasValue)
                                {
                                    var existingInventorySql = "SELECT available_quantity_in_kg FROM materialinventory WHERE material_id = @MaterialId";
                                    var existingQuantity = await connection.QuerySingleOrDefaultAsync<int?>(existingInventorySql, new { MaterialId = materialId }, transaction);

                                    if (existingQuantity.HasValue)
                                    {
                                        var updateMaterialSql = @"
                                            UPDATE materialinventory 
                                            SET available_quantity_in_kg = available_quantity_in_kg + @Quantity 
                                            WHERE material_id = @MaterialId";
                                        await connection.ExecuteAsync(updateMaterialSql, new { MaterialId = materialId, Quantity = quantityInKg }, transaction);
                                    }
                                    else
                                    {
                                        var insertMaterialSql = @"
                                            INSERT INTO materialinventory (material_id, available_quantity_in_kg) 
                                            VALUES (@MaterialId, @Quantity)";
                                        await connection.ExecuteAsync(insertMaterialSql, new { MaterialId = materialId, Quantity = quantityInKg }, transaction);
                                    }

                                    var existingMaterial = allRecycledMaterials.FirstOrDefault(rm => rm.MaterialId == materialId.Value);
                                    if (existingMaterial != null)
                                    {
                                        existingMaterial.QuantityInKg += quantityInKg;
                                    }
                                    else
                                    {
                                        allRecycledMaterials.Add(new RecycledMaterialResult
                                        {
                                            MaterialId = materialId.Value,
                                            MaterialName = materialName,
                                            QuantityInKg = quantityInKg,
                                            RecycledDate = DateTime.UtcNow,
                                            SourcePhoneModels = $"{brandName} {model}"
                                        });
                                    }
                                }
                            }
                        }

                        processedPhoneModels.Add($"{quantityToProcess}x {brandName} {model}");
                        totalProcessedCount += quantityToProcess;
                    }
                    
                }

                var leftoverPhones = totalAvailablePhones - totalProcessedCount;
                var successMessage = leftoverPhones > 0
                    ? $"Recycling process completed! Processed {totalProcessedCount} phones using {actualMachinesUsed} recycling machine(s). {leftoverPhones} phones remain in inventory due to machine capacity limits. Processed: {string.Join(", ", processedPhoneModels)}"
                    : $"Recycling process completed! Processed all {totalProcessedCount} phones using {actualMachinesUsed} recycling machine(s). Processed: {string.Join(", ", processedPhoneModels)}";

                var result = new RecyclingResult
                {
                    Success = true,
                    Message = successMessage,
                    RecycledMaterials = allRecycledMaterials,
                    TotalMaterialsRecycled = allRecycledMaterials.Sum(m => m.QuantityInKg),
                    PhonesProcessed = totalProcessedCount,
                    ProcessingDate = DateTime.UtcNow,
                };
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                if (transaction != null && transaction.Connection != null)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (InvalidOperationException)
                    {
                        // Transaction was already completed
                    }
                }
                return new RecyclingResult
                {
                    Success = false,
                    Message = $"Failed to complete recycling process: {ex.Message}"
                };
            }
        }
    }
}