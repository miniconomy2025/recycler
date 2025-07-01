using MediatR;
using RecyclerApi.Models;
using RecyclerApi.Commands;

namespace RecyclerApi.Handlers
{
    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CreateCompanyResponse>
    {
        private static int _nextCompanyId = 1; 

        public Task<CreateCompanyResponse> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            string assignedRoleName = "Unknown";
            switch (request.KeyId)
            {
                case 1:
                    assignedRole = "Recycler";
                    break;
                case 2:
                    assignedRole = "Supplier";
                    break;
                case 3:
                    assignedRole = "Logistics";
                    break;
                case 4:
                    assignedRole = "Bank";
                    break;
                default:
                    assignedRole = "General";
                    break;
            }
            var newCompanyId = Interlocked.Increment(ref _nextCompanyId);
            var newCompanyNumber = Guid.NewGuid(); 

            var response = new CreateCompanyResponse
            {
                CompanyId = newCompanyId,
                CompanyNumber = newCompanyNumber,
                Name = request.Name,
                Role = assignedRoleName 
            };

            return Task.FromResult(response);
        }
    }
}