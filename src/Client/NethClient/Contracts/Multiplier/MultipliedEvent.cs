using Nethereum.ABI.FunctionEncoding.Attributes;

namespace NethClient.Contracts.Multiplier
{
    public class MultipliedEvent
    {
        [Parameter("uint256", "multiplicand", 1, true)]
        public int Multiplicand { get; set; }

        [Parameter("uint256", "multiplier", 2, true)]
        public int Multiplier { get; set; }

        [Parameter("address", "sender", 3, true)]
        public string Sender { get; set; }

        [Parameter("uint256", "product", 4, false)]
        public int Product { get; set; }
    }
}