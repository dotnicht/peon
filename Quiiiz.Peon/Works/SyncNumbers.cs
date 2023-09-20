using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works;

internal class SyncNumbers : IRunnable
{
    private readonly IRepository<User> repository;
    private readonly IOptions<Blockchain> blockchain;
    private readonly IOptions<Configuration> options;
    private readonly ILogger<SyncNumbers> logger;

    public SyncNumbers(IRepository<User> repository, IOptions<Blockchain> blockchain, ILogger<SyncNumbers> logger, IOptions<Configuration> options)
    {
        this.repository = repository;
        this.blockchain = blockchain;
        this.logger = logger;
        this.options = options;
    }

    public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
    {
        var web3 = blockchain.Value.CreateMaster();

        foreach (var user in repository.Content)
        {
            var gas = options.Value.Gas
                ? await web3.Eth.GetBalance.SendRequestAsync(user.Address)
                : user.Gas;

            var approved = options.Value.Approved
                ? await web3.Eth.ERC20
                    .GetContractService(blockchain.Value.TokenAddress)
                    .AllowanceQueryAsync(new AllowanceFunction
                    {
                        Spender = blockchain.Value.SpenderAddress,
                        Owner = user.Address
                    })
                : user.Approved;

            var token = options.Value.Token
                ? await web3.Eth.ERC20
                    .GetContractService(blockchain.Value.TokenAddress)
                    .BalanceOfQueryAsync(new BalanceOfFunction
                    {
                        Owner = user.Address,
                    })
                : user.Token;

            if (gas != user.Gas || approved != user.Approved || token != user.Token)
            {
                logger.LogInformation("Updating user {User}.", user);
                await repository.Update(user with { Gas = gas, Approved = approved, Token = token });
            }
        }
    }

    public sealed record class Configuration : WorkConfigurationBase
    {
        public required bool Gas { get; init; }
        public required bool Token { get; init; }
        public required bool Approved { get; init; }
    }
}
