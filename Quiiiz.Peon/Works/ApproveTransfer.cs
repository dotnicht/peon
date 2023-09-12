using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works
{
    internal class ApproveTransfer : IRunnable
    {
        private readonly ILogger logger;
        private readonly IRepository<User> repository;
        private readonly IOptions<Blockchain> options;

        public ApproveTransfer(ILogger<ApproveTransfer> logger, IRepository<User> repository, IOptions<Blockchain> options)
        {
            this.logger = logger;
            this.repository = repository;
            this.options = options;
        }

        public Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            throw new NotImplementedException(); 
        }
    }
}
