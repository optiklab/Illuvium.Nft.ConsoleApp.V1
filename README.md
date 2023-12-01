# CLI console app to work with NFT Wallet

Data structures that I have used are strictly linked for the Task. My solution is focused on the ability to efficiently execute 3 types of tasks:
- Persist information about ownership relationshop between imaginary NFT token ids and NFT wallet addresses provided in the task
- Quickly answer what wallet contains a token, by token Id. 
- Quickly answer what tokens are owned by certain wallet.
- Efficiently change the ownership of the Token between the wallet addresses.

I also tried to create the solution to fit the tight deadlines.

There are also a few things I skip and maybe return to it later. For example, if we need data integrity validation, I would probably go with some other ideas (like Merkle Trees used in the industry).

# Prerequisites

To build the solution you need to have .NET Core 6.0 SDK installed.

To open and review the source code it's best to have:
- Visual Studio 2022
- .NET Core 6.0

# How to run

## Windows

>dotnet build
>dotnet run

The application will show you a general help how to use it and list the available commands (see below in this instruction).

# Commands

>Illuvium.Nft.App --help
Description:
  Illuvium app to work with NFT tokens.

Usage:
  Illuvium.Nft.App [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  --read-file <file>    Reads transactions from the ?le in the speci?ed location.
  --read-inline <json>  Reads either a single json element, or an array of json elements representing transactions as an argument.
  --nft <tokenId>       Returns ownership information for the nft with the given id.
  --wallet <Address>    Lists all NFTs currently owned by the wallet of the given address.
  --reset               Deletes all data previously processed by the program.

# How it works (Example)

There is transactions.json comes as an example file with list of transactions to quickly resume the state (for testing). Execute following commands:

>Illuvium.Nft.App --read-file transactions.json 
Read 5 transaction(s) 

>Illuvium.Nft.App --nft 0xA000000000000000000000000000000000000000
Token 0xA000000000000000000000000000000000000000 is not owned by any wallet 

>Illuvium.Nft.App --nft 0xB000000000000000000000000000000000000000
Token 0xA000000000000000000000000000000000000000 is owned by 0x3000000000000000000000000000000000000000 

>Illuvium.Nft.App --nft 0xC000000000000000000000000000000000000000
Token 0xC000000000000000000000000000000000000000 is owned by 0x3000000000000000000000000000000000000000 

>Illuvium.Nft.App --nft 0xD000000000000000000000000000000000000000
Token 0xA000000000000000000000000000000000000000 is not owned by any wallet 

>Illuvium.Nft.App --read-inline  "{ \"Type\": \"Mint\", \"TokenId\": \"0xD000000000000000000000000000000000000000\", \"Address\": \"0x1000000000000000000000000000000000000000\" }"
Read 1 transaction(s) 

>Illuvium.Nft.App --nft 0xD000000000000000000000000000000000000000
Token 0xA000000000000000000000000000000000000000 is owned by 0x1000000000000000000000000000000000000000 

>Illuvium.Nft.App --wallet 0x3000000000000000000000000000000000000000
Wallet 0x3000000000000000000000000000000000000000 holds 2 Tokens: 
0xB000000000000000000000000000000000000000 
0xC000000000000000000000000000000000000000 

>Illuvium.Nft.App -—reset 
Program was reset 

>Illuvium.Nft.App --wallet 0x3000000000000000000000000000000000000000
Wallet 0x3000000000000000000000000000000000000000 holds no Tokens 

# External dependencies

## For running app

- .NET Core 6.0
- System.CommandLine (it is still in beta, but it exists for a long time and allow to easily build Command Line tools)
- Newtonsoft.Json (to work with JSON)
- Newtonsoft.Json.Schema (to validate JSON Schema - IT'S NOT FREE for production apps)

## For running tests using Visual Studio

dotnet add package FluentAssertions
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
dotnet add package xunit
dotnet add package xunit.runner.visualstudio

# Author

Anton Yarkov (C) 2023
anton.yarkov@gmail.com
+34 662 798 010
https://optiklab.github.io/
