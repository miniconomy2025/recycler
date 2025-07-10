using MediatR;
using RecyclerApi.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace RecyclerApi.Handlers
{
    public class GetLogisticsRecordsQueryHandler : IRequestHandler<GetLogisticsRecordsQuery, List<LogisticsRecordDto>>
    {
        private readonly IConfiguration _configuration;

        public GetLogisticsRecordsQueryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<LogisticsRecordDto>> Handle(GetLogisticsRecordsQuery request, CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT
                    id,
                    internal_record_id as InternalRecordId,
                    external_id as ExternalId,
                    type,
                    timestamp,
                    status,
                    items::text as ItemsJson
                FROM Logistics
                ORDER BY timestamp DESC;";

            var dbRecords = await connection.QueryAsync<dynamic>(sql);

            var logisticsRecords = new List<LogisticsRecordDto>();
            foreach (var record in dbRecords)
            {
                var items = new List<LogisticsItemDto>();
                if (record.ItemsJson != null)
                {
                    try
                    {
                        items = JsonSerializer.Deserialize<List<LogisticsItemDto>>(record.ItemsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error deserializing items for logistics record {record.id}: {ex.Message}");
                    }
                }

                logisticsRecords.Add(new LogisticsRecordDto
                {
                    Id = record.id,
                    InternalRecordId = record.InternalRecordId,
                    ExternalId = record.ExternalId,
                    Type = record.type,
                    Timestamp = record.timestamp,
                    Status = record.status,
                    Items = items
                });
            }

            return logisticsRecords;
        }
    }
}