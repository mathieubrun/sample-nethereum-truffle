using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NethClient.Contracts.Multiplier;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

                var json = JObject.Load(new JsonTextReader(new StreamReader(File.OpenRead("Contracts/Multiplier/Multiplier.json"))));
                var abi = json["abi"].ToString();
                var bytecode = json["bytecode"].ToString();

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

                var receipt = await MultiplierContract.DeployContract(web3, senderAddress, abi, bytecode).ConfigureAwait(false);
                var multiplyContract = MultiplierContract.GetContractAt(web3, abi, receipt.ContractAddress);

                var filterAll = await multiplyContract.CreateFilter().ConfigureAwait(false);

                await multiplyContract.Multiply(senderAddress, 7, 7).ConfigureAwait(false);
                await multiplyContract.Multiply(senderAddress, 8, 9).ConfigureAwait(false);

                var changes = await multiplyContract.GetFilterChanges(filterAll).ConfigureAwait(false);
                foreach (var evt in changes)
                {
                    Log($"-> Block {evt.Log.BlockNumber.Value} : {evt.Event.Multiplicand} * {evt.Event.Multiplier} = {evt.Event.Product}");
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
}