using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Commands
{
    public class CreateCompanyCommand : IRequest<CreateCompanyResponse>
    {
        public string? Name { get; set; }
        public int RoleId { get; set; } 
        public int? KeyId { get; set; } 
    }
}
