using System;
using System.Linq;
using System.Threading.Tasks;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Util;
using Nethereum.Web3;

namespace NethClient
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var node = "http://localhost:8545";
                if (args.Length == 2)
                {
                    node = args[1];
                }

                var delay = TimeSpan.FromSeconds(5);
                Log($"Waiting {delay.TotalSeconds} seconds.");
                await Task.Delay(delay).ConfigureAwait(false);

                /*
                contract test {
                    uint _multiplier;
                    function test(uint multiplier){
                        _multiplier = multiplier;
                    }
                    function multiply(uint a) returns(uint d) {
                        return a * _multiplier;
                    }
                }
                 */
                var abi = @"[{'constant':false,'inputs':[{'name':'a','type':'int256'}],'name':'multiply','outputs':[{'name':'r','type':'int256'}],'type':'function'},{'inputs':[{'name':'multiplier','type':'int256'}],'type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'a','type':'int256'},{'indexed':true,'name':'sender','type':'address'},{'indexed':false,'name':'result','type':'int256'}],'name':'Multiplied','type':'event'}]";
                var contractByteCode = "0x6060604052604051602080610104833981016040528080519060200190919050505b806000600050819055505b5060ca8061003a6000396000f360606040526000357c0100000000000000000000000000000000000000000000000000000000900480631df4f144146037576035565b005b604b60048080359060200190919050506061565b6040518082815260200191505060405180910390f35b60006000600050548202905080503373ffffffffffffffffffffffffffffffffffffffff16827f841774c8b4d8511a3974d7040b5bc3c603d304c926ad25d168dacd04e25c4bed836040518082815260200191505060405180910390a380905060c5565b91905056";

                var senderAddress = "0xaffe3d426b8517b17b54290c882f0827f65ec187";
                var senderPassword = "password";

                Log($"Using geth node {node}");
                var web3 = new Web3(node);

                Log($"Unlocking account {senderAddress}");
                var unlockAccountResult = await web3.Personal.UnlockAccount.SendRequestAsync(senderAddress, senderPassword, 1200).ConfigureAwait(false);
                Log($"-> Success: {unlockAccountResult}");

                var funds = await web3.Eth.GetBalance.SendRequestAsync(senderAddress).ConfigureAwait(false);
                Log($"Account {senderAddress} has {UnitConversion.Convert.FromWei(funds.Value)} eth");

                var gasPrice = await web3.Eth.GasPrice.SendRequestAsync().ConfigureAwait(false);
                Log($"Gas price is {gasPrice.Value} wei");

                var multiplier = 7;
                var gasDeploy = await web3.Eth.DeployContract.EstimateGasAsync(abi, contractByteCode, senderAddress, multiplier).ConfigureAwait(false);

                Log($"Deploying contract with multiplier {multiplier} using {gasDeploy.Value} gas");
                var receipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(abi, contractByteCode, senderAddress, gasDeploy, null, multiplier).ConfigureAwait(false);
                Log($"-> Done at block {receipt.BlockNumber.Value} using {receipt.CumulativeGasUsed.Value} gas");
                Log($"-> Contract address is {receipt.ContractAddress}");

                var contract = web3.Eth.GetContract(abi, receipt.ContractAddress);

                var multiplyFunction = contract.GetFunction("multiply");

                var gas7 = await multiplyFunction.EstimateGasAsync(7).ConfigureAwait(false);
                Log($"Multiply 7 by {multiplier} using {gas7.Value} gas");
                var receipt7 = await multiplyFunction.SendTransactionAndWaitForReceiptAsync(senderAddress, gas7, null, null, 7).ConfigureAwait(false);
                Log($"-> Done at block {receipt7.BlockNumber.Value} using {receipt7.CumulativeGasUsed.Value} gas");

                var multiplyEvent = contract.GetEvent("Multiplied");

                Log($"Creating filter for all events");
                var filterAll = await multiplyEvent.CreateFilterAsync().ConfigureAwait(false);

                var gas8 = await multiplyFunction.EstimateGasAsync(8).ConfigureAwait(false);
                Log($"Multiply 8 by {multiplier} using {gas8.Value} gas");
                var receipt8 = await multiplyFunction.SendTransactionAndWaitForReceiptAsync(senderAddress, gas8, null, null, 8).ConfigureAwait(false);
                Log($"-> Done at block {receipt7.BlockNumber.Value} using {receipt7.CumulativeGasUsed.Value} gas");
                
                Log("Get all events");
                var log = await multiplyEvent.GetFilterChanges<MultipliedEvent>(filterAll).ConfigureAwait(false);
                Log($"-> Got {log.Count}");

                foreach (var evt in log)
                {
                    Log($"-> Block {evt.Log.BlockNumber.Value} : {evt.Event.MultiplicationInput} * {multiplier} = {evt.Event.Result}");
                }

                Log("Done. Exiting.");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private static void Log(string text) => Console.WriteLine(text);
    }

    public class MultipliedEvent
    {
        [Parameter("int", "a", 1, true)]
        public int MultiplicationInput { get; set; }

        [Parameter("address", "sender", 2, true)]
        public string Sender { get; set; }

        [Parameter("int", "result", 3, false)]
        public int Result { get; set; }
    }
}