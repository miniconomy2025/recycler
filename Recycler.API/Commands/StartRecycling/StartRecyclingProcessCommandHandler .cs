using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Models;
using Recycler.API.Services;

namespace Recycler.API.Commands.StartRecycling
{
      public class StartRecyclingCommandHandler : IRequestHandler<StartRecyclingCommand, RecyclingResult>
    {
        private readonly IRecyclingService _recyclingService;

        public StartRecyclingCommandHandler(IRecyclingService recyclingService)
        {
            _recyclingService = recyclingService;
        }

        public async Task<RecyclingResult> Handle(StartRecyclingCommand request, CancellationToken cancellationToken)
        {
            return await _recyclingService.StartRecyclingAsync();
        }
    }
}