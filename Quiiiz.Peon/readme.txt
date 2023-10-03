Quiiiz.Peon is a hosted service console dotnet application. 
It's configured using "appsettings.json" file. 
It's capable of performing 5 different types of jobs called Works. 
It's execution worklow is determined by command line arguments. 
Most typical run scenarious are predetermined in "launchSettings.json" (useful for debugging in Visual Studio).

Usage.

For example, we want to create a worklow that will generate addresses for the users and prefill them with gas. 
On Windows machine we will run:

Quiiiz.Peon.exe check fill

where ""Quiiiz.Peon.exe"" is the executable, "check" is the name of the work responsible for generating address, while "fill" is the work filling the gas.

We can stack as many tasks as need, chaining them into long runnning worflows. 

For example running 

Quiiiz.Peon.exe check sync fill sync allow sync extract sync

will result in a workflow that:
    1. generates addresses, 
    2. fills the gas, 
    3. performs approve transactions on behalf of address, 
    4. extracts the token and gas from the address
and has additional database synchonization command after each step. 

Configuration.

