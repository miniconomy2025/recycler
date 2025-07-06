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
        Task<RecyclingEligibilityResult> CheckRecyclingEligibilityAsync();
        Task<int> GetTotalPhoneInventoryAsync();
    }

    public class RecyclingService : IRecyclingService
    {
        private readonly IConfiguration _configuration;
        private const int MINIMUM_PHONE_THRESHOLD = 1000;

        public RecyclingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        public async Task<int> GetTotalPhoneInventoryAsync()
        {
            await using var connection = GetConnection();
            var sql = "SELECT COALESCE(SUM(quantity), 0) FROM phoneinventory";
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<RecyclingEligibilityResult> CheckRecyclingEligibilityAsync()
        {
            await using var connection = GetConnection();
            
            var sql = @"
                SELECT 
                    pi.phone_id as PhoneId,
                    p.model as Model,
                    pb.brand_name as BrandName,
                    pi.quantity as AvailableQuantity
                FROM phoneinventory pi
                JOIN phone p ON pi.phone_id = p.id
                JOIN phonebrand pb ON p.phone_brand_id = pb.id
                WHERE pi.quantity > 0";

            var phoneInventories = await connection.QueryAsync<PhoneInventoryDto>(sql);
            var totalPhones = phoneInventories.Sum(pi => pi.AvailableQuantity);

            var phoneInventoryDtos = new List<PhoneInventoryDto>();
            var totalEstimatedYield = new Dictionary<string, double>();

            foreach (var inventory in phoneInventories)
            {
                var estimate = await EstimateRecyclingYieldAsync(inventory.PhoneId, inventory.AvailableQuantity);
                inventory.EstimatedYield = estimate;
                phoneInventoryDtos.Add(inventory);

                // Aggregate total estimated yield
                foreach (var material in estimate.EstimatedMaterials)
                {
                    if (totalEstimatedYield.ContainsKey(material.Key))
                        totalEstimatedYield[material.Key] += material.Value;
                    else
                        totalEstimatedYield[material.Key] = material.Value;
                }
            }

            var isEligible = totalPhones >= MINIMUM_PHONE_THRESHOLD;
            var message = isEligible 
                ? $"Ready for recycling! {totalPhones} phones available (minimum {MINIMUM_PHONE_THRESHOLD} required). Will produce {totalEstimatedYield.Values.Sum():F2}kg of raw materials."
                : $"Not enough phones for recycling. Available: {totalPhones}, Required: {MINIMUM_PHONE_THRESHOLD}, Need: {MINIMUM_PHONE_THRESHOLD - totalPhones} more";

            return new RecyclingEligibilityResult
            {
                IsEligible = isEligible,
                TotalPhonesAvailable = totalPhones,
                MinimumRequired = MINIMUM_PHONE_THRESHOLD,
                Message = message,
                AvailablePhones = phoneInventoryDtos,
                TotalEstimatedYield = totalEstimatedYield
            };
        }

        public async Task<PhoneRecyclingEstimate> EstimateRecyclingYieldAsync(int phoneId, int quantity = 1)
        {
            await using var connection = GetConnection();

            // Get phone details
            var phoneDetailsSql = @"
                SELECT p.id as PhoneId, p.model as PhoneModel, pb.brand_name as BrandName
                FROM phone p
                JOIN phonebrand pb ON p.phone_brand_id = pb.id
                WHERE p.id = @PhoneId";

            var phoneDetails = await connection.QuerySingleOrDefaultAsync<dynamic>(phoneDetailsSql, new { PhoneId = phoneId });
            
            if (phoneDetails == null)
                throw new ArgumentException($"Phone with ID {phoneId} not found");

            // Get recycling ratios
            var ratiosSql = @"
                SELECT 
                    ppr.phone_part_quantity_per_phone as PartsPerPhone,
                    pmr.raw_material_quantity_per_phone_part as MaterialPerPart,
                    rm.name as MaterialName
                FROM phonetophonepartratio ppr
                JOIN phoneparttorawmaterialratio pmr ON ppr.phone_part_id = pmr.phone_part_id
                JOIN rawmaterial rm ON pmr.raw_material_id = rm.id
                WHERE ppr.phone_id = @PhoneId";

            var ratios = await connection.QueryAsync<dynamic>(ratiosSql, new { PhoneId = phoneId });

            var materialTotals = new Dictionary<string, double>();

            foreach (var ratio in ratios)
            {
                var materialName = (string)ratio.materialname;
                var partsPerPhone = (int)ratio.partsperphone;
                var materialPerPart = (int)ratio.materialperpart;

                var totalMaterialPerPhone = (partsPerPhone * materialPerPart);
                var totalForQuantity = totalMaterialPerPhone * quantity;

                if (materialTotals.ContainsKey(materialName))
                    materialTotals[materialName] += totalForQuantity;
                else
                    materialTotals[materialName] = totalForQuantity;
            }

            return new PhoneRecyclingEstimate
            {
                PhoneId = phoneId,
                PhoneModel = phoneDetails.phonemodel,
                BrandName = phoneDetails.brandname,
                EstimatedMaterials = materialTotals,
                TotalEstimatedQuantity = materialTotals.Values.Sum()
            };
        }

        public async Task<RecyclingResult> StartRecyclingAsync()
        {
     
            // Step 1: Check if we have enough phones for recycling
                var eligibilityCheck = await CheckRecyclingEligibilityAsync();
                if (!eligibilityCheck.IsEligible)
                {
                    return new RecyclingResult
                    {
                        Success = false,
                        Message = $"Recycling not allowed: {eligibilityCheck.Message}"
                    };
                }

            await using var connection = GetConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Step 2: Get all available phones and their quantities
                var phoneInventoriesSql = @"
                    SELECT 
                        pi.phone_id as PhoneId, 
                        pi.quantity,
                        p.model,
                        pb.brand_name
                    FROM phoneinventory pi
                    JOIN phone p ON pi.phone_id = p.id
                    JOIN phonebrand pb ON p.phone_brand_id = pb.id
                    WHERE pi.quantity > 0";

                var phoneInventories = await connection.QueryAsync<dynamic>(phoneInventoriesSql, transaction: transaction);

                if (!phoneInventories.Any())
                {
                    return new RecyclingResult
                    {
                        Success = false,
                        Message = "No phones available for recycling"
                    };
                }

                var totalPhonesCount = phoneInventories.Sum(pi => (int)pi.quantity);
                var allRecycledMaterials = new List<RecycledMaterialResult>();
                var processedPhoneModels = new List<string>();

                // Step 3: Process all phones through the recycling machine
                foreach (var phoneInventory in phoneInventories)
                {
                    var phoneId = (int)phoneInventory.phoneid;
                    var quantity = (int)phoneInventory.quantity;
                    var model = (string)phoneInventory.model;
                    var brandName = (string)phoneInventory.brand_name;

                    // Get recycling estimate for this phone type
                    var estimate = await EstimateRecyclingYieldAsync(phoneId, quantity);

                    // Update phone inventory (set to 0 - all phones recycled)
                    var updatePhoneInventorySql = @"
                        UPDATE phoneinventory 
                        SET quantity = 0 
                        WHERE phone_id = @PhoneId";

                    await connection.ExecuteAsync(updatePhoneInventorySql, new { PhoneId = phoneId }, transaction);

                    // Process materials from this phone type
                    foreach (var (materialName, estimatedQuantity) in estimate.EstimatedMaterials)
                    {
                        var quantityInKg = (int)Math.Floor(estimatedQuantity);

                        if (quantityInKg > 0) // Only process if we have meaningful quantity
                        {
                            // Get material ID
                            var materialIdSql = "SELECT id FROM rawmaterial WHERE name = @MaterialName";
                            var materialId = await connection.QuerySingleOrDefaultAsync<int?>(materialIdSql, new { MaterialName = materialName }, transaction);

                            if (materialId.HasValue)
                            {
                                // Check if material inventory exists
                                var existingInventorySql = "SELECT available_quantity_in_kg FROM materialinventory WHERE material_id = @MaterialId";
                                var existingQuantity = await connection.QuerySingleOrDefaultAsync<int?>(existingInventorySql, new { MaterialId = materialId }, transaction);

                                if (existingQuantity.HasValue)
                                {
                                    // Update existing inventory
                                    var updateMaterialSql = @"
                                        UPDATE materialinventory 
                                        SET available_quantity_in_kg = available_quantity_in_kg + @Quantity 
                                        WHERE material_id = @MaterialId";
                                    await connection.ExecuteAsync(updateMaterialSql, new { MaterialId = materialId, Quantity = quantityInKg }, transaction);
                                }
                                else
                                {
                                    // Create new inventory record
                                    var insertMaterialSql = @"
                                        INSERT INTO materialinventory (material_id, available_quantity_in_kg) 
                                        VALUES (@MaterialId, @Quantity)";
                                    await connection.ExecuteAsync(insertMaterialSql, new { MaterialId = materialId, Quantity = quantityInKg }, transaction);
                                }

                                // Find existing material in results or add new
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

                    processedPhoneModels.Add($"{quantity}x {brandName} {model}");
                }
                // Step 4: Commit transaction
                await transaction.CommitAsync();    

                return new RecyclingResult
                {
                    Success = true,
                    Message = $"Recycling process completed! Processed {totalPhonesCount} phones using recycling machine. Processed: {string.Join(", ", processedPhoneModels)}",
                    RecycledMaterials = allRecycledMaterials,
                    TotalMaterialsRecycled = allRecycledMaterials.Sum(m => m.QuantityInKg),
                    PhonesProcessed = totalPhonesCount,
                    ProcessingDate = DateTime.UtcNow,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new RecyclingResult
                {
                    Success = false,
                    Message = $"Failed to complete recycling process: {ex.Message}"
                };
            }
        }
    }
}