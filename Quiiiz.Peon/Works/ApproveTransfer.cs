using System.Numerics;
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
            foreach (var user in repository.Content)
            {
                if (user.Approved == 0)
                {
                    var account = new Wallet(options.Value.Users.Seed, options.Value.Users.Password).GetAccount((int)user.Id, options.Value.ChainId);
                    var web3 = new Web3(account, options.Value.Node.ToString());

                    web3.Eth.TransactionManager.UseLegacyAsDefault = true;

                    var value = await GetApprove(web3);

                    if (value == 0)
                    {
                        var receipt = await web3.Eth.ERC20
                            .GetContractService(options.Value.TokenAddress)
                            .ApproveRequestAndWaitForReceiptAsync(new ApproveFunction
                            {
                                Spender = options.Value.SpenderAddress,
                                Value = long.MaxValue
                            }, cancellationToken);

                        if (!receipt.Succeeded()) logger.LogError("Approve transaction failed {Hash}.", receipt.TransactionHash);
                    }

                    value = await GetApprove(web3);

                    var updated = user with { Approved = value };

                    await repository.Update(updated);
                }
            }

            async Task<BigInteger> GetApprove(Web3 web3) 
                => await web3.Eth.ERC20.GetContractService(options.Value.TokenAddress)
                    .AllowedQueryAsync(new AllowedFunction { Spender = options.Value.SpenderAddress, Owner = web3.TransactionManager.Account.Address });
        }
    }
}
