using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiiiz.Peon.Provider;

internal class Address : IAddressProvider
{
    public Task<string> GetDepositAddress(long userId)
    {
        throw new NotImplementedException();
    }
}
