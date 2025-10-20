using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Services
{
    public interface ISimulationBootstrapService
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}