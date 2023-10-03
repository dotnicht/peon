
# Description 

Quiiiz.Peon is a hosted service console dotnet application. 
It's configured using "appsettings.json" file. 
It's capable of performing 5 different types of jobs called Works. 
It's execution workflow is determined by command line arguments. 
Most typical run scenarios are predetermined in "launchSettings.json" (useful for debugging in Visual Studio).

# Usage.

For example, we want to create a workflow that will generate addresses for the users and prefill them with gas. 
On Windows machine we will run:

**Quiiiz.Peon.exe check fill**

where **Quiiiz.Peon.exe**" is the executable, "**check**" is the name of the work responsible for generating address, while "**fill**" is the work filling the gas.

We can stack as many tasks as need, chaining them into long running workflows. 

For example running 

**Quiiiz.Peon.exe check sync fill sync allow sync extract sync**

will result in a workflow that:
    1. generates addresses, 
    2. fills the gas, 
    3. performs approve transactions on behalf of address, 
    4. extracts the token and gas from the address
and has additional database synchronization command after each step. 

# Configuration.

Configuration file is in JSON format and has 4 root configuration sections. 

* **Logging**: standard dotnet application section to configure console output verbosity. 
* **Database**: MongoDB configuration with database server connection string and database name.
* **Blockchain**: contains common blockchain-related settings. 
* **Works**: contains workflow execution parameters and work specific configuration. 

## Blockchain

Blockchain network connectivity is managed by 2 parameters: 
* **Node**: EVM-compatible node RPC address in URI format.
* **ChainId**: the ID of the blockchain.

ER20-related settings include:
* **TokenAddress**: the address of ERC20-compatible smart contract to be used as token.
* **SpenderAddress**: the address of the account that will be granted with token spending permissions and checked allowance. 

Security settings:
* **MasterIndex**: signed integer based index that will represent master account (the account that will initially hold the gas to be distributed among generated addresses). 
* **Users**: contains seed and password pair for root HD wallet that all generated addresses will derive from. 
* **Master**: contains seed and password pair for master account (only subaccount by MasterIndex is used). 

## Works 

Works execution is controlled by 3 parameters:
* **Loop**: bool value determining whether to loop workflow execution, **false** to run workflow once. 
* **Exceptions**: bool value determining whether to propagate exceptions occurred within work execution, if set to **true**, every exception occurred while executing the particular work will result in entire workflow termination. 
* **Timeout**: timeout value to wait after workflow execution, useful to put a pause between workflow runs in loop-enabled scenario. 

Every work has it's own unique one-word name and configuration subsections named respectively.
### Check

This work creates records in database "**user**" collection. The user id values are signed identity integers starting from offset value. Work parameters:
* **Offset**: starting index offset.
* **UsersNumber**: the amount of records to generate.

### Fill

This works sends gas currency from master account to the addresses existing in the database. Only users with empty gas balance are subject to this operation. Work parameters:
* Amount: decimal value of gas currency to send.

### Allow
This work executes approve transaction of ERC20 compatible contract on behalf of addresses generated and stored in database. Work parameters:
* **Amount**: signed integer value to pass to approve transaction. 
* **Refresh**: bool value indication whether to automatically update database with the value available upon successful transaction execution. 

### Extract
This work extracts gas and tokens from generated addresses. Parameters are grouped in 2 sections for gas and token extraction respectively. Every section contains parameters:
* **Extract**: bool value indicating whether to perform extract for gas (token).  
* **Address**: string with EVM-compatible address where to extract gas (token)
* **Refresh**: bool value indication whether to automatically update database with the value available upon successful transaction execution. 

### Sync
This work is responsible for keeping database in sync with the blockchain. Every users gas, token and allowance balances are updated. Work can be configured to update only subset of users properties, e.g. only gas or token. Work parameters:
* **Gas**: bool value indicating whether to perform sync for gas.
* **Token**: bool value indicating whether to perform sync of token.
* **Approved**: bool value indicating whether to perform sync of approved.