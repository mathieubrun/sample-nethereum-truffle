using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace NethClient.Contracts.Multiplier
{
    public class MultiplierContract
    {
        private readonly Contract _contract;
        private readonly Function _multiply;
        private readonly Event _multiplied;

        private MultiplierContract(Contract contract)
        {
            _contract = contract;
            _multiply = contract.GetFunction("multiply");
            _multiplied = contract.GetEvent("Multiplied");
        }

        public static async Task<TransactionReceipt> DeployContract(Web3 web3, string senderAddress, string abi, string bytecode)
        {
            var gasDeploy = await web3.Eth.DeployContract.EstimateGasAsync(abi, bytecode, senderAddress).ConfigureAwait(false);

            Log($"Deploying contract using {gasDeploy.Value} gas");
            var receipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(abi, bytecode, senderAddress, gasDeploy, null).ConfigureAwait(false);
            Log($"-> Done at block {receipt.BlockNumber.Value} using {receipt.CumulativeGasUsed.Value} gas");
            Log($"-> Contract address is {receipt.ContractAddress}");

            return receipt;
        }
        
        public static MultiplierContract GetContractAt(Web3 web3, string abi, string address)
        {
            Log($"Getting contract at {address}");
            var contract = web3.Eth.GetContract(abi, address);

            var multiplyContract = new MultiplierContract(contract);

            return multiplyContract;
        }

        public async Task<HexBigInteger> CreateFilter()
        {
            Log($"Creating filter for all events");
            var filter = await _multiplied.CreateFilterAsync().ConfigureAwait(false);
            Log($"-> Created {filter.Value}");

            return filter;
        }

        public async Task<List<EventLog<MultipliedEvent>>> GetFilterChanges(HexBigInteger filterId)
        {
            Log($"Get all events for filter {filterId.Value}");
            var res = await _multiplied.GetFilterChanges<MultipliedEvent>(filterId).ConfigureAwait(false);
            Log($"-> Got {res.Count}");

            return res;
        }

        public async Task<TransactionReceipt> Multiply(string from, int multiplicand, int multiplier)
        {
            var gas = await _multiply.EstimateGasAsync(multiplicand, multiplier).ConfigureAwait(false);
            Log($"Multiply {multiplicand} by {multiplier} using {gas.Value} gas");

            var receipt = await _multiply.SendTransactionAndWaitForReceiptAsync(from, gas, null, null, multiplicand, multiplier).ConfigureAwait(false);
            Log($"-> Done at block {receipt.BlockNumber.Value} using {receipt.CumulativeGasUsed.Value} gas");

            return receipt;
        }

        private static void Log(string text) => Console.WriteLine(text);
    }
}