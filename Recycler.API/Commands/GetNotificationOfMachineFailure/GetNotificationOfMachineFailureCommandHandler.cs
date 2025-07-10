using MediatR;
using Recycler.API;
using Recycler.API.Models;
using RecyclerApi.Commands;

namespace RecyclerApi.Handlers
{
    public class GetNotificationOfMachineFailureCommandHandler(IGenericRepository<ReceivedMachineDto> machinesRepository) : IRequestHandler<GetNotificationOfMachineFailureCommand>
    {
        public async Task Handle(GetNotificationOfMachineFailureCommand request, CancellationToken cancellationToken)
        {
            if (request.MachineName == "recycling_machine")
            {
                var firstOperationalMachine =  (await machinesRepository.GetAllAsync()).FirstOrDefault(machine => machine.IsOperational);

                if (firstOperationalMachine != null)
                {
                    firstOperationalMachine.IsOperational = false;
                    
                    await machinesRepository.UpdateAsync(firstOperationalMachine, ["is_operational"]);
                }
            }
        }
    }
}