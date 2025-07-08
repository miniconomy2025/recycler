using MediatR;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq; 

namespace RecyclerApi.Handlers
{
    public class InternalLogisticsRecord
    {
        public string InternalRecordId { get; set; }
        public string ExternalId { get; set; } 
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
        public List<LogisticsItemDto> Items { get; set; }
        public string Status { get; set; } 
    }

    public class ProcessLogisticsCommandHandler : IRequestHandler<ProcessLogisticsCommand, LogisticsResponseDto>
    {
        private readonly IMediator _mediator;
        private static List<InternalLogisticsRecord> _simulatedLogisticsRecords = new List<InternalLogisticsRecord>();

        public ProcessLogisticsCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<LogisticsResponseDto> Handle(ProcessLogisticsCommand request, CancellationToken cancellationToken)
        {
        
            var newLogisticsRecord = new InternalLogisticsRecord
            {
                InternalRecordId = Guid.NewGuid().ToString(),
                ExternalId = request.Id,
                Type = request.Type,
                Timestamp = DateTime.UtcNow,
                Items = request.Items,
                Status = "Logged"
            };
            _simulatedLogisticsRecords.Add(newLogisticsRecord);

            string message = $"Logistics event '{request.Type}' with ID '{request.Id}' processed successfully.";

            if (request.Type == "DELIVERY")
            {
                if (request.Items != null && request.Items.Any())
                {
                    var receiveCommand = new ReceiveLogisticsItemsCommand { ItemsToReceive = request.Items };
                    await _mediator.Send(receiveCommand, cancellationToken); 

                   
                    message += " Inventory updated for delivered items (including phones).";
                }
                else
                {
                    message += " No items specified for delivery.";
                }
            }
            else if (request.Type == "PICKUP")
            {
                 message += " Pickup event logged.";
            }

            var response = new LogisticsResponseDto
            {
                Message = message,
                LogisticsRecordId = newLogisticsRecord.InternalRecordId
            };

            return Task.FromResult(response);
        }
    }
}