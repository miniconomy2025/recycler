using MediatR;
using Recycler.API.Models;
using Recycler.API.Commands;

namespace Recycler.API.Handlers
{
    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CreateCompanyResponse>
    {
        private static int _nextCompanyId = 1; 

        public Task<CreateCompanyResponse> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            string assignedRoleName;
            switch (request.KeyId)
            {
                case 1:
                    assignedRoleName = "Recycler";
                    break;
                case 2:
                    assignedRoleName = "Supplier";
                    break;
                case 3:
                    assignedRoleName = "Logistics";
                    break;
                case 4:
                    assignedRoleName = "Bank";
                    break;
                default:
                    assignedRoleName = "General";
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