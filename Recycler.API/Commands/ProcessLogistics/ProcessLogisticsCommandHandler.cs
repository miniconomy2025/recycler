using MediatR;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;
using System;
using System.Text.Json;

namespace RecyclerApi.Handlers
{
    public class ProcessLogisticsCommandHandler : IRequestHandler<ProcessLogisticsCommand, LogisticsResponseDto>
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration; 

        public ProcessLogisticsCommandHandler(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        public async Task<LogisticsResponseDto> Handle(ProcessLogisticsCommand request, CancellationToken cancellationToken)
        {

            var newInternalRecordId = Guid.NewGuid(); 

            string message = $"Logistics event '{request.Type}' with ID '{request.Id}' processed successfully.";

            if (request.Type == "DELIVERY")
            {
                if (request.Items != null && request.Items.Any())
                {
                    var receiveCommand = new ReceiveLogisticsItemsCommand { ItemsToReceive = request.Items };
                    await _mediator.Send(receiveCommand, cancellationToken);

                    message += " Inventory updated for delivered items.";
                }
                else
                {
                    message += " No items specified for delivery.";
                }
            }
            else if (request.Type == "PICKUP")
            {
                 message += " Pickup event logged. Inventory adjustment logic for pickup would go here (e.g., decrementing inventory).";
            }

            var response = new LogisticsResponseDto
            {
                Message = message,
                LogisticsRecordId = newInternalRecordId.ToString()
            };

            return Task.FromResult(response);
        }
    }
}
