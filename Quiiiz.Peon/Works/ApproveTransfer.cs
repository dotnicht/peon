using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.HdWallet;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;
using ThirdParty.BouncyCastle.Math;

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

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            const string token = "0x0d551A447fB712D84E430801d20e2EB97f44ad4C";

            foreach (var user in repository.Content)
            {
                if (user.Approved == 0)
                {
                    var account = new Wallet(options.Value.Seed, options.Value.Password).GetAccount((int)user.Id, options.Value.ChainId);
                    var web3 = new Web3(account, options.Value.Node.ToString());

                    web3.Eth.TransactionManager.UseLegacyAsDefault = true;

                    var value = await web3.Eth.ERC20.GetContractService(token)
                        .AllowedQueryAsync(new AllowedFunction { Spender = options.Value.SpenderAddress, Owner = account.Address });

                    if (value == 0)
                    {
                        var receipt = await web3.Eth.ERC20
                            .GetContractService(token)
                            .ApproveRequestAndWaitForReceiptAsync(new ApproveFunction
                                {
                                    Spender = options.Value.SpenderAddress,
                                    Value = long.MaxValue
                                }, cancellationToken);

                        if (!receipt.Succeeded()) logger.LogError("Approve transaction failed {Hash}.", receipt.TransactionHash);
                    }

                    value = await web3.Eth.ERC20.GetContractService(token)
                        .AllowedQueryAsync(new AllowedFunction { Spender = options.Value.SpenderAddress, Owner = account.Address });

                    var updated = user with { Approved = value };

                    await repository.Update(updated);
                }
            }
        }
    }
}
