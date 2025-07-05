using MediatR;
using RecyclerApi.Commands;
using RecyclerApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclerApi.Handlers
{
    public class ReceiveMachineCommandHandler : IRequestHandler<ReceiveMachineCommand, ReceivedMachineDto>
    {
        private static List<ReceivedMachineDto> _simulatedReceivedMachines = new List<ReceivedMachineDto>();
        private static int _nextReceivedMachineId = 1;

        public Task<ReceivedMachineDto> Handle(ReceiveMachineCommand request, CancellationToken cancellationToken)
        {

            var newReceivedMachine = new ReceivedMachineDto
            {
                Id = Interlocked.Increment(ref _nextReceivedMachineId),
                MachineId = request.MachineId,
                ReceivedAt = DateTime.UtcNow,
                Status = "Received"
            };

            _simulatedReceivedMachines.Add(newReceivedMachine);

            return Task.FromResult(newReceivedMachine);
        }

        public static List<ReceivedMachineDto> GetSimulatedReceivedMachines()
        {
            return _simulatedReceivedMachines;
        }
    }
}