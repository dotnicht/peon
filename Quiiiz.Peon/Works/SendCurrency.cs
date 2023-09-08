using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.HdWallet;
using Nethereum.Util;
using Nethereum.Web3;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;
using Quiiiz.Peon.Persistence;
using RecurrentTasks;

namespace Quiiiz.Peon.Works
{
    internal class SendCurrency : IRunnable
    {
        private readonly ILogger logger;
        private readonly IRepository<User> repository;
        private readonly IOptions<Blockchain> options;

        public SendCurrency(ILogger<SendCurrency> logger, IRepository<User> repository, IOptions<Blockchain> options)
        {
            this.logger = logger;
            this.repository = repository;
            this.options = options;
        }

        public async Task RunAsync(ITask currentTask, IServiceProvider scopeServiceProvider, CancellationToken cancellationToken)
        {
            var account = new Wallet(options.Value.Seed, options.Value.Password).GetAccount(default, options.Value.ChainId);
            var web3 = new Web3(account, options.Value.Node.ToString());

            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);

            if (balance.Value.IsZero) throw new InvalidOperationException("Empty root account balance.");

            var price = await web3.Eth.GasPrice.SendRequestAsync();
            var converted = (decimal)Web3.Convert.FromWeiToBigDecimal(price.Value, UnitConversion.EthUnit.Gwei);

            foreach (var user in repository.Content)
            {
                if (user.Balance == default)
                {
                    balance = await web3.Eth.GetBalance.SendRequestAsync(user.Address);

                    var updated = user with { Balance = balance.Value };

                    if (balance == default)
                    {
                        logger.LogInformation("User {UserId} with empty balance at address {Address} detected.", user.Id, user.Address);

                        var tx = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(user.Address, 1, converted, 1000000, null, cancellationToken);

                        logger.LogInformation("Sending transaction {Hash}.", tx.TransactionHash);

                        balance = await web3.Eth.GetBalance.SendRequestAsync(user.Address);

                        updated = user with { Balance = balance.Value };
                    }

                    await repository.Update(updated);
                }
            }
        }
    }
}
