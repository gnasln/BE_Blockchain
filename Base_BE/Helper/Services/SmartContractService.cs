﻿using System.Numerics;
using Base_BE.Application.Common.Interfaces;
using Base_BE.Application.Dtos;
using Base_BE.Application.Vote.Commands;
using Base_BE.Dtos;
using Base_BE.Endpoints;
using Base_BE.Helper.key;
using Base_BE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using TransactionReceipt = Nethereum.RPC.Eth.DTOs.TransactionReceipt;

namespace Base_BE.Helper.Services
{

    public class SmartContractService
    {
        private readonly Web3 _web3;
        private readonly string contractAddress;
        private readonly ILogger<SmartContractService> _logger;
        private readonly ApplicationDbContext context;
        private readonly IUser user;

        public SmartContractService(IConfiguration configuration, ILogger<SmartContractService> logger)
        {

            var rpcUrl = configuration["Ethereum:RpcUrl"];
            var privateKey = configuration["Ethereum:PrivateKey"];
            var chainId = configuration.GetValue<long>("Ethereum:ChainId");
            contractAddress = configuration["Ethereum:ContractAddress"];
            _logger = logger;

            var credentials = new Account(privateKey, chainId);
            _web3 = new Web3(credentials, rpcUrl);
        }

        private Contract GetContract(string contractAddress)
        {
            // Replace with your smart contract ABI
            string abi = "[\n\t{\n\t\t\"inputs\": [],\n\t\t\"stateMutability\": \"nonpayable\",\n\t\t\"type\": \"constructor\"\n\t},\n\t{\n\t\t\"anonymous\": false,\n\t\t\"inputs\": [\n\t\t\t{\n\t\t\t\t\"indexed\": true,\n\t\t\t\t\"internalType\": \"address\",\n\t\t\t\t\"name\": \"voterAddress\",\n\t\t\t\t\"type\": \"address\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"indexed\": true,\n\t\t\t\t\"internalType\": \"uint256\",\n\t\t\t\t\"name\": \"ballotId\",\n\t\t\t\t\"type\": \"uint256\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"indexed\": false,\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"voteId\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"indexed\": false,\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"voterId\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t}\n\t\t],\n\t\t\"name\": \"BallotSubmitted\",\n\t\t\"type\": \"event\"\n\t},\n\t{\n\t\t\"inputs\": [\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"bitcoinAddress\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string[]\",\n\t\t\t\t\"name\": \"candidates\",\n\t\t\t\t\"type\": \"string[]\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"voterId\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"voteId\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"votedTime\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t}\n\t\t],\n\t\t\"name\": \"giveVotingRight\",\n\t\t\"outputs\": [],\n\t\t\"stateMutability\": \"nonpayable\",\n\t\t\"type\": \"function\"\n\t},\n\t{\n\t\t\"inputs\": [\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"voteId\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"voterId\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t}\n\t\t],\n\t\t\"name\": \"checkExistBallot\",\n\t\t\"outputs\": [\n\t\t\t{\n\t\t\t\t\"internalType\": \"bool\",\n\t\t\t\t\"name\": \"\",\n\t\t\t\t\"type\": \"bool\"\n\t\t\t}\n\t\t],\n\t\t\"stateMutability\": \"view\",\n\t\t\"type\": \"function\"\n\t},\n\t{\n\t\t\"inputs\": [\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"candidateId\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"voteId\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t}\n\t\t],\n\t\t\"name\": \"countBallotForCandidate\",\n\t\t\"outputs\": [\n\t\t\t{\n\t\t\t\t\"internalType\": \"uint256\",\n\t\t\t\t\"name\": \"\",\n\t\t\t\t\"type\": \"uint256\"\n\t\t\t}\n\t\t],\n\t\t\"stateMutability\": \"view\",\n\t\t\"type\": \"function\"\n\t},\n\t{\n\t\t\"inputs\": [\n\t\t\t{\n\t\t\t\t\"internalType\": \"address\",\n\t\t\t\t\"name\": \"voterAddress\",\n\t\t\t\t\"type\": \"address\"\n\t\t\t}\n\t\t],\n\t\t\"name\": \"getBallotVoterData\",\n\t\t\"outputs\": [\n\t\t\t{\n\t\t\t\t\"internalType\": \"uint256\",\n\t\t\t\t\"name\": \"\",\n\t\t\t\t\"type\": \"uint256\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string[]\",\n\t\t\t\t\"name\": \"\",\n\t\t\t\t\"type\": \"string[]\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t},\n\t\t\t{\n\t\t\t\t\"internalType\": \"string\",\n\t\t\t\t\"name\": \"\",\n\t\t\t\t\"type\": \"string\"\n\t\t\t}\n\t\t],\n\t\t\"stateMutability\": \"view\",\n\t\t\"type\": \"function\"\n\t}\n]";
            return _web3.Eth.GetContract(abi, contractAddress);
        }

        public async Task<TransactionReceipt> SubmitVoteAsync(SubmitVoteModel model)
        {
            try
            {
                var contract = GetContract(model.BitcoinAddress);
                var giveVotingRightFunction = contract.GetFunction("giveVotingRight");

                _logger.LogInformation($"Submitting vote for VoterId={model.VoterId}, VoteId={model.VoteId}");


                TransactionReceipt receipt = await giveVotingRightFunction.SendTransactionAndWaitForReceiptAsync(
                    from: _web3.TransactionManager.Account.Address,
                    gas: new HexBigInteger(3000000),
                    value: null,
                    receiptRequestCancellationToken: default(CancellationToken),
                    functionInput: new object[]
                    {
                        model.BitcoinAddress,
                        model.Candidates.ToArray(),
                        model.VoterId,
                        model.VoteId,
                        DateTime.UtcNow.ToString("o")
                    }

                );

                return receipt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting vote");
                throw;
            }
        }

        public async Task<int> CountBallotForCandidateAsync(string candidateId, string voteId)
        {
            try
            {
                var contract = GetContract(contractAddress);
                var function = contract.GetFunction("countBallotForCandidate");

                _logger.LogInformation($"Counting ballots for CandidateId={candidateId}, VoteId={voteId}");

                var result = await function.CallAsync<BigInteger>(candidateId.ToString(), voteId.ToString());
                return (int)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting ballots");
                throw;
            }
        }

        //public async Task<BallotVoterDto> GetBallotVoterAsync(string address)
        //{
        //    try
        //    {
        //        var contract = GetContract(address);
        //        var function = contract.GetFunction("getBallotVoterData");

        //        _logger.LogInformation($"Fetching ballot voter data for Address={address}");

        //        var result = await function.CallDeserializingToObjectAsync<BallotVoterDto>(address);


        //        return new BallotVoterDto
        //        {
        //            Id = (long)result.Item1,
        //            CandidateIds = result.Item2,
        //            VoterId = long.Parse(result.Item3),
        //            VotedTime = DateTime.Parse(result.Item4),
        //            VoteId = long.Parse(result.Item5),
        //            Address = result.Item6
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching ballot voter data");
        //        throw;
        //    }
        //}

        public async Task<bool> CheckExistBallotAsync(string voteId, string voterId)
        {
            try
            {
                var contract = GetContract(contractAddress);
                var function = contract.GetFunction("checkExistBallot");

                _logger.LogInformation($"Checking if ballot exists for VoteId={voteId}, VoterId={voterId}");

                var result = await function.CallAsync<bool>(voteId.ToString(), voterId.ToString());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of ballot");
                throw;
            }
        }
    }
}